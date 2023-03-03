using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    [SerializeField] protected int points = 10;
    [SerializeField] protected bool isPowerPellet = false;

    public int Points { get { return points; } }
    public bool IsPowerPellet { get { return isPowerPellet; } }

    public delegate void PelletCollectedEvent(Pellet pellet);
    public static event PelletCollectedEvent OnPelletCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            OnPelletCollected?.Invoke(this);
    }

    void Start()
    {
        transform.localScale *= 1.1f;
    }
}
