using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected Waypoint currentWaypoint;


    [Header("Behavior Settings")]
    [SerializeField] protected float chaseRange = 10f;
    [SerializeField] Waypoint homeEntranceWp;
    [SerializeField] Transform homePlace;
    [SerializeField] Material frightenedMaterial;


    [Header("Debug Settings")]
    [SerializeField] protected bool renderMovementPath = false;
    [SerializeField] protected bool renderChaseRangeGizmo = false;
    [SerializeField] protected KeyCode keyToDebugStateChange = KeyCode.Space;
    [SerializeField] protected EnemyState stateToSwitchTo = EnemyState.Scatter;

    // Velocidades de movimiento para cada estado.
    private float chaseSpeed;
    private float scatterSpeed;
    private float frightenedSpeed;
    private float eatenSpeed;

    private PathController pathCtrl;
    private PlayerController playerCtrl;
    private List<Waypoint> wpPath = new List<Waypoint>();
    private Waypoint targetWaypoint;
    private Material defaultMaterial;

    [SerializeField] EnemyState state = EnemyState.Scatter;

    void Start()
    {
        pathCtrl = new PathController();
        playerCtrl = FindObjectOfType<PlayerController>();

        // Obtenemos el material por defecto del enemigo y lo guardamos.
        defaultMaterial = GetComponent<Renderer>().material;

        // Dado que la velocidad del enemigo para cada estado
        // depende de la velocidad del jugador, la inicializamos en el Start.
        InitializeMovementSpeeds();
    }


    void Update()
    {
        if (Input.GetKeyDown(keyToDebugStateChange))
            ChangeState(stateToSwitchTo);

        // TODO: investigar que posibilidad hay de utilizar una state machine
        // para el comportamiento de los enemigos, separando cada behavior en un estado diferente.

        // Verificamos el estado del enemigo.
        EnemyShouldChasePlayer();

        if (state == EnemyState.Eaten)
            RunEatenBehavior();

        else if (state == EnemyState.Frightened)
            RunFrightenedBehavior();

        else if (state == EnemyState.Scatter)
            RunScatterBehavior();

        else if (state == EnemyState.Chase)
            RunChaseBehavior();
    }

    protected void EnemyShouldChasePlayer()
    {
        // Cuando el enemigo fue comido por el jugador o se encuentra huyendo, no realizamos ninguna acción.
        if (state == EnemyState.Eaten || state == EnemyState.Frightened)
            return;

        // Verificamos la distancia entre el enemigo y el jugador, y actualizamos el estado en consecuencia.
        // Si el mismo se encuentra en el rango de persecución, cambiamos el estado a Chase, y si no, a Scatter.
        var distanceToPlayer = Vector3.Distance(transform.position, playerCtrl.transform.position);
        state = distanceToPlayer <= chaseRange ? EnemyState.Chase : EnemyState.Scatter;
    }

    protected void HandleMovement(float speed)
    {
        // Buscamos el camino más corto entre el waypoint del enemigo actual y el waypoint al que queremos ir.
        wpPath = pathCtrl.FindPath(currentWaypoint, targetWaypoint);

        if (renderMovementPath)
            foreach (var wp in wpPath)
                Debug.DrawLine(transform.position, wp.transform.position, Color.red);

        if (wpPath?.Count > 0)
        {
            // Obtenemos el primer waypoint del camino y calculamos la distancia entre el enemigo y el waypoint,
            // proyectando el waypoint sobre el plano horizontal en la misma altura que el enemigo.
            var nextWaypoint = wpPath[0];
            var wpProjection = new Vector3(nextWaypoint.transform.position.x, transform.position.y, nextWaypoint.transform.position.z);
            var wpDistance = Vector3.Distance(transform.position, wpProjection);

            if (wpDistance <= nextWaypoint.distanceThreshold)
            {
                // En este caso, llegamos al waypoint, por lo que lo guardamos para
                // utilizarlo luego en el cálculo del path para el siguiente frame, y lo quitamos
                // de la lista de waypoints del camino.
                currentWaypoint = nextWaypoint;
                wpPath.RemoveAt(0);
            }
            else
            {
                // Si la distancia es mayor al umbral, nos movemos hacia el waypoint.
                var wpDirection = (wpProjection - transform.position).normalized;
                transform.position += wpDirection.normalized * speed * Time.deltaTime;
            }
        }
    }

    protected void RunEatenBehavior()
    {
        // Seteamos el waypoint de destino
        targetWaypoint = homeEntranceWp;

        // Mientras no haya llegado, movemos al enemigo hacia el waypoint de entrada de la casa.
        if (currentWaypoint != targetWaypoint)
        {
            HandleMovement(eatenSpeed);
        }
        else
        {
            // Si ya llegó al waypoint de entrada de la casa, nos movemos hacia el punto de origen.
            if (transform.position != homePlace.position)
            {
                var dir = homePlace.position - transform.position;
                transform.rotation = Quaternion.LookRotation(dir);
                // Reducimos la velocidad de movimiento para lograr un efecto mas amigable.
                transform.position += dir.normalized * frightenedSpeed * Time.deltaTime;
            }
        }
    }

    protected void RunFrightenedBehavior()
    {
        // Cambiamos el material del enemigo para que se vea asustado.
        GetComponent<Renderer>().material = frightenedMaterial;

        Vector3 targetWaypointProjection;
        float targetWaypointDistance;
        bool acquireWaypoint = true;

        // Cuando el enemigo está huyendo, se debe mover hacia una esquina aleatoria.
        // Por lo tanto, obtenemos una esquina aleatoria y calculamos la distancia entre el enemigo y el waypoint,
        // proyectando el waypoint sobre el plano horizontal en la misma altura que el enemigo.
        // Si la distancia es menor al umbral, volvemos a obtener una esquina aleatoria.
        do
        {
            if (targetWaypoint == null)
                targetWaypoint = MapManager.Instance.GetRandomCornerWaypoint();

            targetWaypointProjection = new Vector3(targetWaypoint.transform.position.x, transform.position.y, targetWaypoint.transform.position.z);
            targetWaypointDistance = Vector3.Distance(transform.position, targetWaypointProjection);

            // Verificamos si el waypoint es válido, es decir, si la distancia entre el enemigo y el waypoint es mayor al umbral.
            // de no ser así, volvemos a obtener una esquina aleatorioa y repetir el proceso de validación.
            acquireWaypoint = targetWaypointDistance <= targetWaypoint.distanceThreshold;
            if (acquireWaypoint)
                targetWaypoint = MapManager.Instance.GetRandomCornerWaypoint();

        } while (acquireWaypoint);

        HandleMovement(frightenedSpeed);
    }

    protected void RunScatterBehavior()
    {
        // Restauramos el material del enemigo para cubrir los casos donde se transiciona
        // del estado frightened o eaten a scatter.
        GetComponent<Renderer>().material = defaultMaterial;

        Vector3 targetWaypointProjection;
        float targetWaypointDistance;
        bool acquireWaypoint = true;

        // En este estado, el enemigo se mueve hacia un waypoint aleatorio del mapa.
        // Verificamos la distancia entre el enemigo y el waypoint, proyectando el waypoint sobre el plano horizontal en la misma altura que el enemigo.
        // Si la distancia es menor al umbral, volvemos a obtener un waypoint aleatorio.
        do
        {
            if (targetWaypoint == null)
                targetWaypoint = MapManager.Instance.GetRandomWaypoint();

            targetWaypointProjection = new Vector3(targetWaypoint.transform.position.x, transform.position.y, targetWaypoint.transform.position.z);
            targetWaypointDistance = Vector3.Distance(transform.position, targetWaypointProjection);

            // Verificamos si el waypoint es válido, es decir, si la distancia entre el enemigo y el waypoint es mayor al umbral.
            // de no ser así, volvemos a obtener un waypoint aleatorio y repetir el proceso de validación.
            acquireWaypoint = targetWaypointDistance <= targetWaypoint.distanceThreshold;
            if (acquireWaypoint)
                targetWaypoint = MapManager.Instance.GetRandomWaypoint();

        } while (acquireWaypoint);

        HandleMovement(scatterSpeed);
    }

    protected void RunChaseBehavior()
    {
        // Restauramos el material del enemigo para cubrir los casos donde se transiciona
        // del estado frightened o eaten a scatter.
        GetComponent<Renderer>().material = defaultMaterial;

        // Seteamos el waypoint del jugador como el waypoint de destino.
        targetWaypoint = playerCtrl.GetWaypoint();

        // Movemos al enemigo hacia el waypoint del jugador.
        HandleMovement(chaseSpeed);
    }

    protected void InitializeMovementSpeeds()
    {
        // Calculamos las velocidades de movimiento en base
        // a la velocidad de movimiento del jugador.
        var playerSpeed = playerCtrl.GetMoveSpeed();

        chaseSpeed = playerSpeed * 0.75f; // ChaseSpeed --> 75% de la velocidad del jugador.
        scatterSpeed = playerSpeed * 0.75f; // ScatterSpeed --> 75% de la velocidad del jugador.
        frightenedSpeed = playerSpeed * 0.5f; // FrightenedSpeed --> 50% de la velocidad del jugador.
        eatenSpeed = playerSpeed * 1.5f; // EatenSpeed --> 150% de la velocidad del jugador.
    }

    protected void ChangeState(EnemyState newState)
    {
        // El enemigo puede transicionar de cualquier estado a cualquier otro estado,
        // con algunas condiciones especiales entre estados particulares, las cuales se
        // manejan en el método Run*Behavior correspondiente.

        // No obstante, un cambio de estado implica un nuevo objetivo en cuanto al movimiento
        // por lo que se resetea el waypoint de destino para que en base al nuevo estado, se
        // pueda calcular un nuevo camino.

        targetWaypoint = null;
        state = newState;
    }

    void OnTriggerEnter(Collider other)
    {
        // La única colisión que nos interesa es la colisión con el jugador.
        if (other.gameObject.CompareTag("Player"))
        {
            // Si el enemigo ya fue comido, no hacemos nada.
            if (state == EnemyState.Eaten)
                return;

            // Si el enemigo está huyendo, lo comemos.
            if (state == EnemyState.Frightened)
                ChangeState(EnemyState.Eaten);
        }
    }

    void OnDrawGizmos()
    {
        if (renderChaseRangeGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }
    }
}
