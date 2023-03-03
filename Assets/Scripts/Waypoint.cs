using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Waypoint : MonoBehaviour
{
    public GameObject collectiblePrefab;
    public Vector2Int gridPosition;
    public bool isBlocked;
    public List<Waypoint> neighbors = new List<Waypoint>();
    public Waypoint previousWaypoint;


    /// <summary>
    /// En el algoritmo A*, G es la distancia desde el punto de inicio hasta el punto actual.
    /// </summary>
    public int g;

    /// <summary>
    /// En el algoritmo A*, H es la estimación de la distancia que se necesita para llegar al punto destino
    /// desde el punto actual.
    /// </summary>
    public int h;

    /// <summary>
    /// En el algoritmo A*, F es la suma de G y H y representa el costo que tiene pasar por este punto
    /// en la trayectoria.
    /// </summary>
    public int f { get { return g + h; } }

    public float distanceThreshold = 0.1f;

    /// <summary>
    /// Retorna el nodo adyacente en la dirección especificada.
    /// </summary>
    public Waypoint GetNeighborByDirection(Vector3 direction)
    {
        var gridDirection = new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.z));
        return neighbors.FirstOrDefault(t => t.gridPosition == gridPosition + gridDirection * MapManager.Instance.WaypointStep);
    }
}
