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

    public GameObject timingWindow;

    public Animator anim;
    bool run_left, run_right, idle;

    private int playerState;

    void Awake()
    {
        playerState = 1;

        holdingSword = false;
        sword.SetActive(false);

        anim = GetComponent<Animator>();

        isGrounded = false;
        rb = GetComponent<Rigidbody>();
        offsetFromCentre = Vector3.Distance(transform.position, worldOrigin.position);
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case 3:                
                break; //No fall multiplier for a floaty death is totally intentional

            case 2:
                rb.velocity = Vector3.up * 0;
                break;

            default:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    playerState = 3;
                    CameraController.SwitchToEndingCamera();
                }

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
                if (rb.velocity.y < 0)
                {
                    rb.velocity += Vector3.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                }
                else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector3.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                }

                if (rb.position.y < -1f) //Emily 9/16
                {
                    transform.position = ppos;
                }
                UpdateAnimations();
                Debug.Log(idle);
                break;
        }
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
        if (other.gameObject.CompareTag("Meteor") && playerState == 1)
        {
            if (!holdingSword)
            {
                promptText.text = "You need a sword!";
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.text = "CTRL to Attack!";
                promptText.gameObject.SetActive(true);
                if (Input.GetButtonDown("Fire1"))
                {
                    promptText.gameObject.SetActive(false);
                    StartCoroutine(AttackOnMeteor(transform, other.gameObject, 3.0f));
                }
            }
            
        }
        if (other.gameObject.CompareTag("Sword") && playerState == 1)
        {
            promptText.text = "CTRL to Equip!";
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

    private IEnumerator AttackOnMeteor(Transform fromPosition, GameObject meteor, float duration)
    {
        //Make sure there is only one instance of this function running
        if (playerState == 2)
        {
            yield break; ///exit if this is still running
        }
        playerState = 2;

        MeteorManager.meteorsPaused = true;
        
        float counter = 0;

        //Get the current position of the object to be moved
        Vector3 startPos = fromPosition.position;
        Vector3 toPosition = Vector3.Lerp(meteor.transform.position, fromPosition.position, 0.5f); //halfway, temp solution
        CameraController.SwitchToAttackCamera();


        while (counter < duration)
        {
            if (counter > duration / 3)
            {
                timingWindow.SetActive(true);
            }

            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        timingWindow.SetActive(false);
        MeteorManager.meteorsPaused = false;
        playerState = 1;

        holdingSword = false;
        sword.SetActive(false);


        Destroy(meteor);
        CameraController.SwitchToMainCamera();
    }

    private void pickUpSword(GameObject sword)
    {
        Destroy(sword);
    }

    private void UpdateAnimations()
    {
        anim.SetBool("run_left", run_left);
        anim.SetBool("run_right", run_right);
        anim.SetBool("idle", idle);

        if (Input.GetKey(KeyCode.A))
        {
            run_left = true;
            run_right = false;
            idle = false;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            run_left = false;
            run_right = true;
            idle = false;
        }
        else
        {
            run_left = false;
            run_right = false;
            idle = true;
        }
    }
}
