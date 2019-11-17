using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public int sceneLevel = 0;
    public static int currentLevel;
    public Transform worldOrigin;
    public Transform meteorWorldOrigin;
    public Transform playerOrigin;
    public TutorialManager tutorialManager;
    public MeteorManager meteorManager;
    public SwordManager swordManager;
    [Header("LINKS TO GUI")]
    public GUIController gui;
    public GameObject timingWindow;
    [HideInInspector] public int timingGrade;

    public static float worldRadius; //Accessed by a LOT of different scripts
    public static float worldHeight;

    [Header("PLAYER MOVEMENT SETTINGS")]
    public float moveSpeed = 40.0f;
    private float defaultMoveSpeed;
    public float horizontalDrag = 0.8f;
    public float jumpForce = 10.0f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.0f;
    private Rigidbody rb;
    private CapsuleCollider myCollider;

    private Vector3 circularVelocity;
    private int previousWallJumpDirection;
    private int holdingSword; //0 is nothing, 1 - 3 are the different types
    private bool isAttacking;
    private bool isDashing;
    private bool prone;

    [Header("DEBUGGING TOOLS")]
    [SerializeField] private bool isGrounded; //Make these
    [SerializeField] private bool canDoubleJump; //public when
    [SerializeField] private float touchedWallDirection; //you're debugging.
    [SerializeField] private GameObject targetedMeteor;
    [SerializeField] private GameObject targetedSword;

    [Header("PREFABS")]
    public GameObject zanbato;
    public GameObject broadsword;
    public GameObject katana;

    [Header("GAME SETTINGS")]
    public int playerMaxHealth = 3;
    private int playerHealth;
    private int playerScore;
    private int meteorsDestroyed;
    public static int maxMeteorsForLevel = 0;
    public static float lowestMeteorPosition;
    private float targetedMeteorDistance;
    public float meteorAttackRange = 200.0f;
    private float initialDeathDelay = 1.0f;
    public float meteorDeathThreshold = 100.0f;
    public static int playerState; //1 = running, 2 = attacking, 3 = game over
    private Vector3 startingPosition;

    [Header("LINKS TO MODEL AND ANIMATIONS")]
    public GameObject avatarModel;
    private float avatarModelRotation;
    public Animator anim;
    private bool running, dashing;

    public GameObject equippedZanbato;
    public GameObject equippedBroadsword;
    public GameObject equippedKatana;    

    [Header("SOUND")]
    public AudioClip jumpSound;
    public AudioClip wallJumpSound;
    public AudioClip swordEquipSound;
    public AudioClip explosionSound;
    public AudioClip attackSound;

    private AudioSource audioSource;


    void Awake()
    {
        currentLevel = sceneLevel;
        worldRadius = Vector3.Distance(transform.position, worldOrigin.position);
        worldHeight = 300.0f;
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();
        startingPosition = transform.position;
        defaultMoveSpeed = moveSpeed;
        ResetLevel();
    }
    void Update()
    {
        switch (playerState)
        {
            case 3:
                rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
                break; //No fall multiplier for a floaty death is totally intentional
            case 2:
                avatarModel.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180.0f, transform.eulerAngles.z);
                rb.velocity = Vector3.up * 0;
                touchedWallDirection = 0;
                isGrounded = false;
                canDoubleJump = false;
                if (Input.GetButtonDown("buttonX") && !TimingWindow.gotPressed)
                {
                    TimingWindow.gotPressed = true;
                }
                gui.TogglePrompt(false, "");
                gui.HidePlayerActionText();
                break;

            default:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TakeDamage(1); //USE THIS TO TEST DAMAGE TAKING
                }
                if (Input.GetKeyDown(KeyCode.G))
                {
                    AddScore(transform.position, 10); //USE THIS TO TEST DAMAGE TAKING
                }

                TrackLowestMeteor();
                worldOrigin.LookAt(new Vector3(transform.position.x, worldOrigin.position.y, transform.position.z));
                transform.rotation = playerOrigin.rotation = worldOrigin.rotation;
                playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * worldRadius);
                transform.position = new Vector3(playerOrigin.position.x, transform.position.y, playerOrigin.position.z);

                var moveDrag = horizontalDrag;
                if(isGrounded && Input.GetAxisRaw("Horizontal") == 0 && !dashing)
                {
                    moveDrag /= 4;
                }
                if (holdingSword == 0)
                {
                    moveSpeed = defaultMoveSpeed * 1.25f;
                }
                else
                {
                    moveSpeed = defaultMoveSpeed;
                }

                if (touchedWallDirection != 0 || isDashing) circularVelocity -= transform.right * Input.GetAxisRaw("Horizontal") * moveSpeed / 40;
                else circularVelocity -= transform.right * Input.GetAxisRaw("Horizontal") * moveSpeed / 8;
                circularVelocity = Vector3.ClampMagnitude(circularVelocity, moveSpeed * 2) * moveDrag;

                if (prone) circularVelocity = new Vector3(0.0f, circularVelocity.y, 0.0f);

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


                //Button controls work when NOT prone.
                if (!prone && !PauseMenu.GameIsPaused) {

                    targetedMeteorDistance = CheckAboveForMeteor();

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
                                audioSource.PlayOneShot(jumpSound);
                            }
                        }
                        else
                        {
                            isGrounded = false;
                            rb.velocity = Vector3.up * jumpForce;
                            //audioSource.PlayOneShot(jumpSound);
                        }
                    }
                    if(Input.GetButtonDown("buttonB"))
                    {
                        if (!isDashing && Input.GetAxisRaw("Horizontal") != 0)
                        {
                            StartCoroutine(StartDashing(0.4f));
                        }
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
                            audioSource.PlayOneShot(swordEquipSound);
                        }
                        else if (holdingSword != 0)
                        {
                            DropSword();
                        }
                    }
                    
                    if (Input.GetButtonDown("buttonX"))
                    {
                        if (holdingSword != 0 && !isAttacking)
                        {
                            StartCoroutine(Attack(0.5f));
                        }
                    }
                    if (Input.GetButton("buttonX") && Input.GetButton("Jump"))
                    {
                        if (holdingSword != 0 && targetedMeteor != null && targetedMeteorDistance < meteorAttackRange)
                        {
                            StartCoroutine(AttackOnMeteor(transform, targetedMeteor, 3.0f));
                        }
                    }
                }

                if (!PauseMenu.GameIsPaused)
                {
                    gui.UpdatePlayerMarker(transform.position);
                    UpdateAnimations();
                }

                if (playerState == 4)
                {
                    /*if (Input.GetButtonDown("buttonX") && GUIController.menuUnlocked)
                    {
                        RestartLevel();
                    }*/
                }
                break;
        }
        
    }
    public void RestartLevel()
    {
        meteorManager.ResetMeteors();
        swordManager.ResetSwords();
        ResetLevel();
        tutorialManager.ResetTutorial();
    }

    private float CheckAboveForMeteor()
    {
        if (Physics.Raycast(transform.position, transform.up, out RaycastHit hit, worldHeight))
        {
            if (hit.transform.gameObject.CompareTag("Meteor"))
            {

                targetedMeteor = hit.transform.gameObject;
                //Debug.Log("meteor is " + hit.distance + " away");

                if (hit.distance > meteorAttackRange)
                {
                    //don't use targetedMeteorDistance BEFORE you set it...
                    gui.UpdateMeteorHeightUI(hit.distance - meteorAttackRange, holdingSword);
                    //gui.TogglePrompt(true, "It's still too far!");
                    //also indicate that it's the lowest one or NOT.
                }
                else
                {
                    if (holdingSword == 0)
                    {
                        gui.UpdateMeteorHeightUI(0.0f, holdingSword);
                        gui.TogglePrompt(true, "You need a sword!");
                    }
                    else
                    {
                        gui.UpdateMeteorHeightUI(0.0f, holdingSword);
                        gui.TogglePrompt(true, "Attack Meteor", "buttonsXA");
                    }
                }

                return hit.distance;
            }
            else
            {
                gui.UpdateMeteorHeightUI(0.0f, holdingSword);
                //Debug.Log("something is " + hit.distance + " away");

                targetedMeteor = null;
                gui.TogglePrompt(false, "");
                gui.UpdateMeteorHeightUI(0.0f, holdingSword);

                return worldHeight;
            }
        }
        else
        {
            //nothing was spotted...
            gui.UpdateMeteorHeightUI(0.0f, holdingSword);

            targetedMeteor = null;
            gui.TogglePrompt(false, "");
            gui.UpdateMeteorHeightUI(0.0f, holdingSword);

            return worldHeight;
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
        gui.UpdateEquipmentUI(swordID);
    }

    private void ResetLevel()
    {
        playerHealth = playerMaxHealth;
        playerScore = 0;
        meteorsDestroyed = 0;
        gui.HidePlayerActionText();
        gui.ResetGUI();
        gui.UpdateHealthUI(playerHealth);
        gui.UpdateScoreUI(playerScore);
        gui.UpdateMeteorsDestroyed(meteorsDestroyed);
        ResetBreakables();

        transform.position = startingPosition;
        avatarModelRotation = 0.0f;
        CameraController.SwitchToMainCamera();

        playerState = 1;
        holdingSword = 0;
        EquipSword(0);
        isGrounded = false;
        canDoubleJump = false;
        touchedWallDirection = 0;
        prone = false;

        initialDeathDelay = 1.0f;
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
        meteorManager.currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");
        
        if (meteorManager.currentMeteors.Length <= 0)
        {
            gui.lowestMeteorMarker.gameObject.SetActive(false);
            gui.UpdateMeteorDirectionUI(0, 0, new Vector3());
        }
        else
        {
            gui.lowestMeteorMarker.gameObject.SetActive(true);

            //Debug.Log("currently there are " + meteorManager.currentMeteors.Length);

            lowestMeteorPosition = worldHeight; //the current height at which meteors spawn

            foreach (GameObject meteor in meteorManager.currentMeteors)
            {
                if (meteor == null)
                {
                    //lowestMeteorPosition = worldHeight;
                    //meteorManager.currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");
                    //Debug.Log("caught a null"); //not being called for the missing
                    return;
                }
                else
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
                            gui.UpdateMeteorDirectionUI(-1, leftAngleDifference, meteor.transform.position);
                        }
                        else
                        {
                            gui.UpdateMeteorDirectionUI(1, rightAngleDifference, meteor.transform.position);
                        }
                    }
                    else
                    {
                        meteor.GetComponent<MeteorController>().isLowest = false; //All other meteors is made false
                    }
                    gui.UpdateMeteorLandingUI(lowestMeteorPosition, meteorDeathThreshold, worldHeight - meteorDeathThreshold);

                    if (initialDeathDelay > 0)
                    {
                        initialDeathDelay -= Time.deltaTime;
                    }
                    else
                    {
                        if (lowestMeteorPosition < meteorDeathThreshold)
                        {
                            lowestMeteorPosition = worldHeight;
                            GameOver(ResultsMenu.METEOR_DEATH); //From a meteor landing

                        }
                    }
                }
            }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<FlyingMeteorController>() != null)
        {
            TakeDamage(1);
        }
        else if(collision.GetContact(0).normal.y > 0.5f)
        {
            prone = false;
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
                //targetedMeteor = other.gameObject;
                //targetedMeteor.GetComponent<MeteorController>().withinAttackRange = true;                
            }

            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = other.gameObject;
                if(holdingSword == 0)
                {
                    gui.TogglePlayerActionText(targetedSword, holdingSword);
                }
                else
                {
                    gui.TogglePlayerActionText(targetedSword, holdingSword);
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
                //targetedMeteor = null;
                //gui.TogglePrompt(false, "");
            }
            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = null;
                gui.HidePlayerActionText();
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
            if (Mathf.Abs(rb.velocity.y) > 10.0f) {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 2.0f, rb.velocity.z);
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
        audioSource.PlayOneShot(wallJumpSound);
        circularVelocity *= -2.0f;
    }
    private IEnumerator Attack(float duration)
    {
        isAttacking = true;
        touchedWallDirection = 0;
        float counter = 0;
        bool brokeSomething = false;

        audioSource.PlayOneShot(attackSound);
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
                            AddScore(hit.transform.position, 50);
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
        gui.ScaleBlackBars(70.0f, 0.5f);
        
        //Get the current position of the object to be moved
        Vector3 startPos = fromPosition.position;
        Vector3 toPosition = meteor.transform.position;
        CameraController.SwitchToAttackCamera();

        float counter = 0;
        timingGrade = 0;
        timingWindow.GetComponent<TimingWindow>().StartTimingWindow(duration, holdingSword);

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }
        avatarModelRotation = 0.0f;
        playerState = 1;
        CameraController.SwitchToMainCamera();
        gui.ScaleBlackBars(0.0f, 0.5f);

        if (timingGrade > 0)
        {
            if (meteor.GetComponent<MeteorController>().meteorID == 0 || holdingSword == meteor.GetComponent<MeteorController>().meteorID)
            {
                gui.RemoveMinimapMeteor(meteor);
                Destroy(meteor);
                
                meteorsDestroyed++;
                gui.UpdateMeteorsDestroyed(meteorsDestroyed);
                audioSource.PlayOneShot(explosionSound);

                if (TutorialManager.tutorialActive)
                {
                    if (currentLevel == 2 || currentLevel == 3)
                    {
                        if (!tutorialManager.firstActionCleared)
                        {
                            tutorialManager.firstActionCleared = true;
                            meteorManager.SpawnSpecialMeteor();
                        }
                        else if (!tutorialManager.secondActionCleared)
                        {
                            tutorialManager.secondActionCleared = true;
                            meteorManager.SpawnMeteor();
                        }
                    }
                    else
                    {
                        tutorialManager.secondActionCleared = true;
                        meteorManager.SpawnMeteor();
                    }
                }
            }
            else
            {
                prone = true;
                gui.TogglePrompt(true, "What?! It didn't work!");
            }
            
            holdingSword = 0;
            EquipSword(0);

            //Debug.Log(meteorsDestroyed + "/" + maxMeteorsForLevel);
            if (meteorsDestroyed >= maxMeteorsForLevel)
            {
                //Debug.Log("YOU WIN!");
                GameClear();
            }

            ResetBreakables();
            if (timingGrade >= 3)
            {
                //ResetBreakables(); //Reset only if timing grade was EXCELLENT
                AddScore(transform.position, 500);
            }
            else
            {
                AddScore(transform.position, timingGrade * 100);
            }
        }
        else
        {
            TakeDamage(1);
            prone = true;

        }
    }

    private void PickUpSword(GameObject pickedUpSword)
    {
        gui.HidePlayerActionText();
        holdingSword = pickedUpSword.GetComponent<SwordController>().swordID;
        EquipSword(holdingSword);
        Destroy(pickedUpSword);

        if (TutorialManager.tutorialActive && currentLevel == 1)
        {
            tutorialManager.firstActionCleared = true;
        }
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
        gui.HidePlayerActionText();

        Destroy(pickedUpSword);
        Instantiate(swordToSpawn, transform.position, transform.rotation);
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
        
        Instantiate(swordToSpawn, transform.position, transform.rotation);
    }

    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
        gui.FlashRed();
        gui.UpdateHealthUI(playerHealth);

        if (playerHealth <= 0)
        {
            GameOver(ResultsMenu.HEALTH_DEATH); //From health loss
        }
    }
    public void AddScore(Vector3 floatPosition, int amount)
    {
        playerScore += amount;
        gui.AnimateScore(floatPosition, amount);
        gui.UpdateScoreUI(playerScore);
    }

    private void GameOver(int resultType)
    {
        playerState = 3;
        CameraController.SwitchToEndingCamera();
        gui.HidePlayerActionText();
        gui.TogglePrompt(false, "");
        gui.ShowResults(resultType, 3.0f, 2.0f); //This ends with unlocking the menu
    }
    private void GameClear()
    {
        playerState = 4;
        gui.HidePlayerActionText();
        gui.TogglePrompt(false, "");
        gui.ShowResults(ResultsMenu.VICTORY, 3.0f, 2.0f); //This ends with unlocking the menu        
    }

    private void UpdateAnimations()
    {
        avatarModel.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + avatarModelRotation, transform.eulerAngles.z);
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
        anim.SetBool("attacking", isAttacking);
        anim.SetBool("doublejumping", !canDoubleJump);
        anim.SetBool("jumping", isGrounded);

    }
}
