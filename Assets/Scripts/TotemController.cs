using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemController : MonoBehaviour
{
    [SerializeField] protected float rotationSpeed = 5f;
    [SerializeField] protected float rotationInterval = 5f;

    private PlayerController playerCtrl;
    private bool isRotating = false;
    private Vector3 rotationDirection;

    void Start()
    {
        playerCtrl = FindObjectOfType<PlayerController>();
    }


    void Update()
    {
        InvokeRepeating(nameof(CheckRotation), 0f, rotationInterval);
    }

    protected void CheckRotation()
    {
        var playerDir = (playerCtrl.transform.position - transform.position).normalized;

        // Usamos el producto punto para determinar la dirección del player respecto al totem.
        float north = Vector3.Dot(playerDir, Vector3.forward);
        float south = Vector3.Dot(playerDir, Vector3.back);
        float east = Vector3.Dot(playerDir, Vector3.right);
        float west = Vector3.Dot(playerDir, Vector3.left);

        // Comparamos los valores para determinar la dirección del jugador.
        if (north > south && north > east && north > west)
        {
            // En este caso el jugador esta al norte del totem.
            rotationDirection = Vector3.forward;
            if (!isRotating)
                StartCoroutine(nameof(Rotate));
        }
        else if (south > north && south > east && south > west)
        {
            // En este caso el jugador esta al sur del totem.
            rotationDirection = Vector3.back;
            if (!isRotating)
                StartCoroutine(nameof(Rotate));
        }
        else if (east > north && east > south && east > west)
        {
            // En este caso el jugador esta al este del totem.
            rotationDirection = Vector3.right;
            if (!isRotating)
                StartCoroutine(nameof(Rotate));
        }
        else if (west > north && west > south && west > east)
        {
            // En este caso el jugador esta al oeste del totem.
            rotationDirection = Vector3.left;
            if (!isRotating)
                StartCoroutine(nameof(Rotate));
        }

    }

    private IEnumerator Rotate()
    {
        isRotating = true;

        Quaternion targetRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

        while (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        isRotating = false;
    }
}
