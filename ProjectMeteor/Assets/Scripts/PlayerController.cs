using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    private Rigidbody rb;
    public Transform worldOrigin;
    public Transform playerOrigin;

    private Vector3 offsetFromCentre;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //Get this working later so you can manually set the player's position away from center
        //offsetFromCentre = transform.position - worldOrigin.position;
    }

    // Update is called once per frame
    void Update()
    {
        worldOrigin.Rotate(0.0f, Input.GetAxis("Horizontal") * moveSpeed, 0.0f);
        playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * -30); //10 is temp
        playerOrigin.rotation = worldOrigin.rotation;


        //sets only the x and y values of the player to match the player's origin
        var pos = transform.position;
        pos.x = playerOrigin.position.x;
        pos.z = playerOrigin.position.z;
        transform.position = pos;

        if (Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }


}
