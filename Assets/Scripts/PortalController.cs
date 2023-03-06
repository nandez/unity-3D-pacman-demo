using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] protected PortalController exitPortal;
    [SerializeField] protected Waypoint waypoint;

    public Waypoint GetWaypoint() => waypoint;

    [SerializeField] protected ParticleSystem portalParticles;
    [SerializeField] protected float portalEffectDuration = 2f;

    // En esta lista guardamos una referencia a los objetos que vamos a recibir en el portal.
    private List<GameObject> incomingObjects = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        // Solo nos interesa que entren los jugadores y los enemigos.
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (incomingObjects.Contains(other.gameObject))
            {
                // En este caso, estamos procesando un objeto que entró desde otro portal y llegó a este.
                // Por lo tanto, ejecutamos la animación de salida del portal (si es que no se está ejecutando ya).
                // y luego en el OnTriggerExit lo removemos de la lista.
                if (!portalParticles.isPlaying)
                    StartCoroutine(PlayPortalEffect());
            }
            else
            {
                // En este caso, estamos procesando un objeto que entra por primera vez al portal, por lo tanto
                // debemos agregarlo a la lista de objetos entrantes del portal de salida, para que cuando el objeto
                // salga del portal de salida, sepa que tiene que ejecutar la animación de salida del portal.
                exitPortal.incomingObjects.Add(other.gameObject);

                // Ejecutamos la animación de entrada al portal (si es que no se está ejecutando ya).
                if (!portalParticles.isPlaying)
                    StartCoroutine(PlayPortalEffect());

                if (other.CompareTag("Player"))
                {
                    // Obtenemos el waypoint de salida y lo asignamos como el siguiente waypoint del jugador.
                    var player = other.GetComponent<PlayerController>();
                    player.Waypoint = exitPortal.GetWaypoint();
                    var wpProjection = new Vector3(player.Waypoint.transform.position.x, player.transform.position.y, player.Waypoint.transform.position.z);
                    player.transform.position = wpProjection;
                }
            }
        }
    }



    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Existen 2 casos donde se ejecuta el triggerExit.. uno es cuando el objeto sale del portal del cual entró
            // (esto ocurre al setear el transform.position del objeto en el waypoint de salida), y el otro es cuando el objeto
            // sale del portal de salida (esto ocurre cuando el objeto ya se encuentra en el waypoint de salida y se mueve hacia
            // en dirección contraria al portal.
            //
            // Para diferenciar estos casos, verificamos si el objeto que sale del portal se encuentra en la lista de objetos entrantes
            // de esta forma, podemos eliminar el objeto de la lista de entrada y permitirle el ingreso nuevamente.

            if (incomingObjects.Contains(other.gameObject))
                incomingObjects.Remove(other.gameObject);
        }
    }

    private IEnumerator PlayPortalEffect()
    {
        portalParticles.Play();
        yield return new WaitForSeconds(portalEffectDuration);
        portalParticles.Stop();
    }
}
