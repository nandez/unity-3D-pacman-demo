using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance { get { return _instance; } }

    public List<Waypoint> waypoints = new List<Waypoint>();

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
            wp.GridPosition = new Vector2Int(Mathf.RoundToInt(wp.transform.position.x), Mathf.RoundToInt(wp.transform.position.z));
            waypoints.Add(wp);
        }

    }



    // Update is called once per frame
    void Update()
    {

    }
}
