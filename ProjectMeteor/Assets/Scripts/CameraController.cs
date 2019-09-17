using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerOrigin;
    public float offsetFromPlayer = 15.0f;

    void Start()
    {
    }

    void LateUpdate()
    {
        transform.position = playerOrigin.position + (playerOrigin.transform.forward * offsetFromPlayer); //10 is temp
        transform.rotation = playerOrigin.rotation;
        transform.Rotate(0, 180.0f, 0, Space.Self);
    }
}
