using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerOrigin;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - playerOrigin.position;
    }

    void LateUpdate()
    {
        transform.position = playerOrigin.position + (playerOrigin.transform.forward * -10); //10 is temp
        transform.rotation = playerOrigin.rotation;
        //transform.Rotate(0, 180.0f, 0, Space.Self);

        //transform.position = playerOrigin.position + offset;
    }
}
