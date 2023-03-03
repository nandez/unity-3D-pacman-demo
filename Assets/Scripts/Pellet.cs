using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    [SerializeField] protected int points = 10;
    [SerializeField] protected bool isPowerPellet = false;
    [SerializeField] private float powerPelletBounceFrequency = 1.5f;
    [SerializeField] private float powerPelletBounceAmplitude = 0.15f;

    public int Points { get { return points; } }
    public bool IsPowerPellet { get { return isPowerPellet; } }

    public delegate void PelletCollectedEvent(Pellet pellet);
    public static event PelletCollectedEvent OnPelletCollected;

    private Vector3 initialPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Desactivamos la colisión entre el pellet y el player para evitar que se vuelva a detectar
            Physics.IgnoreCollision(other, GetComponent<Collider>(), true);

            // Invocamos el evento para que el GameManager lo capture y actualice el puntaje.
            OnPelletCollected?.Invoke(this);
        }
    }

    void Start()
    {
        // Si se trata de un power pellet, lo escalamos un poco para que se vea más grande.
        if (isPowerPellet)
            transform.localScale *= 1.1f;

        initialPosition = transform.position;
    }

    void Update()
    {
        // Cuando se trata de un power pellet, le aplicamos un efecto de oscilación.
        if (isPowerPellet)
        {
            var position = initialPosition;
            position.y += Mathf.Sin(Time.fixedTime * Mathf.PI * powerPelletBounceFrequency) * powerPelletBounceAmplitude;
            transform.position = position;
        }
    }
}
