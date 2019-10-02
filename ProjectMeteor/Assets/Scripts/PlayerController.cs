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
    private float worldRadius;

    private bool isGrounded;
    private bool canDoubleJump;
    private bool holdingSword;
    public GameObject equippedSword;

    public GameObject newSword;

    public Text promptText;
    public Text healthText;
    public Text scoreText;

    public GameObject timingWindow;
    public int timingGrade;

    private static int playerState;

    private GameObject targetedMeteor;
    private GameObject targetedSword;

    public int playerMaxHealth = 3;
    private int playerHealth;

    private int playerScore;

    public static float lowestMeteorPosition;
    public float meteorDeathThreshold = 100.0f;
    public Animator anim;
    bool run_left, run_right, idle;

    void Awake()
    {
        playerState = 1;

        playerHealth = playerMaxHealth;
        healthText.text = "Health: " + playerHealth;
        playerScore = 0;
        scoreText.text = "Score: " + playerScore;

        holdingSword = false;
        equippedSword.SetActive(false);

        isGrounded = false;
        canDoubleJump = false;
        rb = GetComponent<Rigidbody>();
        worldRadius = Vector3.Distance(transform.position, worldOrigin.position);
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
                if (Input.GetButtonDown("Fire1") && !TimingWindow.gotPressed)
                {
                    TimingWindow.gotPressed = true;
                }
                break;
            default:
                //REMOVE THIS LATER
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TakeDamage(1);
                }
                lowestMeteorPosition = 300.0f;
                GameObject[] meteors = GameObject.FindGameObjectsWithTag("Meteor");
                foreach (GameObject meteor in meteors)
                {                    
                    if (meteor.transform.position.y < lowestMeteorPosition)
                    {
                        meteor.GetComponent<MeteorController>().isLowest = true;
                        lowestMeteorPosition = meteor.transform.position.y;
                    }
                    else
                    {
                        meteor.GetComponent<MeteorController>().isLowest = false;
                    }
                }
                if (lowestMeteorPosition < meteorDeathThreshold)
                {
                    GameOver(); //From a meteor landing
                }

                //worldOrigin.Rotate(0.0f, Input.GetAxis("Horizontal") * -moveSpeed / 100, 0.0f);
                worldOrigin.LookAt(new Vector3(transform.position.x, worldOrigin.position.y, transform.position.z));

                playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * worldRadius);

                transform.rotation = playerOrigin.rotation = worldOrigin.rotation;
                transform.position = new Vector3(playerOrigin.position.x, transform.position.y, playerOrigin.position.z);



                var horizonalVelocity = transform.right * Input.GetAxis("Horizontal") * moveSpeed * -0.75f;
                var verticalVelocity = new Vector3(0.0f, rb.velocity.y, 0.0f);

                //Debug.Log(horizonalVelocity);
                //Debug.Log(verticalVelocity);
                rb.velocity = horizonalVelocity + verticalVelocity;



                var lowestPos = transform.position;
                lowestPos.y = -0.8f;

                //sets only the x and y values of the player to match the player's origin
                //var pos = transform.position;
                //pos.x = playerOrigin.position.x;
                //pos.z = playerOrigin.position.z;

                //transform.position = pos;

                
                if (Input.GetButtonDown("Jump") && !isGrounded && canDoubleJump)
                {
                    canDoubleJump = false;
                    rb.velocity = Vector3.up * jumpForce;
                }
                if (Input.GetButtonDown("Jump") && isGrounded)
                {
                    isGrounded = false;
                    rb.velocity = Vector3.up * jumpForce;
                }                
                rb.velocity += Vector3.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector3.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                }

                if (rb.position.y < -1f) //Emily 9/16
                {
                    transform.position = lowestPos;
                }

                if (Input.GetButtonDown("Fire2"))
                {
                    if (holdingSword)
                    {
                        DropSword(newSword);
                    }
                    else
                    {
                        if (targetedSword != null) //if you're near a sword
                        {
                            PickUpSword(targetedSword);
                            promptText.gameObject.SetActive(false);
                        }
                    }
                }

                if(Input.GetButtonDown("Fire1") && holdingSword && targetedMeteor != null)
                {
                    promptText.gameObject.SetActive(false);
                    StartCoroutine(AttackOnMeteor(transform, targetedMeteor, 3.0f));
                }
                break;
        }
        UpdateAnimations();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit something!" + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Platform"))
        {
            Debug.Log("Landed!");
            isGrounded = true;
            canDoubleJump = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerState == 1)
        {
            if (other.gameObject.CompareTag("Meteor"))
            {
                targetedMeteor = other.gameObject;
                targetedMeteor.GetComponent<MeteorController>().withinAttackRange = true;
                if (!holdingSword)
                {
                    promptText.text = "You need a sword!";                    
                }
                else
                {
                    promptText.text = "CTRL to Attack!";
                }
            }
            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = other.gameObject;
                promptText.text = "ALT to Equip!";
            }
            promptText.gameObject.SetActive(true);
        }            
    }
    private void OnTriggerExit(Collider other)
    {
        if (playerState == 1)
        {
            promptText.gameObject.SetActive(false);
            if (other.gameObject.CompareTag("Meteor"))
            {
                other.GetComponent<MeteorController>().withinAttackRange = false;
                targetedMeteor = null;
            }
            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = null;
            }
        }        
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
        
        //Get the current position of the object to be moved
        Vector3 startPos = fromPosition.position;
        Vector3 toPosition = meteor.transform.position;
        CameraController.SwitchToAttackCamera();

        float counter = 0;
        timingGrade = 0;
        timingWindow.GetComponent<TimingWindow>().StartTimingWindow(duration);

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        MeteorManager.meteorsPaused = false;
        playerState = 1;

        holdingSword = false;
        equippedSword.SetActive(false);

        CameraController.SwitchToMainCamera();

        if (timingGrade > 0)
        {
            Destroy(meteor);
        }
        else
        {
            TakeDamage(1);
        }
    }

    private void PickUpSword(GameObject pickedUpSword)
    {
        holdingSword = true;
        equippedSword.SetActive(true);
        Destroy(pickedUpSword);
    }

    private void DropSword(GameObject swordType)
    {
        holdingSword = false;
        equippedSword.SetActive(false);
        Instantiate(swordType, transform.position, transform.rotation);
    }

    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
        healthText.text = "Health: " + playerHealth;

        if (playerHealth <= 0)
        {
            GameOver(); //From health loss
        }
    }
    public void AddScore(int amount)
    {
        playerScore += amount;
        scoreText.text = "Score: " + playerScore;
    }

    private void GameOver()
    {
        MeteorManager.meteorsPaused = true;
        playerState = 3;
        CameraController.SwitchToEndingCamera();
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
