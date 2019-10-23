using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingMeteorController : MonoBehaviour
{
    private Vector3 destination;
    //public float moveSpeed = 10.0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject origin = new GameObject();
        origin.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        origin.transform.Rotate(0.0f, Random.Range(0, 360.0f), 0.0f);
        destination = origin.transform.position + (origin.transform.forward * PlayerController.worldRadius);
    }

    void FixedUpdate()
    {
        transform.LookAt(destination);
        rb.velocity = transform.forward * MeteorManager.flyingMeteorMoveSpeed;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>() != null)
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(1);
        }
        Destroy(gameObject);
    }
}
