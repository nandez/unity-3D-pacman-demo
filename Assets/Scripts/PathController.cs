using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathController
{
    public List<Waypoint> FindPath(Waypoint start, Waypoint end)
    {
        // Definimos las 2 listas requeridas para el algoritmo A*
        var openList = new List<Waypoint>();
        var closedList = new List<Waypoint>();

        // Agregamos el punto de inicio a la lista de abiertos
        openList.Add(start);

        while (openList.Count > 0)
        {
            // Buscamos el waypoint con menor F (coste) en la lista de abiertos
            var currentWaypoint = openList.OrderBy(t => t.f).First();

            openList.Remove(currentWaypoint);
            closedList.Add(currentWaypoint);

            // Si el waypoint actual es el punto de destino, retornamos el camino calculado.
            if (currentWaypoint == end)
                return GetPath(start, end);

            // Iteramos sobre los waypoints adyacentes al nodo actual.
            foreach (var neighbor in currentWaypoint.neighbors)
            {
                if (neighbor.isBlocked || closedList.Contains(neighbor))
                    continue;

                // Calculamos los valores de G y H para el nodo adyacente.
                if (currentWaypoint.isPortal && neighbor.isPortal)
                {
                    // Si ambos waypoints son portales, no se suma nada a G y H.
                    neighbor.g = 0;
                    neighbor.h = 0;
                }
                else
                {
                    neighbor.g = CalculateManhattanDistance(start, neighbor);
                    neighbor.h = CalculateManhattanDistance(end, neighbor);
                }

                neighbor.previousWaypoint = currentWaypoint;

                if (!openList.Contains(neighbor))
                    openList.Add(neighbor);
            }
        }

        // Si no se encontró un camino, retornamos una lista vacía.
        return new List<Waypoint>();
    }

    /// <summary>
    /// La Distancia Manhattan es el total de los valores absolutos de las discrepancias entre las coordenadas "x"
    /// e "y" de las celdas actual y objetivo.
    /// </summary>
    protected int CalculateManhattanDistance(Waypoint a, Waypoint b)
    {
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) + Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }

    /// <summary>
    /// Retorna la lista de waypoints calculado que conforman el camino desde el punto de
    /// inicio hasta el punto de destino.
    /// </summary>
    protected List<Waypoint> GetPath(Waypoint start, Waypoint end)
    {
        var path = new List<Waypoint>();
        var currentWaypoint = end;

        while (currentWaypoint != start)
        {
            path.Add(currentWaypoint);
            currentWaypoint = currentWaypoint.previousWaypoint;
        }

        path.Reverse();
        return path;
    }
}
