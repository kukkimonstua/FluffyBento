using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region VARIABLES
    public int sceneLevel = 0;
    public static int currentLevel;
    public Transform worldOrigin;
    public Transform meteorWorldOrigin;
    public Transform playerOrigin;
    public TutorialManager tutorialManager;
    public MeteorManager meteorManager;
    public SwordManager swordManager;
    public GUIController gui;
    public TimingWindow timingWindow;
    [HideInInspector] public int timingGrade;

    public static float worldRadius; //Accessed by a LOT of different scripts
    public static float worldHeight;

    [Header("PLAYER MOVEMENT SETTINGS")]
    public float moveSpeed = 20.0f;
    private float speed = 0.0f;
    public float acceleration = 10.0f;
    public float horizontalDrag = 0.5f;
    private float lastDirectionPressed = 1.0f;
    public int maxAirDashes = 2;
    private int airDashCounter;

    public float jumpForce = 16.0f;
    public float fallMultiplier = 4.0f;
    public float lowJumpMultiplier = 6.0f;
    private Rigidbody rb;
    private CapsuleCollider myCollider;

    private Vector3 circularVelocity;
    private int previousWallJumpDirection;
    private int holdingSword; //0 is nothing, 1 - 3 are the different types
    private const int NO_SWORD_EQUIPPED = 0;
    private const int ZANBATO_EQUIPPED = 1;
    private const int BROADSWORD_EQUIPPED = 2;
    private const int KATANA_EQUIPPED = 3;
    private bool isAttacking;
    private bool isDashing;
    private bool isWallJumping;
    private bool prone;

    [Header("DEBUGGING TOOLS")]
    [SerializeField] private bool isGrounded; //Make these
    [SerializeField] private bool canDoubleJump; //public when
    [SerializeField] private float touchedWallDirection; //you're debugging.
    [SerializeField] private GameObject targetedMeteor;
    [SerializeField] private GameObject targetedSword;

    [Header("GAME SETTINGS")]
    public int playerMaxHealth = 3;
    private int playerHealth;
    private int playerScore;
    public static int runningScore;
    private int meteorsDestroyed;
    public static int maxMeteorsForLevel = 0;
    public static float lowestMeteorPosition;
    private float targetedMeteorDistance;
    public float meteorAttackRange = 200.0f;
    private float initialDeathDelay = 1.0f;
    public float meteorDeathThreshold = 100.0f;
    public static int playerState; //1 = running, 2 = attacking, 3 = game over
    private Vector3 startingPosition;

    [Header("PREFABS")]
    public GameObject zanbato;
    public GameObject broadsword;
    public GameObject katana;

    [Header("LINKS TO MODEL AND ANIMATIONS")]
    public GameObject avatarModel;
    private float avatarModelRotation;
    public Animator anim;
    private bool running, wallJumping;

    public GameObject equippedZanbato;
    public GameObject equippedBroadsword;
    public GameObject equippedKatana;    

    [Header("SOUND")]
    public AudioClip jumpSound;
    public AudioClip wallJumpSound;
    public AudioClip swordEquipSound;
    public AudioClip explosionSound;
    public AudioClip attackSound;
    public AudioClip damageSound;
    private AudioSource audioSource;

    [Header("MUSIC")]
    public BGMController bgm;
    public AudioClip bgmLvl1;
    public AudioClip bgmLvl2;
    public AudioClip bgmLvl3;

    public GameObject successPrefab;
    public const int DISABLED = 0;
    public const int ACTIVELY_PLAYING = 1;
    public const int ATTACKING_METEOR = 2;
    public const int GAME_OVER = 3;
    public const int VICTORY = 4;
    #endregion

    void Awake()
    {
        currentLevel = sceneLevel;
        worldRadius = Vector3.Distance(transform.position, worldOrigin.position);
        worldHeight = 300.0f;
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();
        startingPosition = transform.position;
        ResetLevel();
    }
    void Update()
    {

        switch (playerState)
        {
            case GAME_OVER:
                rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
                break; //No fall multiplier for a floaty death is totally intentional
            case ATTACKING_METEOR:
                avatarModel.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180.0f, transform.eulerAngles.z);
                rb.velocity = Vector3.up * 0;
                touchedWallDirection = 0;
                isGrounded = false;
                canDoubleJump = false;
                if (Input.GetButtonDown("buttonX") && !timingWindow.gotPressed)
                {
                    timingWindow.TimedPress();
                }
                gui.TogglePrompt(false, "");
                gui.HidePlayerActionText();
                break;

            default: //usually "ACTIVELY_PLAYING"
                TrackLowestMeteor();

                worldOrigin.LookAt(new Vector3(transform.position.x, worldOrigin.position.y, transform.position.z));
                transform.rotation = playerOrigin.rotation = worldOrigin.rotation;
                playerOrigin.position = worldOrigin.position + (worldOrigin.transform.forward * worldRadius);
                transform.position = new Vector3(playerOrigin.position.x, transform.position.y, playerOrigin.position.z);



                //BEGIN NEW
                

                if (Input.GetAxisRaw("Horizontal") == 0 && !isDashing)
                {
                    circularVelocity *= horizontalDrag;
                    if (isGrounded) //Less slow-down in the air, important for wall jumping
                    {
                        speed *= horizontalDrag;
                    }
                }

                if (!isWallJumping)
                {
                    if (Input.GetAxisRaw("Horizontal") > 0 || Input.GetAxisRaw("Horizontal") < 0)
                    {
                        lastDirectionPressed = Input.GetAxisRaw("Horizontal"); //Used for dash and wall jump
                    }
                    speed += Input.GetAxisRaw("Horizontal") * acceleration * 0.4f * Time.deltaTime;
                    speed = Mathf.Clamp(speed, -acceleration, acceleration);

                    //Debug.Log(speed + " from adding " + Input.GetAxisRaw("Horizontal"));
                    if (Input.GetAxisRaw("Horizontal") > 0.0f && speed < 0.0f
                        || Input.GetAxisRaw("Horizontal") < 0.0f && speed > 0.0f)
                    {
                        ResetMomentum();
                    }
                }
                

                circularVelocity -= transform.right * speed;
                circularVelocity = Vector3.ClampMagnitude(circularVelocity, moveSpeed);
                //END NEW

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

                if (!PauseMenu.GameIsPaused)
                {
                    //Button controls work when NOT prone.
                    if (!prone)
                    {
                        targetedMeteorDistance = CheckAboveForMeteor();
                        
                        if (Input.GetButtonDown("Jump"))
                        {
                            if (!isGrounded)
                            {
                                if (touchedWallDirection != 0 && !isWallJumping)
                                {
                                    StartCoroutine(StartWallJumping(0.2f, touchedWallDirection));
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
                        if (Input.GetButtonDown("buttonB"))
                        {
                            if (!isDashing)
                            {
                                if (isGrounded)
                                {
                                    StartCoroutine(StartDashing(0.4f, lastDirectionPressed));
                                }
                                else if (airDashCounter > 0)
                                {
                                    airDashCounter--;
                                    StartCoroutine(StartDashing(0.4f, lastDirectionPressed));
                                }
                            }
                        }
                        if (Input.GetButtonDown("buttonY"))
                        {
                            if (targetedSword != null) //if you're near a sword
                            {
                                if (holdingSword == NO_SWORD_EQUIPPED)
                                {
                                    PickUpSword(targetedSword);
                                }
                                else
                                {
                                    SwitchSwords(targetedSword);
                                }
                                audioSource.PlayOneShot(swordEquipSound);
                            }
                            else if (holdingSword != NO_SWORD_EQUIPPED)
                            {
                                DropSword();
                            }
                        }

                        if (Input.GetButtonDown("buttonX"))
                        {
                            if (holdingSword != NO_SWORD_EQUIPPED && !isAttacking)
                            {
                                StartCoroutine(Attack(0.5f));
                            }
                        }
                        if (Input.GetButton("buttonX") && Input.GetButton("Jump"))
                        {
                            if (holdingSword != NO_SWORD_EQUIPPED && targetedMeteor != null && targetedMeteorDistance < meteorAttackRange)
                            {
                                StartCoroutine(AttackOnMeteor(transform, targetedMeteor, 3.0f));
                            }
                        }
                    }

                    gui.UpdatePlayerMarker(avatarModel.transform);
                    UpdateAnimations();
                    QuickDebugging(); //REMOVE WHEN DONE
                }
                break;
        }
        
    }
    private void ResetMomentum()
    {
        speed = 0.0f;
        circularVelocity = new Vector3(0.0f, circularVelocity.y, 0.0f);
    }
    private void QuickDebugging()
    {
#if (UNITY_EDITOR)
        if (Input.GetKeyDown(KeyCode.B))
        {
            ResetBreakables();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1); //USE THIS TO TEST DAMAGE TAKING
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddScore(transform.position, 10);
            //successPrefab.SetActive(true);
            //CameraController.cameraState = 4;
        }
#endif
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
                    gui.UpdateMeteorLandingUI(lowestMeteorPosition, meteorDeathThreshold);

                    if (initialDeathDelay > 0)
                    {
                        initialDeathDelay -= Time.deltaTime;
                    }
                    else
                    {
                        if (lowestMeteorPosition < meteorDeathThreshold && playerState == ACTIVELY_PLAYING)
                        {
                            lowestMeteorPosition = worldHeight;
                            GameOver(ResultsMenu.METEOR_DEATH); //From a meteor landing

                        }
                    }
                }
            }

        }
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
                    gui.UpdateMeteorHeightUI(hit.distance - meteorAttackRange, holdingSword);
                    gui.TogglePrompt(false, "");
                }
                else
                {
                    if (holdingSword == NO_SWORD_EQUIPPED)
                    {
                        gui.UpdateMeteorHeightUI(0.0f, NO_SWORD_EQUIPPED);
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
    private void ResetBreakables()
    {
        GameObject[] breakables = GameObject.FindGameObjectsWithTag("Breakable");
        foreach (GameObject b in breakables)
        {
            b.GetComponent<BreakableController>().GetRestored();
        }
    }

    #region COLLISIONS AND TRIGGERS
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<FlyingMeteorController>() != null)
        {
            TakeDamage(1);
        }
        else if(collision.GetContact(0).normal.y > 0.5f) //Touched the ground!
        {
            prone = false;
            isGrounded = true;
            canDoubleJump = true;
            touchedWallDirection = 0;
            previousWallJumpDirection = 0;
            airDashCounter = maxAirDashes;
        }
        else if(collision.GetContact(0).normal.x + collision.GetContact(0).normal.z != 0) //Touched a wall!
        {
            touchedWallDirection = collision.GetContact(0).normal.x + collision.GetContact(0).normal.z;
            ResetMomentum();
            if (rb.velocity.y < 0) //Clings to wall!
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
        if (playerState == ACTIVELY_PLAYING)
        {
            if (other.gameObject.CompareTag("Sword"))
            {
                targetedSword = other.gameObject;
                if(holdingSword == NO_SWORD_EQUIPPED)
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
        if (playerState == ACTIVELY_PLAYING)
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
    #endregion
    #region PLAYER ACTIONS
    private IEnumerator StartDashing(float duration, float direction)
    {
        isDashing = true;
        anim.SetBool("dashing", isDashing);
        myCollider.height /= 2;
        myCollider.center = new Vector3(myCollider.center.x, myCollider.center.y - (myCollider.height / 2), myCollider.center.z);

        moveSpeed *= 2;
        float counter = 0;
        while (counter < duration)
        {
            circularVelocity = transform.right * -direction * moveSpeed * Mathf.Clamp(0.9f - (counter / duration), 0.0f, 1.0f);

            if (Mathf.Abs(rb.velocity.y) > 10.0f)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 2.0f, rb.velocity.z);
            }

            counter += Time.deltaTime;
            yield return null;
        }
        moveSpeed /= 2;

        ResetMomentum();
        myCollider.center = new Vector3(myCollider.center.x, myCollider.center.y + (myCollider.height / 2), myCollider.center.z);
        myCollider.height *= 2;
        isDashing = false;
        anim.SetBool("dashing", isDashing);

    }
    private IEnumerator StartWallJumping(float duration, float wallDirection)
    {
        isWallJumping = true;
        wallJumping = true;
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

        ResetMomentum();
        audioSource.PlayOneShot(wallJumpSound);

        float counter = 0.0f;
        while (counter < duration)
        {
            speed = -lastDirectionPressed * moveSpeed / 3;
            circularVelocity = transform.right * -speed;

            counter += Time.deltaTime;
            yield return null;
        }
        lastDirectionPressed *= -1;

        isWallJumping = false;
        wallJumping = false;
        //circularVelocity *= -2.0f;
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

        holdingSword = NO_SWORD_EQUIPPED;
        EquipSword(NO_SWORD_EQUIPPED);
        
        Instantiate(swordToSpawn, transform.position, transform.rotation);
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
    #endregion

    private IEnumerator AttackOnMeteor(Transform fromPosition, GameObject meteor, float duration)
    {
        //Make sure there is only one instance of this function running
        if (playerState == ATTACKING_METEOR)
        {
            yield break; ///exit if this is still running
        }

        playerState = ATTACKING_METEOR;
        gui.ToggleNonTimingWindowGUI(false);
        gui.ScaleBlackBars(75.0f, 0.5f);

        //Get the current position of the object to be moved
        Vector3 startPos = fromPosition.position;
        Vector3 toPosition = meteor.transform.position;
        CameraController.SwitchToAttackCamera();

        float counter = 0;
        timingGrade = 0;
        timingWindow.StartTimingWindow(duration, holdingSword, meteor.GetComponent<MeteorController>().meteorID);

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        while (!timingWindow.eventOver)
        {
            yield return null;
        }

        //Everything after this point is one frame, setting everything to the result
        avatarModelRotation = 0.0f;
        playerState = 1;
        CameraController.SwitchToMainCamera();
        gui.ScaleBlackBars(0.0f, 0.5f);
        gui.ToggleNonTimingWindowGUI(true);

        if (timingGrade > 0)
        {
            if (meteor.GetComponent<MeteorController>().meteorID == 0 || holdingSword == meteor.GetComponent<MeteorController>().meteorID)
            {
                gui.RemoveMinimapMeteor(meteor);
                Destroy(meteor);

                meteorsDestroyed++;
                gui.UpdateMeteorsDestroyed(meteorsDestroyed);
                CameraController.cameraShakeTimer = 1.0f;
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

            holdingSword = NO_SWORD_EQUIPPED;
            EquipSword(NO_SWORD_EQUIPPED);

            Debug.Log(meteorsDestroyed + "/" + maxMeteorsForLevel);
            if (meteorsDestroyed >= maxMeteorsForLevel)
            {
                //Debug.Log("YOU WIN!");
                gui.UpdateMeteorLandingUI(worldHeight, meteorDeathThreshold);
                GameClear();
            }

            ResetBreakables();
            if (timingGrade >= 3)
            {
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

    #region GAME EVENTS
    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
        gui.FlashRed();
        gui.UpdateHealthUI(playerHealth);
        audioSource.PlayOneShot(damageSound);
        CameraController.cameraShakeTimer = 0.5f;
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
        playerState = GAME_OVER;
        CameraController.SwitchToEndingCamera();
        gui.HidePlayerActionText();
        gui.TogglePrompt(false, "");
        gui.ShowResults(playerScore, resultType, 3.0f, 2.0f); //This ends with unlocking the menu
    }
    private void GameClear()
    {
        playerState = VICTORY;
        gui.HidePlayerActionText();
        gui.TogglePrompt(false, "");
        gui.ShowResults(playerScore, ResultsMenu.VICTORY, 3.0f, 2.0f); //This ends with unlocking the menu        
    }
    #endregion
    private void UpdateAnimations()
    {
        avatarModel.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + avatarModelRotation, transform.eulerAngles.z);
        if (speed < -0.1f)
        {
            if (avatarModelRotation < 90.0f) avatarModelRotation += 30.0f;
            running = true;
        }
        else if (speed > 0.1f)
        {
            if (avatarModelRotation > -90.0f) avatarModelRotation -= 30.0f;
            running = true;
        }
        else
        {
            running = false;
        }
        anim.SetBool("running", running);
        anim.SetBool("attacking", isAttacking);
        anim.SetBool("doublejumping", !canDoubleJump);
        anim.SetBool("jumping", !isGrounded);
        anim.SetBool("wallJumping", wallJumping);
    }

    #region SCENE MANAGEMENT FUNCTIONS
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

        playerState = ACTIVELY_PLAYING;
        holdingSword = NO_SWORD_EQUIPPED;
        EquipSword(NO_SWORD_EQUIPPED);
        isGrounded = false;
        canDoubleJump = false;
        touchedWallDirection = 0;
        airDashCounter = maxAirDashes;
        prone = false;

        initialDeathDelay = 1.0f;
        if (currentLevel == 1)
        {
            bgm.FadeInMusic(bgmLvl1, 0.0f);
        }
        if (currentLevel == 2)
        {
            bgm.FadeInMusic(bgmLvl2, 0.0f);
        }
        if (currentLevel == 3)
        {
            bgm.FadeInMusic(bgmLvl3, 0.0f);
        }
    }
    public void GoToNextLevel()
    {
        playerState = DISABLED;
        StartCoroutine(FadeToNextLevel(1.0f));
    }
    private IEnumerator FadeToNextLevel(float duration)
    {
        bgm.FadeOutMusic(duration);
        gui.FadeIntoBlack(1.0f, duration);
        yield return new WaitForSeconds(duration * 1.1f);

        Debug.Log("go to next level because current level is " + currentLevel);
        if (currentLevel == 3)
        {
            GameManager.sceneIndex = GameManager.LEVEL_3_ED;
            SceneManager.LoadScene(GameManager.START_CUTSCENE);
        }
        else if (currentLevel == 2)
        {
            GameManager.sceneIndex = GameManager.LEVEL_2_ED;
            SceneManager.LoadScene(GameManager.START_CUTSCENE);
        }
        else
        {
            GameManager.sceneIndex = GameManager.LEVEL_1_ED;
            SceneManager.LoadScene(GameManager.START_CUTSCENE);
        }
    }
    public void RestartLevel()
    {
        playerState = DISABLED;
        StartCoroutine(FadeToRestart(1.0f));
    }
    private IEnumerator FadeToRestart(float duration)
    {
        Time.timeScale = 1f;
        bgm.FadeOutMusic(duration);
        gui.FadeIntoBlack(1.0f, duration);
        yield return new WaitForSeconds(duration * 1.1f);
        PauseMenu.GameIsPaused = false;

        meteorManager.ResetMeteors();
        swordManager.ResetSwords();
        ResetLevel();
        tutorialManager.ResetTutorial();
    }
    public void QuitGame()
    {
        playerState = DISABLED;
        StartCoroutine(FadeToQuit(1.0f));
    }
    private IEnumerator FadeToQuit(float duration)
    {
        Time.timeScale = 1f;
        gui.FadeIntoBlack(1.0f, duration);
        bgm.FadeOutMusic(duration);
        yield return new WaitForSeconds(duration * 1.1f);
        PauseMenu.GameIsPaused = false;

        SceneManager.LoadScene(GameManager.MAIN_MENU_INDEX);
    }
    #endregion

}
