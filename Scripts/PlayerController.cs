using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //Camera
    [Header("Camera Related")]
    [Tooltip("Temp Mouse sensitivity")] public float sensitivity = 10;
    [Tooltip("Main Camera in player")]public Camera cam;
    [Tooltip("How Much camera comes forward looking down")]public float camOffsetForward = 0.5f;
    [Tooltip("Cam Holder usually headbone")] public Transform camHolder;

    private float camX, camY;
    private float turnSpeed = 0;
    private float lookOffsetToAchieve = 0;
    private float currentLookOffset = 0;
    private bool camCanMove = true;

    //Movement
    [Header("Movement Related")]
    public float moveSpeed;
    public float sprintStamCost;
    public AudioSource stepSound;

    private float horizontalInput, verticalInput;
    private bool crouching = false;

    private CharacterController charController;

    //Physics
    [Header("Physics Related")]
    private Rigidbody playerRB;

    //Jumping
    [Header("Jumping Related")]
    [Range(0, 4)] public float downAmount;
    public float maxJumpHoldTime;
    public float jumpStrength;
    public bool inputsOn = false;
    public bool jumpStarted = false;

    private float startJumpTime;
    private bool canJump;

    private LayerMask playerMask => ~(1 << gameObject.layer);

    private Stats stats;
    [NonSerialized] public PlayerQuestHandler questHandler;
    private DialogueHandler dialogueHandler;
    private Animator movementAnimator;
    private EquipmentHandler equipmentHandler;

    private bool weaponOut = false;
    private bool jumping = false;
    [NonSerialized] public bool onGround = false;

    public Settings settingsFile;
    private EquipmentSlot weaponSlot;

    private bool equipBack = false;

    public bool devLoadGame = false;
    private void Awake()
    {
        foreach (EquipmentSlot slot in GetComponentsInChildren<EquipmentSlot>())
        {
            if(slot.equipmentType == EquipmentType.weapon)
            {
                weaponSlot = slot;
                break;
            }
        }
            stats = GetComponent<Stats>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        dialogueHandler = GetComponentInChildren<DialogueHandler>();
        charController = GetComponent<CharacterController>();
        movementAnimator = GetComponent<Animator>();

        if (SceneManager.GetActiveScene().name == "RealMainMenu") return;


        //Gets rigidbody and if camera isin't selected select main
        playerRB = GetComponent<Rigidbody>();

        questHandler = GetComponentInChildren<PlayerQuestHandler>();

        equipmentHandler = GetComponent<EquipmentHandler>();

        Inventory.instance.settings = settingsFile;

        if (!Gamemanager.newGame || devLoadGame)
        {
            if (devLoadGame && Gamemanager.newGame) Gamemanager.newGame = false;
            StartCoroutine(Load());
        }
    }

    private void SetUi()
    {
        questHandler = GetComponentInChildren<PlayerQuestHandler>();
        dialogueHandler = GetComponentInChildren<DialogueHandler>();
    }

    private const float forwardJump = 16;
    private void Update()
    {
        if (!inputsOn)
        {
            return;
        }


        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Jump")) && canJump && !jumpStarted) // get the start jump time and add first force
        {
            jumping = true;
            canJump = false;
            downVel = Vector3.zero;
            jumpStarted = true;
            startJumpTime = Time.time;
            movementAnimator.SetTrigger("Jump");
        }
        if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Jump")) && canJump || jumping ) 
        {
            //If holding time less than max move the character up and forward if input is given
            float temp = startJumpTime + maxJumpHoldTime - Time.time;
            if(temp > 0)
            {
                float jumpForce = jumpStrength;
                charController.Move(new Vector3(0,jumpForce * Time.deltaTime, 0) + transform.forward * forwardJump * Time.deltaTime * verticalInput);
            }
            else
            {
                jumping = false;
            }  
        }
        CheckForWater();



        //if (input != 0 && canJump && !jumpStarted) // get the start jump time and add first force
        //{
        //    downVel = Vector3.zero;
        //    jumpStarted = true;
        //    startJumpTime = Time.time;
        //    movementAnimator.SetTrigger("Jump");
        //}
        //else if (input != 0 && canJump && jumpStarted) // Keeps holding down space adds more force until max hold is reached
        //{
        //    float temp = startJumpTime + maxJumpHoldTime - Time.time;
        //    if (temp > 0)
        //    {
        //        float jumpForce = jumpStrength * input;
        //        charController.Move(new Vector3(0, jumpForce * Time.deltaTime, 0));
        //    }
        //    else //Makes sure player cant continue holding down
        //    {
        //        canJump = false;
        //    }
        //}
        //else if (input == 0 && canJump && jumpStarted) //if player lets go of space disables ability to keep holding the space
        //{
        //    canJump = false;
        //}


        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Crouch"))) //Crouching By with animation
        {
            crouching = true;
            movementAnimator.SetBool("Crouch", true);
        }
        else if (Input.GetKeyUp((KeyCode)PlayerPrefs.GetInt("Crouch"))) //Uncrouching
        {
            crouching = false;
            movementAnimator.SetBool("Crouch", false);
        }

        if(dialogueHandler == null)
        {
            dialogueHandler = GetComponentInChildren<DialogueHandler>();
        }


        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Interact"))) //tries to interact with object infront
        {
            if(ContainerScript.instance != null)
            {
                if (ContainerScript.instance.current != null)
                {
                    ContainerScript.instance.CloseStorage();
                }
            }

            if(InteractableUiHandler.instance.open)
            {
                InteractableUiHandler.instance.HideUi();
            }

            if (!dialogueHandler.IsTalking()) //if player is talking then cant interact
            {
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 6, playerMask))
                {
                    Interactable temp = hit.collider.GetComponentInParent<Interactable>();
                    if (temp != null) //If the hit object has interactable then interact with it
                    {
                        temp.Interact(transform);
                    }
                }
            }

        }


        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("Sheath")))
        {
            if (!equipmentHandler.IsWeaponEquipped()) return;

            if (equipmentHandler.IsSheathed())
            {
                movementAnimator.SetBool("Sheathed", false);
                movementAnimator.SetTrigger("Draw");
            }
            else
            {
                
                movementAnimator.SetBool("Sheathed", true);
                movementAnimator.SetTrigger("PutAway");
            }
        }
    }

    private void FixedUpdate()
    {
        Move(); // Moves player
        CheckForGround(); //Checks if player is in air
    }

    private void CheckForWater()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2, playerMask))
        {
            if(hit.collider.gameObject.layer != LayerMask.NameToLayer("Water"))
            {
                movementAnimator.SetBool("Swimming", false);
                if(equipBack)
                {
                    equipBack = false;
                    weaponOut = true;
                    Weapon wep = (Weapon)weaponSlot.itemInSlot.item;
                    equipmentHandler.ChangeEquipment(wep, weaponSlot.itemInSlot.itemNum);

                }
            }
            else
            {
                if(equipmentHandler.rangedScript.currentBow != null)
                {
                    equipmentHandler.ChangeEquipment(equipmentHandler.rangedScript.currentBow, weaponSlot.itemInSlot.itemNum, true);
                    equipBack = true;
                    weaponOut = false;
                }
                else if(equipmentHandler.meleeScript.currentMelee != null)
                {
                    equipmentHandler.ChangeEquipment(equipmentHandler.meleeScript.currentMelee, weaponSlot.itemInSlot.itemNum, true);
                    equipBack = true;
                    weaponOut = false;
                }
                movementAnimator.SetBool("Swimming", true);
            }
        }
    }

    public Vector3 downVel;
    private void Move()
    {
        if(stats != null)
        {
            if (!stats.alive) return;
        }


        //Gets Axis values for WASD or arrow keys
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalFull = Input.GetAxis("Horizontal");
        float verticalFull = Input.GetAxis("Vertical");

        if(horizontalInput == 0)
        {
            horizontalFull = Mathf.Clamp(horizontalFull, -0.2f, 0.2f);
        }
        else if(verticalInput == 0)
        {
            verticalFull = Mathf.Clamp(verticalFull, -1f, 0.3f);
        }
        else
        {
            horizontalFull = Mathf.Clamp(horizontalFull, -0.5f, 0.5f);
            verticalFull = Mathf.Clamp(verticalFull, -1f, 1f);
        }

        bool running = false;
        if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Run")))
        {
            if(verticalInput == 0 && horizontalInput == 0)
            {

            }
            else if (stats.EnoughStaminaFor(sprintStamCost))
            {
                running = true;
                stats.DrainStamina(sprintStamCost * Time.deltaTime);
                horizontalFull += horizontalFull * 2;
                verticalFull += verticalFull * 2;
            }
        }

        Vector3 moveVector;
        if (!inputsOn)
        {
            horizontalInput = 0;
            verticalInput = 0;
        }
        if (crouching) //half the speed while crouching
        {
            if (verticalInput < -0.5f)
            {
                horizontalInput = Mathf.Clamp(horizontalInput, -0.5f, 0.5f);
            }
            verticalInput = Mathf.Clamp(verticalInput, -0.5f, 0.5f);
            if (equipmentHandler.IsBowEquipped() && !equipmentHandler.IsSheathed())
            {
                moveVector = -transform.right * (moveSpeed / 1.5f) * verticalInput + transform.forward * (moveSpeed / 1.5f) * horizontalInput;
            }
            else
            {
                moveVector = transform.forward * (moveSpeed / 1.5f) * verticalInput + transform.right * (moveSpeed / 1.5f) * horizontalInput;
            }

            moveVector.y = playerRB.velocity.y;
            playerRB.velocity = moveVector;
            movementAnimator.SetFloat("moveX", horizontalFull);
            movementAnimator.SetFloat("moveY", verticalFull);
        }
        else //otherwise add inputs together and move
        {
            movementAnimator.SetFloat("moveX", horizontalFull);
            movementAnimator.SetFloat("moveY", verticalFull);

            horizontalInput = Mathf.Clamp(horizontalInput, -0.65f, 0.65f);
            verticalInput = Mathf.Clamp(verticalInput, -0.65f, 1f);
        }

        if (verticalInput < -0.5f) //Slows down quite alot when going diagonal back
        {
            if (horizontalInput != 0)
            {
                verticalInput /= 1.55f;
                horizontalInput /= 1.55f;
            }
        }
        else if (verticalInput > 0.5f) //slows down only slight going diagonal forward
        {
            if (horizontalInput != 0)
            {
                verticalInput /= 1.25f;
                horizontalInput /= 1.25f;
            }
        }

        moveVector = transform.forward * moveSpeed * verticalInput + transform.right * moveSpeed * horizontalInput;

        if (!CheckForGround())
        {
            if(!jumping)
            {
                if(!charController.isGrounded)
                {
                    downVel += Physics.gravity * Time.deltaTime;

                    charController.Move(downVel);
                    movementAnimator.SetTrigger("Grounded");
                }
            }
        }
        else
        {
            downVel = Vector3.zero;
        }

        if(running)
        {
            moveVector = 2 * moveVector;
        }

        charController.Move(moveVector * Time.deltaTime);

    }

    private bool jumpDelayStarted = false;
    private bool CheckForGround()
    {
        //Sends a raycast to players feet and if it hits something player is on ground
        if (onGround && !jumpDelayStarted && !canJump && !jumping)
        {
            jumpDelayStarted = true;
            StartCoroutine(AllowJump());
            return true;
        }
        else
        {
            return onGround;
        }
    }
    private IEnumerator AllowJump()
    {
        yield return new WaitForSeconds(stats.jumpDelay);
        jumpStarted = false;
        canJump = true;
        jumpDelayStarted = false;
    }

    public void EnableCamMovement(bool allow)
    {
        camCanMove = allow;
    }

    public float turnBackToSpeed;
    private float prevTurnSpeed;
    private void LateUpdate()
    {
        if(!camCanMove || !inputsOn)
        {
            return;
        }
        if (stats != null)
        {
            if (!stats.alive) return;
        }

        //Adds axis values for mouse 
        camY += Input.GetAxisRaw("Mouse X") * (PlayerPrefs.GetFloat("Sensitivity") * 100) * Time.deltaTime; // camY is Mouse X because Y needs to be rotated when mouse is moved in horizontally
        camX -= Input.GetAxisRaw("Mouse Y") * (PlayerPrefs.GetFloat("Sensitivity") * 100) * Time.deltaTime; // camX needs to be inverted because rotating down is positive

        //Basically just does math how much player needs to turn and gives the animator that value
        turnSpeed += Input.GetAxisRaw("Mouse X") * Time.deltaTime;
        turnSpeed = Mathf.Clamp(turnSpeed, -1f, 1f);

        if(turnSpeed != 0)
        {
            if (prevTurnSpeed < 0)
            {
                if (turnSpeed > prevTurnSpeed)
                {
                    if (turnSpeed >= -0.01f)
                    {
                        turnSpeed = 0;
                    }
                }
            }
            else if (prevTurnSpeed > 0)
            {
                if (turnSpeed < prevTurnSpeed)
                {
                    if (turnSpeed <= 0.01f)
                    {
                        turnSpeed = 0;
                    }
                }
            }
            prevTurnSpeed = turnSpeed;
            turnSpeed = Mathf.Lerp(turnSpeed, 0, turnBackToSpeed * Time.deltaTime);
            movementAnimator.SetFloat("mouseX", turnSpeed);
        }

        // Clamps value so cant look upwards forever or downwards also gives animator the value
        camX = Mathf.Clamp(camX, -90, 80);
        movementAnimator.SetFloat("mouseY", camX);

        //Changes body euler angles only X and both for camera
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, camY);

        lookOffsetToAchieve = 0;
        if (currentLookOffset >= lookOffsetToAchieve && !weaponOut)
        {
            currentLookOffset = Mathf.Lerp(currentLookOffset, lookOffsetToAchieve, 2 * Time.deltaTime);
        }
        cam.transform.eulerAngles = new Vector3(camX, camY - currentLookOffset);

        //Moves the camera forward when looking downwards 
        Vector3 desiredPos = Vector3.Lerp(camHolder.position, camHolder.position + camHolder.forward * camOffsetForward, camX / 80);
        cam.transform.position = desiredPos;
    }

    public void PlayStepSound()
    {
        return; //maybe steps idk
        stepSound.Play();
    }

    public void SetBowTurn()
    {
        Quaternion rott = Quaternion.Euler(new Vector3(0, transform.rotation.y, camX) + extraOffset);
        movementAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, rott);
    }

    private Vector3 extraOffset = new Vector3(0, 7, 5);
    private void OnAnimatorIK(int layerIndex)
    {
        RangedScript rs = GetComponent<RangedScript>();
        MeleeWeapons mw = GetComponent<MeleeWeapons>();
        if(rs.currentBow == null && mw.currentMelee == null || equipmentHandler.IsSheathed())
        {
            weaponOut = false;
            movementAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.identity);
            return;
        }
        else
        {
            weaponOut = true;
        }
        Quaternion rott;
        if (rs.currentBow != null)
        {
            rott = Quaternion.Euler(new Vector3(0, transform.rotation.y, camX) + extraOffset);
        }
        else
        {
            rott = Quaternion.Euler(new Vector3(cam.transform.eulerAngles.x, 0, cam.transform.eulerAngles.z));
        }
        movementAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, rott);
    }

    public void Pause(bool close)
    {
        camCanMove = close;
    }

    public void Save()
    {
        ContainerScript.instance.CloseStorage(false);

        Stats stat = GetComponentInParent<Stats>();
        if (stat != null)
        {
            SaveSystem.SavePlayer(Inventory.instance, stat, transform.position);
        }

        Inventory.instance.ClosePauseMenu();
        Inventory.instance.AllowUi(true);
        EquipmentRarityController.SaveData();
        Pause(true);
    }
    public IEnumerator Load()
    {
        PlayerData dat = SaveSystem.LoadPlayer();
        yield return new WaitForEndOfFrame();
        if(dat != null)
        {
            Inventory.instance.LoadData();

            questHandler.LoadData(dat.questData);

            inputsOn = false;
            charController.enabled = false;

            transform.SetPositionAndRotation(new Vector3(dat.playerPosition[0], dat.playerPosition[1], dat.playerPosition[2]), transform.rotation);
        }

        inputsOn = true;
        charController.enabled = true;
    }

}
