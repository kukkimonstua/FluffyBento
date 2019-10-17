using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("PLAYER MOVEMENT SETTINGS")]
    private Rigidbody rb;
    private CapsuleCollider myCollider;

    public float moveSpeed = 5.0f;
    public float horizontalDrag = 0.8f;
    public float jumpForce = 10.0f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.0f;

    private bool isAttacking;
    private bool isDashing;

    [Header("GLOBAL SETTINGS")]
    public Transform worldOrigin;
    public Transform meteorWorldOrigin;
    public Transform playerOrigin;
    public static float worldRadius; //Accessed by a LOT of different scripts

    [Header("PLAYER MOVEMENT")]
    private Vector3 circularVelocity;
    public bool isGrounded; //Make these
    public bool canDoubleJump; //public when
    public float touchedWallDirection; //you're debugging.
    private int previousWallJumpDirection;
    private int holdingSword; //0 is nothing, 1 - 3 are the different types

    [Header("GAME SETTINGS")]
    public static int playerState; //1 = running, 2 = attacking, 3 = game over
    
    public GameObject zanbato;
    public GameObject broadsword;
    public GameObject katana;

    private GameObject targetedMeteor;
    private GameObject targetedSword;

    private Vector3 startingPosition;
    public int playerMaxHealth = 3;
    private int playerHealth;
    private int playerScore;
    private int meteorsDestroyed;
    public static float lowestMeteorPosition;
    public float meteorDeathThreshold = 100.0f;

    [Header("LINKS TO MODEL AND ANIMATIONS")]
    public GameObject avatarModel;
    private float avatarModelRotation;
    public Animator anim;
    private bool running, dashing;

    public GameObject equippedZanbato;
    public GameObject equippedBroadsword;
    public GameObject equippedKatana;

    [Header("LINKS TO GUI")]
    public GameObject timingWindow;
    public PauseMenu pauseMenu;
    public int timingGrade;
    public GUIController gui;

    void Awake()
    {
        worldRadius = Vector3.Distance(transform.position, worldOrigin.position);
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<CapsuleCollider>();
        startingPosition = transform.position;
        ResetLevel();
    }
    void Update()
    {
        switch (playerState)
        {
            case 3:
                
                rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);

                if (Input.GetButtonDown("buttonX") && GUIController.menuUnlocked)
                {
                    MeteorManager.ResetMeteors();
                    SwordManager.ResetSwords();
                    ResetLevel();
                }

                break; //No fall multiplier for a floaty death is totally intentional

            case 2:
                rb.velocity = Vector3.up * 0;
                touchedWallDirection = 0;
                isGrounded = false;
                if (Input.GetButtonDown("buttonX") && !TimingWindow.gotPressed)
                {
                    TimingWindow.gotPressed = true;
                }
                break;

            default:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    //ResetLevel();
                    TakeDamage(1); //USE THIS TO TEST DAMAGE TAKING
                }

                TrackLowestMeteor();

                //worldOrigin.Rotate(0.0f, Input.GetAxisRaw("Horizontal") * -moveSpeed / 100, 0.0f);
                worldOrigin.LookAt(new Vector3(transform.position.x, worldOrigin.position.y, transform.position.z));
                transform.rotation = playerOrigin.rotation = worldOrigin.rotation;

                playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * worldRadius);

                transform.position = new Vector3(playerOrigin.position.x, transform.position.y, playerOrigin.position.z);

                var moveDrag = horizontalDrag;
                if(isGrounded && Input.GetAxisRaw("Horizontal") == 0 && !dashing)
                {
                    moveDrag /= 4;
                }

                if (touchedWallDirection != 0 || isDashing) circularVelocity -= transform.right * Input.GetAxisRaw("Horizontal") * moveSpeed / 40;
                else circularVelocity -= transform.right * Input.GetAxisRaw("Horizontal") * moveSpeed / 8;
                circularVelocity = Vector3.ClampMagnitude(circularVelocity, moveSpeed * 2) * moveDrag;

                

                if (Input.GetButtonDown("Jump"))
                {
                    if (!isGrounded)
                    {
                        if (touchedWallDirection != 0)
                        {
                            WallJump(touchedWallDirection);
                        }
                        else if (canDoubleJump)
                        {
                            canDoubleJump = false;
                            rb.velocity = Vector3.up * jumpForce;
                        }
                    }
                    else
                    {
                        isGrounded = false;
                        rb.velocity = Vector3.up * jumpForce;
                    }
                    
                }
                if(Input.GetButtonDown("buttonB"))
                {
                    if (!isDashing && Input.GetAxisRaw("Horizontal") != 0)
                    {
                        StartCoroutine(StartDashing(0.4f));
                    }
                }
                var fallM = fallMultiplier;
                if (touchedWallDirection != 0 && rb.velocity.y < 0)
                {
                    fallM /= 2;
                }

                rb.velocity += Vector3.up * Physics2D.gravity.y * (fallM - 1) * Time.deltaTime;
                if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector3.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                }

                rb.velocity = new Vector3(circularVelocity.x, rb.velocity.y, circularVelocity.z);

                if (rb.velocity.y < fallMultiplier * -2.5f)
                {
                    isGrounded = false;
                }

                var lowestPos = transform.position;
                lowestPos.y = -0.8f;

                if (rb.position.y < -1f) //Emily 9/16
                {
                    transform.position = lowestPos;
                }
                
                if (Input.GetButtonDown("buttonY"))
                {
                    if (targetedSword != null) //if you're near a sword
                    {
                        if (holdingSword == 0)
                        {
                            PickUpSword(targetedSword);
                        }
                        else
                        {
                            SwitchSwords(targetedSword);
                        }
                    }
                    else if (holdingSword != 0)
                    {
                        DropSword();
                    }
                }
                if (targetedMeteor != null)
                {
                    if (holdingSword == 0)
                    {
                        gui.TogglePrompt(true, "You need a sword!");
                    }
                    else
                    {
                        gui.TogglePrompt(true, "(X)\nAttack Meteor");
                    }
                }
                if(Input.GetButtonDown("buttonX"))
                {
                    if (holdingSword != 0)
                    {
                        if (targetedMeteor != null)
                        {
                            gui.TogglePrompt(false, "");
                            StartCoroutine(AttackOnMeteor(transform, targetedMeteor, 3.0f));
                        }
                        else if (!isAttacking)
                        {
                            StartCoroutine(Attack(0.5f));
                        }
                    }
                    else
                    {
                        Debug.Log("You have no sword!");
                    }
                }

                UpdateAnimations();
                break;
        }
        
    }
    private void EquipSword(int swordID)
    {
        switch (swordID)
        {
            case 0: //NO SWORD
                equippedZanbato.SetActive(false);
                equippedBroadsword.SetActive(false);
                equippedKatana.SetActive(false);
                break;
            case 1:
                equippedZanbato.SetActive(true);
                equippedBroadsword.SetActive(false);
                equippedKatana.SetActive(false);
                break;
            case 2:
                equippedZanbato.SetActive(false);
                equippedBroadsword.SetActive(true);
                equippedKatana.SetActive(false);
                break;
            case 3:
                equippedZanbato.SetActive(false);
                equippedBroadsword.SetActive(false);
                equippedKatana.SetActive(true);
                break;
        }
    }

    private void ResetLevel()
    {
        playerState = 1;
        holdingSword = 0;
        EquipSword(0);
        isGrounded = false;
        canDoubleJump = false;
        touchedWallDirection = 0;

        playerHealth = playerMaxHealth;
        playerScore = 0;
        meteorsDestroyed = 0;
        gui.TogglePlayerActionText(false, "");
        gui.ResetGUI();
        gui.UpdateHealthUI(playerHealth);
        gui.UpdateScoreUI(playerScore);
        gui.UpdateMeteorsDestroyed(meteorsDestroyed);
        ResetBreakables();

        transform.position = startingPosition;
        avatarModelRotation = 0.0f;
        CameraController.SwitchToMainCamera();
    }
    private void ResetBreakables()
    {
        GameObject[] breakables = GameObject.FindGameObjectsWithTag("Breakable");
        foreach (GameObject b in breakables)
        {
            b.GetComponent<BreakableController>().GetRestored();
        }
    }
    private void TrackLowestMeteor()
    {
        lowestMeteorPosition = 400.0f; //the current height at which meteors spawn
        GameObject[] meteors = GameObject.FindGameObjectsWithTag("Meteor");
        if (meteors.Length <= 0) gui.UpdateMeteorDirectionUI(0, 0);

        foreach (GameObject meteor in meteors)
        {
            if (meteor.transform.position.y < lowestMeteorPosition)
            {
                meteor.GetComponent<MeteorController>().isLowest = true;
                lowestMeteorPosition = meteor.transform.position.y;
                meteorWorldOrigin.LookAt(new Vector3(meteor.transform.position.x, meteorWorldOrigin.transform.position.y, meteor.transform.position.z));
                
                float leftAngleDifference = 360 - (worldOrigin.eulerAngles.y - meteorWorldOrigin.eulerAngles.y);
                if (meteorWorldOrigin.eulerAngles.y > worldOrigin.eulerAngles.y) leftAngleDifference = meteorWorldOrigin.eulerAngles.y - worldOrigin.eulerAngles.y;
                float rightAngleDifference = 360 - leftAngleDifference;

                if (leftAngleDifference < rightAngleDifference)
                {
                    gui.UpdateMeteorDirectionUI(-1, leftAngleDifference);
                }
                else
                {
                    gui.UpdateMeteorDirectionUI(1, rightAngleDifference);
                }
            }
            else
            {
                meteor.GetComponent<MeteorController>().isLowest = false; //All other meteors is made false
            }
        }

        gui.UpdateMeteorLandingUI(lowestMeteorPosition, meteorDeathThreshold, 300.0f - meteorDeathThreshold);
        if (lowestMeteorPosition < meteorDeathThreshold)
        {
            GameOver(); //From a meteor landing
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.GetContact(0).normal.y > 0.5f)
        {
            isGrounded = true;
            canDoubleJump = true;
            touchedWallDirection = 0;
            previousWallJumpDirection = 0;
        }
        else if(collision.GetContact(0).normal.x + collision.GetContact(0).normal.z != 0)
        {
            touchedWallDirection = collision.GetContact(0).normal.x + collision.GetContact(0).normal.z;
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
//        isGrounded = false; CAN'T DO THIS BECAUSE IMPOSSIBLE TO TELL WHERE THE EXIT EVENT HAPPENED
        touchedWallDirection = 0;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (playerState == 1)
        {
            if (other.gameObject.CompareTag("Meteor"))
            {
                targetedMeteor = other.gameObject;
                targetedMeteor.GetComponent<MeteorController>().withinAttackRange = true;                
            }

            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = other.gameObject;
                if(holdingSword == 0)
                {
                    gui.TogglePlayerActionText(true, "(Y) Equip");
                }
                else
                {
                    gui.TogglePlayerActionText(true, "(Y) Swap");
                }
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
                gui.TogglePrompt(false, "");
            }
            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = null;
                gui.TogglePlayerActionText(false, "");
            }
        }        
    }
    private IEnumerator StartDashing(float duration)
    {
        isDashing = true;
        myCollider.height /= 2;
        myCollider.center = new Vector3(myCollider.center.x, myCollider.center.y - (myCollider.height / 2), myCollider.center.z);

        circularVelocity = Vector3.ClampMagnitude(circularVelocity * 5.0f, moveSpeed * 1.5f);
        float currentDrag = horizontalDrag;
        dashing = true;        
        float counter = 0;
        while (counter < duration)
        {
            horizontalDrag = 0.9f;
            counter += Time.deltaTime;

            if (counter * 2.0f > duration && horizontalDrag >= 0.9f)
            {
                horizontalDrag = currentDrag / 2;
            }
            yield return null;
        }
        dashing = false;
        horizontalDrag = currentDrag;

        myCollider.center = new Vector3(myCollider.center.x, myCollider.center.y + (myCollider.height / 2), myCollider.center.z);
        myCollider.height *= 2;
        isDashing = false;
    }
    private void WallJump(float wallDirection)
    {
        float jumpMultiplier = 0.8f;
        if (wallDirection > 0)
        {
            if (previousWallJumpDirection > 0)
            {
                jumpMultiplier = 0.3f;
            }
            else
            {
                previousWallJumpDirection = 1;
            }
        }
        if (wallDirection < 0)
        {
            if (previousWallJumpDirection < 0)
            {
                jumpMultiplier = 0.3f;
            }
            else
            {
                previousWallJumpDirection = -1;
            }
        }
        rb.velocity = Vector3.up * jumpForce * jumpMultiplier;
        circularVelocity *= -2.0f;
    }
    private IEnumerator Attack(float duration)
    {
        isAttacking = true;
        touchedWallDirection = 0;
        float counter = 0;
        bool brokeSomething = false;
        while (counter < duration)
        {
            if (!brokeSomething)
            {
                if (Physics.Raycast(avatarModel.transform.position + new Vector3(0.0f, myCollider.height / 2, 0.0f), avatarModel.transform.forward, out RaycastHit hit, 2.0f))
                {
                    if (hit.transform.gameObject.GetComponent<BreakableController>() != null)
                    {
                        if(!hit.transform.gameObject.GetComponent<BreakableController>().isBroken)
                        {
                            AddScore(50);
                            hit.transform.gameObject.GetComponent<BreakableController>().GetBroken();
                            brokeSomething = true;
                        }
                        
                    }
                }
                else
                {
                    Debug.DrawRay(avatarModel.transform.position + new Vector3(0.0f, myCollider.height/2, 0.0f), avatarModel.transform.forward * 2.0f, Color.white);
                }
            }
            counter += Time.deltaTime;
            yield return null;
        }
        isAttacking = false;
    }

    private IEnumerator AttackOnMeteor(Transform fromPosition, GameObject meteor, float duration)
    {
        //Make sure there is only one instance of this function running
        if (playerState == 2)
        {
            yield break; ///exit if this is still running
        }
        playerState = 2;
        gui.ScaleBlackBars(50.0f, 0.5f);
        
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
        playerState = 1;
        CameraController.SwitchToMainCamera();
        gui.ScaleBlackBars(0.0f, 0.5f);

        if (timingGrade > 0)
        {
            holdingSword = 0;
            EquipSword(0);
            gui.UpdateEquipmentUI("EQUIP: -");

            meteorsDestroyed++;
            gui.UpdateMeteorsDestroyed(meteorsDestroyed);
            Destroy(meteor);
            if (timingGrade >= 2)
            {
                ResetBreakables(); //Reset only if timing grade was EXCELLENT
            }
        }
        else
        {
            TakeDamage(1);
        }
        
    }

    private void PickUpSword(GameObject pickedUpSword)
    {
        gui.TogglePlayerActionText(false, "");
        holdingSword = pickedUpSword.GetComponent<SwordController>().swordID;
        EquipSword(holdingSword);
        gui.UpdateEquipmentUI("EQUIP: Sword " + holdingSword);
        Destroy(pickedUpSword);

    }
    private void SwitchSwords(GameObject pickedUpSword)
    {
        GameObject swordToSpawn = new GameObject();
        switch (holdingSword)
        {
            default:
                swordToSpawn = zanbato;
                break;
            case 2:
                swordToSpawn = broadsword;
                break;
            case 3:
                swordToSpawn = katana;
                break;
        }
        holdingSword = pickedUpSword.GetComponent<SwordController>().swordID;
        EquipSword(holdingSword);
        gui.UpdateEquipmentUI("EQUIP: Sword " + holdingSword);

        Destroy(pickedUpSword);

        Instantiate(swordToSpawn, transform.position + new Vector3(0.0f, 1.0f, 0.0f), transform.rotation);
        gui.TogglePlayerActionText(false, "");
    }

    private void DropSword()
    {
        GameObject swordToSpawn = new GameObject();
        switch (holdingSword)
        {
            default:
                swordToSpawn = zanbato;
                break;
            case 2:
                swordToSpawn = broadsword;
                break;
            case 3:
                swordToSpawn = katana;
                break;
        }

        holdingSword = 0;
        EquipSword(0);
        gui.UpdateEquipmentUI("EQUIP: -");
        Instantiate(swordToSpawn, transform.position + new Vector3(0.0f, 1.0f, 0.0f), transform.rotation);
    }

    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
        gui.UpdateHealthUI(playerHealth);

        if (playerHealth <= 0)
        {
            GameOver(); //From health loss
        }
    }
    public void AddScore(int amount)
    {
        playerScore += amount;
        gui.AnimateScore(amount);
        gui.UpdateScoreUI(playerScore);
    }

    private void GameOver()
    {
        playerState = 3;
        CameraController.SwitchToEndingCamera();
        gui.TogglePlayerActionText(false, "");
        gui.TogglePrompt(false, "");
        gui.ShowGameOverUI(3.0f, 2.0f); //This ends with unlocking the menu        
    }
    private void UpdateAnimations()
    {
        if (pauseMenu.Paused() == false)
        {
            avatarModel.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + avatarModelRotation, transform.eulerAngles.z);
        }
        

        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            if (avatarModelRotation < 90.0f) avatarModelRotation += 30.0f;
            running = true;
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            if (avatarModelRotation > -90.0f) avatarModelRotation -= 30.0f;
            running = true;
        }
        else
        {
            running = false;
        }
        anim.SetBool("running", running);
        anim.SetBool("dashing", dashing);
    }
}
