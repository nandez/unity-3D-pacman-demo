using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] protected Transform target;
    [SerializeField] protected float smoothSpeed = 3f;
    [SerializeField] protected Vector3 offset;

    void LateUpdate()
    {
        // Actualizamos la posición de la cámara para que siga al jugador.
        transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
