using System;
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
    public static float worldRadius;

    public bool isGrounded;
    public bool canDoubleJump;
    public bool onWall;
    private Vector3 horizonalVelocity;

    private bool holdingSword;
    public GameObject equippedSword;

    public GameObject newSword;

    public Text promptText;
    public TextMesh actionText;
    public Text healthText;
    public Text scoreText;
    public Slider meteorLandingSlider;
    public Text meteorLandingDanger;

    public Text tempEquipText;

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
        worldRadius = Vector3.Distance(transform.position, worldOrigin.position);
        rb = GetComponent<Rigidbody>();
        playerState = 1;

        playerHealth = playerMaxHealth;
        healthText.text = "Health: " + playerHealth;
        playerScore = 0;
        scoreText.text = "Score: " + playerScore;
        meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        tempEquipText.text = "Equipped: None";

        holdingSword = false;
        equippedSword.SetActive(false);

        isGrounded = false;
        canDoubleJump = false;
        onWall = false;

        
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case 3:
                rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
                break; //No fall multiplier for a floaty death is totally intentional

            case 2:
                rb.velocity = Vector3.up * 0;
                if (Input.GetButtonDown("Fire1") && !TimingWindow.gotPressed)
                {
                    TimingWindow.gotPressed = true;
                }
                break;
            default:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TakeDamage(1); //USE THIS TO TEST DAMAGE TAKING
                }

                TrackLowestMeteor();

                //worldOrigin.Rotate(0.0f, Input.GetAxis("Horizontal") * -moveSpeed / 100, 0.0f);
                worldOrigin.LookAt(new Vector3(transform.position.x, worldOrigin.position.y, transform.position.z));
                transform.rotation = playerOrigin.rotation = worldOrigin.rotation;

                playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * worldRadius);

                transform.position = new Vector3(playerOrigin.position.x, transform.position.y, playerOrigin.position.z);

                horizonalVelocity -= transform.right * Input.GetAxis("Horizontal") * moveSpeed / 6;
                horizonalVelocity = Vector3.ClampMagnitude(horizonalVelocity, moveSpeed * 2) * 0.9f;

                if (Input.GetButtonDown("Jump"))
                {
                    if (!isGrounded)
                    {
                        if (onWall)
                        {
                            Debug.Log("WALL JUMP");
                            rb.velocity = Vector3.up * jumpForce;
                            horizonalVelocity *= -1.0f;
                        }
                        else if (canDoubleJump)
                        {
                            canDoubleJump = false;
                            rb.velocity = Vector3.up * jumpForce;
                        }
                    }
                    else
                    {
                        rb.velocity = Vector3.up * jumpForce;
                    }
                }
                           
                rb.velocity += Vector3.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector3.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                }

                rb.velocity = new Vector3(horizonalVelocity.x, rb.velocity.y, horizonalVelocity.z);

                var lowestPos = transform.position;
                lowestPos.y = -0.8f;

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

                UpdateAnimations();
                break;
        }
        
    }

    private void TrackLowestMeteor()
    {
        lowestMeteorPosition = 300.0f; //the current height at which meteors spawn
        float sliderRange = lowestMeteorPosition - meteorDeathThreshold;

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

        //Debug.Log(lowestMeteorPosition + " is higher than " + meteorDeathThreshold);
        meteorLandingSlider.value = (lowestMeteorPosition - meteorDeathThreshold) / sliderRange;
        if (lowestMeteorPosition / 2 < meteorDeathThreshold)
        {
            meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Sin(Time.time * 5.0f) * 0.5f + 0.5f);
        }
        if (lowestMeteorPosition < meteorDeathThreshold)
        {
            GameOver(); //From a meteor landing
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Hit something!" + collision.relativeVelocity);
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
            canDoubleJump = true;
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            onWall = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            onWall = false;
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
                promptText.gameObject.SetActive(true);
            }

            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = other.gameObject;
                actionText.gameObject.SetActive(true);
            }
            
        }            
    }
    private void OnTriggerExit(Collider other)
    {
        if (playerState == 1)
        {

            if (other.gameObject.CompareTag("Meteor"))
            {
                other.GetComponent<MeteorController>().withinAttackRange = false;
                targetedMeteor = null;
                promptText.gameObject.SetActive(false);
            }
            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = null;
                actionText.gameObject.SetActive(false);
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
        tempEquipText.text = "Equipped: None";

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
        actionText.gameObject.SetActive(false);
        holdingSword = true;
        equippedSword.SetActive(true);
        tempEquipText.text = "Equipped: Sword";
        Destroy(pickedUpSword);
    }

    private void DropSword(GameObject swordType)
    {
        holdingSword = false;
        equippedSword.SetActive(false);
        tempEquipText.text = "Equipped: None";
        Instantiate(swordType, transform.position + new Vector3(0.0f, 1.0f, 0.0f), transform.rotation);
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
        if (Input.GetAxis("Horizontal") < 0)
        {
            run_left = true;
            run_right = false;
            idle = false;
        }
        else if (Input.GetAxis("Horizontal") > 0)
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

        anim.SetBool("run_left", run_left);
        anim.SetBool("run_right", run_right);
        anim.SetBool("idle", idle);
    }
}
