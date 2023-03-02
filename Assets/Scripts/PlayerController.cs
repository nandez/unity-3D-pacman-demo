using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected Waypoint waypoint;

    private bool isMoving = false;
    private Vector3 movingDirection = Vector3.zero;
    private Vector3 inputDir = Vector3.zero;

    public Waypoint GetWaypoint() => waypoint;
    public float GetMoveSpeed() => moveSpeed;

    //private Waypoint lastWaypoint;
    //private Vector3 inputDirection = Vector3.zero;
    //private Vector3 lastDirection = Vector3.zero;

    public delegate void InitialMoveEvent();
    public static event InitialMoveEvent OnPlayerInitialMove;
    private bool initialMoveDone = false;

    void Start()
    {

    }


    void Update()
    {
        /*
            Vamos a dividir el movimiento en dos partes:

            1.  Si el jugador no se está moviendo, obtenemos el input del usuario y lo procesamos
                para determinar la dirección del posible movimiento, verificando si existe un waypoint
                al cual navegar en esa dirección.

            2.  Si el jugador se está moviendo, verificamos si ya llegó al waypoint al cual se
                estaba moviendo y si es así, lo detenemos, para luego volver al paso 1.
        */

        // Obtenemos el input del usuario para determinar la dirección del posible movimiento.
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
            }
            else
            {
                // Movemos al jugador en la dirección indicada.
                transform.position += movingDirection * moveSpeed * Time.deltaTime;
            }
        }
    }
}
