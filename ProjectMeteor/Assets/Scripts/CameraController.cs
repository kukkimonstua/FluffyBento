using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform playerOrigin;
    public float zoomLevel = 10.0f;
    private float zoom;
    private float maxZoomLevel;
    private Vector3 cameraHeight;

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
                maxZoomLevel = zoomLevel * 1.5f;
                zoom = zoomLevel + Mathf.Abs(PlayerController.lowestMeteorPosition - 300.0f) / 30;
                if (zoom > maxZoomLevel) zoom = maxZoomLevel;
                cameraHeight = playerOrigin.transform.up * zoom / 2.5f;

                //Player on high platforms
                if (player.position.y > 5)
                {
                    zoom += (player.position.y - 5.0f) / 2; //This is adjustable
                    cameraHeight += new Vector3(0.0f, player.position.y - 5.0f, 0.0f);
                }


                transform.position = playerOrigin.position + (playerOrigin.transform.forward * zoom) + cameraHeight;
                transform.rotation = playerOrigin.rotation;

                tiltCameraValue += Input.GetAxis("Vertical") * -4.0f;
                tiltCameraValue = Mathf.Clamp(tiltCameraValue, -40.0f, 40.0f);
                tiltCameraValue *= 0.9f;
                if (tiltCameraValue > 0) transform.Translate(0.0f, tiltCameraValue / 3.0f, 0.0f);

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
