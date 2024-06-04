// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FirstPersonController : MonoBehaviour
{
    private CharacterController controller;
    public float gravity = -9.8f;

    #region Camera Movement Variables

    public Camera playerCamera;
    public GameObject head;
    public Transform joint;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;
    #endregion

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;

    public bool isWalking = false;
    #endregion

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    public bool isSprinting = false;
    // Internal Variables
    private CanvasGroup sprintBarCG;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    public Vector3 velocity;
    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    public bool isGrounded = false;
    // Internal Variables
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = false;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float speedReduction = .5f;

    public bool isCrouched = false;
    // Internal Variables
    private Vector3 originalScale;

    #endregion

    #region Blackout
    private Image blackoutScreen;
    private float curAlpha = 0;
    private float targetAlpha = 0;
    public float FadeRate = 1f;
    private Action OnCompleteCallback = null;
    #endregion

    #region Torch

    // If hasTorch is false, torch should be null.
    public bool hasTorch;
    private GameObject torch; //The torch held by player. 
    private Transform fireAlpha;
    private Transform fireAdd;
    private Transform fireSpark;
    private Transform fireGlow;
    private Light fireLight;
    private float originalIntensity;


    public Transform leftHand;
    public LayerMask pickUpLayerMask;
    public float torchDuration = 60f;
    private float torchCurrDuration;

    public bool isAttacking;
    public float attackCost = 10f;
    
    // Hard coded, should depend on animation clip
    private float attackDuration = 2.333f;

    public float attackCooldownReset = 5f;
    private float attackCooldown = 0f;

    public TMP_Text interactText;


    #endregion


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }

        torchCurrDuration = torchDuration;
    }

    void Start()
    {

        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if(useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if(hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion

        #region Blackout
        blackoutScreen = FindObjectOfType<Image>();
        #endregion
    }

    float camRotation;

    private void Update()
    {
        #region Camera

        // Control camera movement
        if(cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        joint.position = head.transform.position;

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if(Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if(holdToZoom && !isSprinting)
            {
                if(Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if(Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if(isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if(!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if(enableSprint)
        {
            if(isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if(!unlimitedSprint)
                {
                    sprintRemaining -= 1 * Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        isSprinting = false;
                        isSprintCooldown = true;
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if(isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }

            // Handles sprintBar 
            if(useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if(enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        // Apply velocity
        controller.Move(velocity * Time.deltaTime);
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        #endregion

        #region Crouch

        if (enableCrouch)
        {
            if (!isSprinting)
            {

                if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
                {
                    Crouch();
                }

                if (Input.GetKeyDown(crouchKey) && holdToCrouch)
                {
                    isCrouched = false;
                    Crouch();
                }
                else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
                {
                    isCrouched = true;
                    Crouch();
                }
            }
        }

        #endregion

        CheckGround();

        #region Torch            
        
        float pickUpDistance = 3f;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, pickUpDistance))
        {
            if (raycastHit.transform.TryGetComponent(out WeaponGrabbable grabbable))
            {
                interactText.text = "Press E";
                if (Input.GetKeyDown(KeyCode.E) && isGrounded && !isWalking && !isSprinting)
                {
                    interactText.text = "";
                    if (!hasTorch)
                    {
                        // Copy the torch to hand
                        torch = grabbable.Grab(leftHand);
                        Transform fire = torch.transform.GetChild(0).GetChild(0);
                        fireAlpha = fire.GetChild(0);
                        fireAdd = fire.GetChild(1);
                        fireSpark = fire.GetChild(2);
                        fireGlow = fire.GetChild(3);
                        fireLight = fire.GetChild(4).GetComponent<Light>();
                        originalIntensity = fireLight.intensity;
                    } 

                    hasTorch = true;
                    torchCurrDuration = torchDuration;

                    // Destroy the original torch
                    Destroy(grabbable.gameObject);
                }
            }
        } else
        {
            interactText.text = "";
        }

        if (hasTorch)
        {
            torchCurrDuration -= Time.deltaTime;

            Vector3 lightChange = new Vector3(torchCurrDuration / torchDuration, torchCurrDuration / torchDuration, torchCurrDuration / torchDuration);
            // Reducing the torch intensity
            fireAlpha.localScale = lightChange;
            fireAdd.localScale = lightChange;
            fireGlow.localScale = lightChange;
            fireSpark.localScale = lightChange;
            fireLight.intensity = originalIntensity * torchCurrDuration / torchDuration;

        }
        if (torchCurrDuration < 0)
        {
            hasTorch = false;
            Destroy(torch);
            torch = null;
            torchCurrDuration = 0f;
        }

        if (attackCooldown == 0f && !isAttacking && hasTorch && Input.GetKeyDown(KeyCode.Mouse0))
        {
            isAttacking = true;
            torchCurrDuration -= attackCost;
        }


        if (attackCooldown == 0f && isAttacking)
        {
            attackDuration -= Time.deltaTime;

            if (attackDuration < 0)
            {
                attackCooldown = attackCooldownReset;
                attackDuration = 2.333f;
                isAttacking = false;
            }
        } else if (attackCooldown != 0f && !isAttacking)
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
            {
                attackCooldown = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isAttacking = false;
            attackCooldown = 0f;
            attackDuration = 2.333f;

            hasTorch = false;
            Destroy(torch);
            torch = null;
            torchDuration = 0f;
        }

        #endregion

        #region Blackout
        if (curAlpha != targetAlpha)
        {
            curAlpha = Mathf.MoveTowards(curAlpha, targetAlpha, Time.deltaTime);
            Color color = blackoutScreen.color;
            color.a = curAlpha;
            blackoutScreen.color = color;
            if (curAlpha == targetAlpha)
            {
                targetAlpha = 0f;
                if (OnCompleteCallback != null)
                {
                    OnCompleteCallback();
                    OnCompleteCallback = null;
                }
            }
        }
        #endregion
    }

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            // Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if (x != 0 || z != 0 && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // All movement calculations shile sprint is active
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
            {
                /*
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                */


                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (x != 0 || z != 0)
                {
                    isSprinting = true;

                    if (isCrouched)
                    {
                        // Uncrouch
                        Crouch();
                    }

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.deltaTime;
                    }
                }

                // rb.AddForce(velocityChange, ForceMode.VelocityChange);
                Vector3 move = transform.right * x + transform.forward * z;

                controller.Move(move * sprintSpeed * Time.deltaTime);
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }
                /*
                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
                */
                Vector3 move = transform.right * x + transform.forward * z;

                controller.Move(move * walkSpeed * Time.deltaTime);
            }
        }

        #endregion
    }

    /**
     * Fades the screen to black, calls the given function, and then fades the screen back
     */
    public void SetBlackout(Action onComplete)
    {
        targetAlpha = 1f;
        OnCompleteCallback = onComplete;
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            velocity.y = jumpPower;
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if(isCrouched && !holdToCrouch)
        {
            Crouch();
        }

    }

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if(isCrouched)
        {
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

}


