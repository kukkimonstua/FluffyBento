﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform playerOrigin;
    public float zoomLevel = 10.0f;
    private float zoom;

    public static int cameraState;
    public Transform attackCameraPosition;
    public Transform endingCameraPosition;

    private float tiltCameraValue;
    //public static bool moving = false;

    private static float endingZoom;
    private static float endingZoomTarget;

    void Start()
    {
        tiltCameraValue = 0.0f;
        cameraState = 1;
    }

    void LateUpdate()
    {
        switch (cameraState)
        {
            case 3: //Victory or defeat.
                Debug.Log(endingZoom);
                if (endingZoom < endingZoomTarget * 0.95f)
                {
                    endingZoom += (endingZoomTarget - endingZoom) / 3 * Time.deltaTime;
                }
                transform.position = endingCameraPosition.position + (transform.forward * endingZoom);
                transform.rotation = endingCameraPosition.rotation;
                break;
            case 2: //Attacking a meteor.
                //if (moving)
                //{
                //StartCoroutine(MoveToPosition(transform, attackCameraPosition, 0.5f));
                //}
                //else
                
                transform.position = attackCameraPosition.position;
                transform.rotation = attackCameraPosition.rotation;
                break;

            default: //or 1
                float lowestMeteorPosition = 300.0f;
                GameObject[] meteors = GameObject.FindGameObjectsWithTag("Meteor");
                foreach (GameObject meteor in meteors)
                {
                    if (meteor.transform.position.y < lowestMeteorPosition) lowestMeteorPosition = meteor.transform.position.y;
                }
                zoom = zoomLevel + Mathf.Abs(lowestMeteorPosition - 300.0f) / 3;
                if (zoom > 50.0f) zoom = 50.0f;

                //Player on high platforms
                if (player.position.y > 5) zoom += (player.position.y - 5.0f) * 3 / 2; //This is adjustable

                transform.position = playerOrigin.position + (playerOrigin.transform.forward * zoom) + (playerOrigin.transform.up * zoom / 2.5f);
                transform.rotation = playerOrigin.rotation;

                tiltCameraValue = Input.GetAxis("Vertical") * -15.0f;

                transform.Rotate(5.0f - zoom / 5 + tiltCameraValue, 180.0f, 0, Space.Self);
                break;
        }
        
    }

    public static void SwitchToMainCamera()
    {
        cameraState = 1;
    }
    public static void SwitchToAttackCamera()
    {
        cameraState = 2;
    }
    public static void SwitchToEndingCamera()
    {
        endingZoom = 0.0f;
        endingZoomTarget = 5.0f;
        cameraState = 3;
    }

    //THIS ISN'T WORKING YET
    private IEnumerator MoveToPosition(Transform fromPosition, Transform toPosition, float duration)
    {
        float counter = 0;

        //Get the current position of the object to be moved
        Vector3 startPos = fromPosition.position;
        Vector3 endPos = toPosition.position;
        Quaternion startRot = fromPosition.rotation;
        Quaternion endRot = toPosition.rotation;


        while (counter < duration)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, attackCameraPosition.position, counter / duration);
            transform.rotation = Quaternion.Lerp(startRot, attackCameraPosition.rotation, counter / duration);
            yield return null;
        }
        //moving = false;
    }
}
