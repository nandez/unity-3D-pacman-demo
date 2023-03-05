using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected int points = 200;
    [SerializeField] Material material;
    [SerializeField] Material frightenedMaterial;

    [Header("Movement Settings")]
    [SerializeField] protected Waypoint currentWaypoint;


    [Header("Behavior Settings")]
    [SerializeField] protected float chaseRange = 10f;
    [SerializeField] Waypoint homeEntranceWp;
    [SerializeField] Transform homePlace;
    [SerializeField] float homeWaitTime = 2f;

    [SerializeField] protected float powerPelletFadeWarningFlashSpeed = 5f;
    [SerializeField] protected float powerPelletFadeWarningFlashDuration = 1f;


    [Header("Debug Settings")]
    [SerializeField] protected bool renderMovementPath = false;
    [SerializeField] protected bool renderChaseRangeGizmo = false;

    // Velocidades de movimiento para cada estado.
    private float chaseSpeed;
    private float scatterSpeed;
    private float frightenedSpeed;
    private float eatenSpeed;

    private PathController pathCtrl;
    private PlayerController playerCtrl;
    private List<Waypoint> wpPath = new List<Waypoint>();
    private Waypoint targetWaypoint;
    private EnemyState currentState = EnemyState.Scatter;
    private Coroutine changeColorOnPowerPelletFadeWarningCoroutine;
    private bool waitingInHome = true;
    private SkinnedMeshRenderer meshRenderer;

    public delegate void EnemyEatenEvent(int points);
    public static event EnemyEatenEvent OnEnemyEaten;

    void Start()
    {
        // Creamos el path controller y obtenemos una referencia al player controller.
        pathCtrl = new PathController();
        playerCtrl = FindObjectOfType<PlayerController>();

        // Seteamos el material por defecto.
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        meshRenderer.material = material;

        // Dado que la velocidad del enemigo para cada estado
        // depende de la velocidad del jugador, la inicializamos en el Start.
        InitializeMovementSpeeds();

        GameManager.Instance.OnPowerPelletStatusChange += OnPowerPelletStatusChange;
        GameManager.Instance.OnPowerPelletFadeWarning += OnPowerPelletFadeWarning;
    }



    void Update()
    {
        // Verificamos el estado del enemigo.
        CheckForPlayerToChase();

        if (currentState == EnemyState.Chase)
            RunChaseBehavior();

        else if (currentState == EnemyState.Scatter)
            RunScatterBehavior();

        else if (currentState == EnemyState.Frightened)
            RunFrightenedBehavior();

        else if (currentState == EnemyState.Eaten)
            RunEatenBehavior();
    }

    protected void CheckForPlayerToChase()
    {
        // Cuando el enemigo se encuentra huyendo, esta en estado eaten o fue
        // recientemente comido y está esperando para salir de la casa, no realizamos ninguna acción.
        if (currentState == EnemyState.Eaten || currentState == EnemyState.Frightened || waitingInHome)
            return;

        // Finalmente, verificamos la distancia entre el enemigo y el jugador, y actualizamos el estado en consecuencia.
        // Si el mismo se encuentra en el rango de persecución, cambiamos el estado a Chase, y si no, a Scatter.
        var distanceToPlayer = Vector3.Distance(transform.position, playerCtrl.transform.position);
        currentState = distanceToPlayer <= chaseRange ? EnemyState.Chase : EnemyState.Scatter;
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
                transform.rotation = Quaternion.LookRotation(wpDirection);
                transform.position += wpDirection.normalized * speed * Time.deltaTime;
            }
        }
    }

    protected void RunEatenBehavior()
    {
        // TODO: agregar la animación..

        // Seteamos el waypoint de destino
        targetWaypoint = homeEntranceWp;

        // Mientras no haya llegado, movemos al enemigo hacia el waypoint de entrada de la casa.
        if (currentWaypoint != targetWaypoint)
        {
            HandleMovement(eatenSpeed);
        }
        else
        {
            // Verificamos la distancia entre el enemigo y el punto de origen.
            var distanceToHomePosition = Vector3.Distance(transform.position, homePlace.position);
            if (distanceToHomePosition >= 0.1f)
            {
                var dir = homePlace.position - transform.position;
                transform.rotation = Quaternion.LookRotation(dir);
                transform.position += dir.normalized * frightenedSpeed * Time.deltaTime;
            }
            else
            {
                // Si llegó al punto de origen, seteamos el estado a Scatter y
                // ponemos en true la variable waitingInHome.
                waitingInHome = true;
                ChangeState(EnemyState.Scatter);
            }
        }
    }

    protected void RunFrightenedBehavior()
    {
        // Cambiamos el material del enemigo para que se vea asustado.
        meshRenderer.material = frightenedMaterial;

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
        meshRenderer.material = material;

        // Si el enemigo no está esperando en la casa, se mueve hacia un waypoint aleatorio del mapa.
        if (!waitingInHome)
        {
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
        else
        {
            // En este caso, el enemigo fue recientemente comido y está esperando en la casa para volver a salir.
            // por lo que lo movemos hacia el waypoint de salida de la casa y luego seteamos la variable waitingInHome en false.
            var homeEntranceWpProjection = new Vector3(homeEntranceWp.transform.position.x, transform.position.y, homeEntranceWp.transform.position.z);
            var homeEntranceWpDistance = Vector3.Distance(transform.position, homeEntranceWpProjection);
            if (homeEntranceWpDistance >= 0.1f)
            {
                // Mientras el enemigo no haya llegado al waypoint de salida de la casa, lo movemos hacia el.
                var dir = homeEntranceWpProjection - transform.position;
                transform.rotation = Quaternion.LookRotation(dir);
                transform.position += dir.normalized * frightenedSpeed * Time.deltaTime;
            }
            else
            {
                // Si terminó de salir de la casa, seteamos el waypoint actual para habilitar los calculos
                // del pathfinding e indicamos que ya esta listo para salir.
                currentWaypoint = homeEntranceWp;
                waitingInHome = false;
            }
        }
    }

    protected void RunChaseBehavior()
    {
        // Restauramos el material del enemigo para cubrir los casos donde se transiciona
        // del estado frightened o eaten a scatter.
        meshRenderer.material = material;

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
        currentState = newState;
    }

    private void OnPowerPelletStatusChange(bool active)
    {
        // Cuando el enemigo fue comido por el jugador, no realizamos ninguna acción.
        if (currentState == EnemyState.Eaten)
            return;

        // Este evento se dispara para notificar que el power pellet fue activado o desactivado.
        // En ambos casos, debemos desactivar la corutina que se encarga de cambiar el color del enemigo
        // cuando el power pellet está a punto de desaparecer. (ver OnPowerPelletFadeWarning)
        if (changeColorOnPowerPelletFadeWarningCoroutine != null)
            StopCoroutine(changeColorOnPowerPelletFadeWarningCoroutine);

        // Finalmente, cambiamos el estado dependiendo de si el power pellet fue activado o desactivado.
        ChangeState(active ? EnemyState.Frightened : EnemyState.Scatter);
    }

    private void OnPowerPelletFadeWarning()
    {
        // Este evento se dispara para notificar que quedan pocos segundos antes de que
        // el power pellet desaparezca.
        // En este punto, los enemigos pueden estar en 2 estados;
        //
        // - frightened: desde que el jugador activó el power pellet, el enemigo no ha sido comido.
        // - eaten: el enemigo fue comido por el jugador.
        //
        // Entonces, en el único estado que nos interesa hacer algo es cuando el enemigo está huyendo,
        // en cuyo caso, cambiamos su color hasta que el efecto del power pellet desaparezca.
        if (currentState == EnemyState.Frightened)
            changeColorOnPowerPelletFadeWarningCoroutine = StartCoroutine(PowerPelletFadeWarningFlashEffect());
    }

    protected IEnumerator PowerPelletFadeWarningFlashEffect()
    {
        // Este método se encarga de cambiar el color del material del enemigo
        // para simular un efecto de parpadeo, una vez que el power pellet emite la notificación
        // de alerta.
        Color originalColor = frightenedMaterial.color;
        while (true)
        {
            var t = Mathf.PingPong(Time.time * powerPelletFadeWarningFlashSpeed, powerPelletFadeWarningFlashDuration) / powerPelletFadeWarningFlashDuration;
            frightenedMaterial.color = Color.Lerp(originalColor, Color.clear, t);

            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // La única colisión que nos interesa es la colisión con el jugador.
        if (other.gameObject.CompareTag("Player"))
        {
            // Si el enemigo ya fue comido, no hacemos nada.
            if (currentState == EnemyState.Eaten)
                return;

            // Si el enemigo está huyendo, lo comemos.
            if (currentState == EnemyState.Frightened)
            {
                // Emitimos para notificar que el enemigo fue comido.
                OnEnemyEaten?.Invoke(points);
                ChangeState(EnemyState.Eaten);
            }
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
