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

    public int waypointStep = 2;

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
        // Iteramos sobre todos los waypoints del mapa y calculamos su posición en la grilla.
        // Esto podemos hacerlo dado que sabemos de antemano, que las posiciones de los waypoints
        // en el mundo son las mismas que las posiciones en la grilla.
        // De no ser asi, entonces deberíamos setear el GridPosition de cada waypoint en el editor.
        foreach (var wp in FindObjectsOfType<Waypoint>())
        {
            wp.gridPosition = new Vector2Int(Mathf.RoundToInt(wp.transform.position.x), Mathf.RoundToInt(wp.transform.position.z));
            waypoints.Add(wp);
        }

        // Iteramos sobre todos los waypoints del mapa y calculamos sus waypoints adyacentes
        // NOTA: Dado que necesitamos tener ya cargada la propiedad GridPosition de cada waypoint,
        // no podemos realizar este calculo en el loop anterior. Si bien es una operación costosa,
        // solo se realiza una vez al inicio del juego.
        // 1. Creamos un diccionario que nos permita acceder a los waypoints por su posición en la grilla.
        // 2. Iteramos sobre todos los waypoints del mapa y calculamos sus waypoints adyacentes.
        var waypointDictionary = waypoints.ToDictionary(t => t.gridPosition, t => t);
        foreach (var wp in waypoints)
        {
            // Calculamos el waypoint adyacente en la dirección Norte
            var neighbor = new Vector2Int(wp.gridPosition.x, wp.gridPosition.y + waypointStep);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);

            // Calculamos el waypoint adyacente en la dirección Sur
            neighbor = new Vector2Int(wp.gridPosition.x, wp.gridPosition.y - waypointStep);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);

            // Calculamos el waypoint adyacente en la dirección Este
            neighbor = new Vector2Int(wp.gridPosition.x + waypointStep, wp.gridPosition.y);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);

            // Calculamos el waypoint adyacente en la dirección Oeste
            neighbor = new Vector2Int(wp.gridPosition.x - waypointStep, wp.gridPosition.y);
            if (waypointDictionary.ContainsKey(neighbor))
                wp.neighbors.Add(waypointDictionary[neighbor]);
        }

        // Obtenemos los valores minimo y maximo de la grilla para calcular los waypoints de las esquinas.
        var minX = waypoints.Min(t => t.gridPosition.x) + waypointStep;
        var maxX = waypoints.Max(t => t.gridPosition.x) - waypointStep;
        var minY = waypoints.Min(t => t.gridPosition.y) + waypointStep;
        var maxY = waypoints.Max(t => t.gridPosition.y) - waypointStep;

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
