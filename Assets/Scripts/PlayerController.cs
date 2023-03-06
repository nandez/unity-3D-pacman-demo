using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected Waypoint waypoint;
    [SerializeField] protected Animation playerAnimation;

    private bool isMoving = false;
    private bool isDead = false;
    private Vector3 movingDirection = Vector3.zero;
    private Vector3 inputDir = Vector3.zero;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Waypoint startingWaypoint;
    private string currentAnimation = Constants.Player.IDLE_ANIMATION;

    public Waypoint Waypoint
    {
        get { return waypoint; }
        set { waypoint = value; }
    }

    public float GetMoveSpeed() => moveSpeed;

    public delegate void InitialMoveEvent();
    public static event InitialMoveEvent OnPlayerInitialMove;
    private bool initialMoveDone = false;

    public delegate void OnPlayerDeathEvent();
    public static event OnPlayerDeathEvent OnPlayerDeath;


    void Start()
    {
        // Guardamos la posición inicial del jugador y el waypoint en el que se encuentra.
        startingWaypoint = waypoint;
        startingPosition = transform.position;
        startingRotation = transform.rotation;

        // Nos suscribimos al evento de reseteo de nivel para poder reiniciar el jugador.
        GameManager.Instance.OnLevelReset += GameManager_OnLevelReset;
    }

    void Update()
    {
        // Verificamos el estado del jugador.
        // Si fue comido por un fantasma, entonces verificamos si
        // el jugador presiona una tecla de dirección para iniciar el juego.
        // En dicho caso, la variable isDead queda en false.
        CheckForDeadState();

        // Si la flag está en true, entonces el jugador aún no ha presionado
        // una tecla de dirección para iniciar el juego, por lo que no hacemos nada.
        if (isDead)
            return;

        // Llegado a este punto, vamos a dividir el movimiento en dos partes:
        //
        // 1.  Si el jugador no se está moviendo, obtenemos el input del usuario y lo procesamos
        //     para determinar la dirección del posible movimiento, verificando si existe un waypoint
        //     al cual navegar en esa dirección.
        //
        // 2.  Si el jugador se está moviendo, verificamos si ya llegó al waypoint al cual se
        //     estaba moviendo y si es así, lo detenemos, para luego volver al paso 1.

        // Verificamos el input del jugador.
        if (Input.GetKeyDown(KeyCode.UpArrow))
            inputDir = Vector3.forward;

        else if (Input.GetKeyDown(KeyCode.DownArrow))
            inputDir = Vector3.back;

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            inputDir = Vector3.left;

        else if (Input.GetKeyDown(KeyCode.RightArrow))
            inputDir = Vector3.right;

        if (!isMoving)
        {
            // Validamos si es posible moverse en la dirección indicada, buscando el waypoint adyacente
            // al actual en la dirección indicada.
            var nextWaypoint = waypoint.GetNeighborByDirection(inputDir);

            // Si no se encontró un waypoint porque no se especificó dirección alguna, entonces
            // verificamos si aun podemos movernos en la dirección en la que nos movimos por última vez.
            if (nextWaypoint == null && inputDir != movingDirection)
            {
                inputDir = movingDirection;
                nextWaypoint = waypoint.GetNeighborByDirection(inputDir);
            }


            if (nextWaypoint != null)
            {
                // Si es posible moverse en la dirección indicada, asignamos el waypoint al cual se
                // moverá el jugador y la dirección en la que se moverá.
                waypoint = nextWaypoint;
                movingDirection = inputDir;

                // Verificamos si es el primer movimiento
                // y emitimos el evento para notificar el inicio de la partida.
                if (!initialMoveDone)
                {
                    initialMoveDone = true;
                    OnPlayerInitialMove?.Invoke();
                }

                // Iniciamos el movimiento.
                isMoving = true;
            }
        }
        else
        {
            var wpProjection = new Vector3(waypoint.transform.position.x, transform.position.y, waypoint.transform.position.z);
            var wpDistance = Vector3.Distance(transform.position, wpProjection);

            if (wpDistance <= waypoint.distanceThreshold)
            {
                // Detenemos el movimiento.
                isMoving = false;

                if (currentAnimation != Constants.Player.IDLE_ANIMATION)
                {
                    currentAnimation = Constants.Player.IDLE_ANIMATION;
                    playerAnimation.CrossFade(Constants.Player.IDLE_ANIMATION);
                }
            }
            else
            {
                // Movemos al jugador en la dirección indicada.
                transform.rotation = Quaternion.LookRotation(movingDirection);
                transform.position = Vector3.MoveTowards(transform.position, wpProjection, moveSpeed * Time.deltaTime);

                // Verificamos si es necesario cambiar la animación.
                if (currentAnimation != Constants.Player.RUN_ANIMATION)
                {
                    currentAnimation = Constants.Player.RUN_ANIMATION;
                    playerAnimation.CrossFade(Constants.Player.RUN_ANIMATION);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // En este punto, nos interesa solamente la colisión con un enemigo
        // cuando el jugador no tiene un power pellet activo, dado que en ese
        // caso, el jugador pierde una vida.
        if (other.gameObject.CompareTag("Enemy") && !GameManager.Instance.IsPowerPelletActive && !isDead)
        {
            isDead = true;
            currentAnimation = Constants.Player.DEATH_ANIMATION;
            playerAnimation.CrossFade(Constants.Player.DEATH_ANIMATION);

            // Emitimos el evento para notificar la muerte del jugador luego de un segundo.
            // para permitir que la animación de muerte se reproduzca.
            Invoke(nameof(EmitOnPlayerDeathEvent), 1f);
        }
    }

    private void EmitOnPlayerDeathEvent()
    {
        OnPlayerDeath?.Invoke();
    }

    private void GameManager_OnLevelReset()
    {
        // Cuando se reinicia el nivel, el jugador debe volver a su estado inicial.

        // Reseteamos la animación
        currentAnimation = Constants.Player.IDLE_ANIMATION;
        playerAnimation.CrossFade(Constants.Player.IDLE_ANIMATION);

        // Reiniciamos el waypoint y el estado de movimiento.
        waypoint = startingWaypoint;
        initialMoveDone = false;
        isMoving = false;
        inputDir = Vector3.zero;

        // Reseteamos la posición y rotación del jugador
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }

    protected void CheckForDeadState()
    {
        // En este caso en particular, el jugador puede volver a la vida
        // si presiona una tecla de dirección mientras está muerto y el juego
        // se encuentra en estado Idle.
        if (isDead && GameManager.Instance.GameState == GameState.Idle)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                inputDir = Vector3.forward;
                isDead = false;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                inputDir = Vector3.back;
                isDead = false;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                inputDir = Vector3.left;
                isDead = false;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                inputDir = Vector3.right;
                isDead = false;
            }
        }
    }
}
