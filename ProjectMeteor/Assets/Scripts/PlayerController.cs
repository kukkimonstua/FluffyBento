using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.0f;

    public Transform worldOrigin;
    public Transform playerOrigin;
    private float offsetFromCentre;

    private bool isGrounded;

    void Awake()
    {
        isGrounded = false;
        rb = GetComponent<Rigidbody>();
        offsetFromCentre = Vector3.Distance(transform.position, worldOrigin.position);
    }

    // Update is called once per frame
    void Update()
    {
        worldOrigin.Rotate(0.0f, Input.GetAxis("Horizontal") * -moveSpeed / 100, 0.0f);
        playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * offsetFromCentre);
        playerOrigin.rotation = worldOrigin.rotation;

        transform.rotation = playerOrigin.rotation; //perhaps temporary solution?

        //sets only the x and y values of the player to match the player's origin
        var pos = transform.position;
        pos.x = playerOrigin.position.x;
        pos.z = playerOrigin.position.z;
        transform.position = pos;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isGrounded = false;
            //rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            rb.velocity = Vector3.up * jumpForce;
        }
        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        } else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }

}
