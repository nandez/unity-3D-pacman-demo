using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Vector2Int GridPosition;
    public bool IsBlocked;
    public List<Waypoint> Neighbors;
    public Waypoint PreviousWaypoint;


    /// <summary>
    /// En el algoritmo A*, G es la distancia desde el punto de inicio hasta el punto actual.
    /// </summary>
    public int G;

    /// <summary>
    /// En el algoritmo A*, H es la estimación de la distancia que se necesita para llegar al punto destino
    /// desde el punto actual.
    /// </summary>
    public int H;

    /// <summary>
    /// En el algoritmo A*, F es la suma de G y H y representa el costo que tiene pasar por este punto
    /// en la trayectoria.
    /// </summary>
    public int F { get { return G + H; } }
}
