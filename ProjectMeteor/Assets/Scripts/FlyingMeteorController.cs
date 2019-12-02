using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingMeteorController : MonoBehaviour
{
    private Vector3 destination;
    //public float moveSpeed = 10.0f;
    private Rigidbody rb;
    private GameObject origin;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = new GameObject("FMsWorldOrigin");

        origin.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        origin.transform.LookAt(new Vector3(transform.position.x, 0.0f, transform.position.z));
        destination = origin.transform.position + (origin.transform.forward * PlayerController.worldRadius);
    }

    void FixedUpdate()
    {
        if (PlayerController.playerState == PlayerController.ACTIVELY_PLAYING && !PauseMenu.GameIsPaused)
        {
            transform.LookAt(destination);
            rb.velocity = transform.forward * MeteorManager.flyingMeteorMoveSpeed;
        }
        else
        {
            rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (origin != null)
        {
            Destroy(origin);
        }
    }
}
