using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [SerializeField] protected Transform target;

    private void LateUpdate()
    {
        var newPosition = target.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }

}
