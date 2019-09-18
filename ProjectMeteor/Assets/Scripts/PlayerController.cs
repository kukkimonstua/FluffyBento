using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private bool holdingSword;
    [SerializeField] private GameObject sword;

    public Text promptText;

    void Awake()
    {
        holdingSword = false;
        sword.SetActive(false);

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
        var ppos = transform.position;
        pos.x = playerOrigin.position.x;
        pos.z = playerOrigin.position.z;
        ppos.y = -0.8f;
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

        if (rb.position.y < -1f) //Emily 9/16
        {
           transform.position = ppos;
        }
        //Debug.Log(rb.position.y);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Meteor"))
        {
            if(holdingSword)
            {
                promptText.text = "Click to Attack!";
                promptText.gameObject.SetActive(true);
                if (Input.GetButtonDown("Fire1"))
                {
                    Debug.Log("ATTACKED!");
                    attackMeteor(other.gameObject);
                    holdingSword = false;
                    sword.SetActive(false);
                    promptText.gameObject.SetActive(false);
                }
            }
            else
            {
                promptText.text = "You need a sword!";
                promptText.gameObject.SetActive(true);
            }
        }
        if (other.gameObject.CompareTag("Sword"))
        {
            promptText.text = "Click to Equip!";
            promptText.gameObject.SetActive(true);
            if (Input.GetButtonDown("Fire1"))
            {
                holdingSword = true;
                sword.SetActive(true);

                pickUpSword(other.gameObject);
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        promptText.gameObject.SetActive(false);
    }

    private void attackMeteor(GameObject meteor)
    {

        Destroy(meteor);

    }

    private void pickUpSword(GameObject sword)
    {
        Destroy(sword);
    }

}
