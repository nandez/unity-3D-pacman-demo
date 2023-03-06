using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance { get { return _instance; } }

    public List<Waypoint> waypoints = new List<Waypoint>();
    public List<Waypoint> corners = new List<Waypoint>();
    public List<Pellet> pellets = new List<Pellet>();
    [SerializeField] protected GameObject pelletContainer;

    [SerializeField] protected int waypointStep = 2;
    public int WaypointStep { get { return waypointStep; } }


    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        // Obtenemos la altura del jugador para instanciar los pellets en la altura correcta.
        var playerHeight = FindObjectOfType<PlayerController>().transform.position.y;

        // Iteramos sobre todos los waypoints del mapa y calculamos su posición en la grilla.
        // Esto podemos hacerlo dado que sabemos de antemano, que las posiciones de los waypoints
        // en el mundo son las mismas que las posiciones en la grilla.
        // De no ser asi, entonces deberíamos setear el GridPosition de cada waypoint en el editor.
        foreach (var wp in FindObjectsOfType<Waypoint>())
        {
            wp.gridPosition = new Vector2Int(Mathf.RoundToInt(wp.transform.position.x), Mathf.RoundToInt(wp.transform.position.z));
            waypoints.Add(wp);

            // Verificamos si el waypoint tiene un collectible asociado y lo instanciamos.
            // ej: un pellet, power pellet, etc.
            if (wp.collectiblePrefab != null)
            {
                var wpProjection = new Vector3(wp.transform.position.x, playerHeight, wp.transform.position.z);
                var collectible = Instantiate(wp.collectiblePrefab, wpProjection, wp.collectiblePrefab.transform.rotation);
                collectible.transform.parent = pelletContainer.transform;
                pellets.Add(collectible.GetComponent<Pellet>());
            }
        }

        // Iteramos sobre todos los waypoints del mapa y calculamos sus waypoints adyacentes
        // NOTA: Dado que necesitamos tener ya cargada la propiedad GridPosition de cada waypoint,
        // no podemos realizar este calculo en el loop anterior. Si bien es una operación costosa,
        // solo se realiza una vez al inicio del juego.
        // 1. Creamos un diccionario que nos permita acceder a los waypoints por su posición en la grilla.
        // 2. Iteramos sobre todos los waypoints del mapa y calculamos sus waypoints adyacentes.
        //
        // NOTA: Los waypoints que representan las zonas de teletransporte, ya tiene pre-cargadas sus respectivas
        //       entradas y salidas como waypoints adyacentes.
        var waypointDictionary = waypoints.ToDictionary(t => t.gridPosition, t => t);
        foreach (var wp in waypoints)
        {
            // Calculamos el waypoint adyacente en la dirección Norte
            var neighbor = new Vector2Int(wp.gridPosition.x, wp.gridPosition.y + WaypointStep);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);

            // Calculamos el waypoint adyacente en la dirección Sur
            neighbor = new Vector2Int(wp.gridPosition.x, wp.gridPosition.y - WaypointStep);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);

            // Calculamos el waypoint adyacente en la dirección Este
            neighbor = new Vector2Int(wp.gridPosition.x + WaypointStep, wp.gridPosition.y);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);

            // Calculamos el waypoint adyacente en la dirección Oeste
            neighbor = new Vector2Int(wp.gridPosition.x - WaypointStep, wp.gridPosition.y);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);
        }

        // Obtenemos los valores minimo y maximo de la grilla para calcular los waypoints de las esquinas.
        var minX = waypoints.Min(t => t.gridPosition.x) + WaypointStep;
        var maxX = waypoints.Max(t => t.gridPosition.x) - WaypointStep;
        var minY = waypoints.Min(t => t.gridPosition.y) + WaypointStep;
        var maxY = waypoints.Max(t => t.gridPosition.y) - WaypointStep;

        corners.Add(waypointDictionary[new Vector2Int(minX, minY)]);
        corners.Add(waypointDictionary[new Vector2Int(minX, maxY)]);
        corners.Add(waypointDictionary[new Vector2Int(maxX, minY)]);
        corners.Add(waypointDictionary[new Vector2Int(maxX, maxY)]);
    }

    public Waypoint GetRandomWaypoint()
    {
        return waypoints[Random.Range(0, waypoints.Count)];
    }

    public Waypoint GetRandomCornerWaypoint()
    {
        return corners[Random.Range(0, corners.Count)];
    }
}
