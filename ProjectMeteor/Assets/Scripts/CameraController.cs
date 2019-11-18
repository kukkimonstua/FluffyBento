using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform playerOrigin;
    public float zoomLevel = 12.0f;
    private float defaultZoomLevel;
    private float zoom;

    private Vector3 cameraHeight;
    public float verticalPosThreshold = 5.0f;

    private float tiltCameraValue;
    public static float tutorialCameraTimer;
    //public static bool moving = false;

    public static int cameraState;
    public Transform attackCameraPosition;
    public Transform endingCameraPosition;
    private static float endingZoom;
    private static float endingZoomTarget;
    public Transform successCameraPosition;

    void Start()
    {
        defaultZoomLevel = zoomLevel;
        tiltCameraValue = 0.0f;
        cameraState = 1;
    }
    
    void LateUpdate()
    {
        switch (cameraState)
        {
            case 4:
                transform.position = successCameraPosition.position;
                transform.rotation = successCameraPosition.rotation;
                break;
            case 3: //Victory or defeat.
                if (endingZoom < endingZoomTarget * 0.95f)
                {
                    endingZoom += (endingZoomTarget - endingZoom) * Time.deltaTime;
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
                if (Input.GetButton("buttonL"))
                {
                    ChangeZoomLevel(zoomLevel, -20.0f);
                }
                if (Input.GetButton("buttonR"))
                {
                    ChangeZoomLevel(zoomLevel, 20.0f);
                }

                zoom = zoomLevel + Mathf.Abs(PlayerController.lowestMeteorPosition - PlayerController.worldHeight) / 20.0f;
                if (zoom > zoomLevel * 1.5f) zoom = zoomLevel * 1.5f;
                cameraHeight = playerOrigin.transform.up * zoom / 2.5f;

                //Player on high platforms
                if (player.position.y > verticalPosThreshold)
                {
                    zoom += (player.position.y - verticalPosThreshold) / 3; //This is adjustable
                    cameraHeight += new Vector3(0.0f, player.position.y - verticalPosThreshold, 0.0f);
                }


                transform.position = playerOrigin.position + (playerOrigin.transform.forward * zoom) + cameraHeight;
                transform.rotation = playerOrigin.rotation;

                

                if (!PauseMenu.GameIsPaused)
                {
                    tiltCameraValue += Input.GetAxisRaw("Vertical") * -4.0f;
                    if (tutorialCameraTimer > 0.0f)
                    {
                        tiltCameraValue = -40.0f;
                        tutorialCameraTimer -= Time.deltaTime;
                    }
                    tiltCameraValue = Mathf.Clamp(tiltCameraValue, -40.0f, 40.0f);
                    tiltCameraValue *= 0.9f;
                }

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

    private void ChangeZoomLevel(float currentLevel, float direction)
    {
        currentLevel += direction * Time.deltaTime;
        if (currentLevel > defaultZoomLevel * 2.0f)
        {
            currentLevel = defaultZoomLevel * 2.0f;
        }
        if (currentLevel < defaultZoomLevel * 0.5f)
        {
            currentLevel = defaultZoomLevel * 0.5f;
        }
        zoomLevel = currentLevel;

        /*
        if (!zooming && 0 == 1)
        {
            if (direction < 0 && currentLevel > defaultZoomLevel * 0.5)
            {
                zooming = true;
                StartCoroutine(ZoomToNewLevel(defaultZoomLevel / -2, 0.3f));
            }
            if (direction > 0 && currentLevel < defaultZoomLevel * 1.5)
            {
                zooming = true;
                StartCoroutine(ZoomToNewLevel(defaultZoomLevel / 2, 0.3f));
            }
        }
        */
    }
    /*
    private IEnumerator ZoomToNewLevel(float amount, float duration)
    {
        float counter = 0;
        float startingZoomLevel = zoomLevel;
        float targetZoomLevel = zoomLevel + amount;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            zoomLevel = Mathf.Lerp(startingZoomLevel, targetZoomLevel, counter / duration);
            yield return null;
        }
        zooming = false;
    }
    */

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
