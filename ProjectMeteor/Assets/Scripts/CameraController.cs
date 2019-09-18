using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform playerOrigin;
    public float zoomLevel = 10.0f;
    private float zoom;
   
    void Start()
    {
    }

    void LateUpdate()
    {
        float lowestMeteorPosition = 300.0f;
        GameObject[] meteors = GameObject.FindGameObjectsWithTag("Meteor");
        foreach (GameObject meteor in meteors)
        {
            if (meteor.transform.position.y < lowestMeteorPosition) lowestMeteorPosition = meteor.transform.position.y;
        }
        zoom = zoomLevel + Mathf.Abs(lowestMeteorPosition - 300.0f) / 3;

        //Player on high platforms
        if (player.position.y > 5) zoom += (player.position.y - 5) * 2; //This is adjustable

        transform.position = playerOrigin.position + (playerOrigin.transform.forward * zoom) + (playerOrigin.transform.up * zoom / 3);
        transform.rotation = playerOrigin.rotation;
        transform.Rotate(5.0f - zoom / 5, 180.0f, 0, Space.Self);
    }
}
