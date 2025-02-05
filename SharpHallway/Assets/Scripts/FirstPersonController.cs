﻿// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AK;
using UnityEngine.SceneManagement;
using System.IO;
using System.Reflection;
//using System.Diagnostics;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{

    private Rigidbody rb;

    #region Camera Movement Variables
    [Header("Camera Movement")]
    public Camera playerCamera;

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

    #region Camera Zoom Variables
    [Header("Camera Zoom")]

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion
    #endregion

    #region Movement Variables
    [Header("Movement")]
    public bool playerCanMove = true;
    public bool footsteps = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;
    private bool hittingWall = false;


    // Internal Variables
    private bool isWalking = false;

    #region Sprint
    [Header("Sprint")]
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

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump
    [Header("Jump")]
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;

    #endregion

    #region Crouch
    [Header("Crouch")]

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion
    #endregion

    #region Head Bob

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    #endregion

    #region WWise Events and such

    [Header("WWise Events")]
    [SerializeField] public AK.Wwise.Event walkFootstep;
    [SerializeField] public AK.Wwise.Event stomp;
    public bool enableFrontRearVolumeIncrease = true; // turn this off if the volume increase is messing with front-rear panning for footsteps
    public float stepInterval = 0.1f; // a variable to determine interval between steps
    public float stepTimer = 0f;    // tracks how much time passed since last footstep sound
    float inputThreshold = 0.1f; // Define a small threshold so that footsteps stop when player stops, no delay
    float currentSpeed; // variable to calculate the current speed
    float stepSlow = 1.8f;  // variable to slow footsteps down in accordance with headbob
    public float maxReverbDistance = 10f; // Maximum distance for reverb effect
    private float distanceToWall; // Distance to the closest wall


    #endregion




    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }


    }

    void Start()
    {
       
  

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (crosshair)
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

        if (useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (hideBarWhenFull)
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

    }

    float camRotation;

    private void Update()
    {
        UpdateWallDistance();

        #region MAPP Team Additions (errors here)

        #endregion

        #region Camera

        // Control camera movement
        if (cameraCanMove)
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
            playerCamera.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
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
            if (holdToZoom && !isSprinting)
            {
                if (Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if (Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if (isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if (enableSprint)
        {
            if (isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if (!unlimitedSprint)
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
            if (isSprintCooldown)
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
            if (useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch)
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

        #endregion

        CheckGround();

        if (enableHeadBob)
        {
            HeadBob();
        }

        #region Footsteps

        if (!hittingWall)
        {
            if (Input.GetKey(KeyCode.LeftShift)) // Assuming LeftShift is the run key
            {
                currentSpeed = sprintSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            // Where we change the time between footsteps, increases if running
            float stepInterval2 = stepInterval * (walkSpeed / currentSpeed) * stepSlow;

            // check if input from player
            // vertical = W , S
            // horizontal = A , D

            if (Mathf.Abs(Input.GetAxis("Vertical")) > inputThreshold || Mathf.Abs(Input.GetAxis("Horizontal")) > inputThreshold)
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval2 && footsteps == true)
                {
                    walkFootstep.Post(gameObject);
                    stepTimer = 0;
                }
            }
            else
            {
                stepTimer = 0;
            }
        }
        #endregion

        if (isWalking == false && isSprinting == false && Input.GetKeyDown(KeyCode.Q))
        {
            stomp.Post(gameObject);
        }
    }

    void FixedUpdate()
    {


        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if ((targetVelocity.x != 0 || targetVelocity.z != 0) && isGrounded)
            {
                isWalking = true;

            }
            else
            {
                isWalking = false;

            }

            // All movement calculations while sprint is active
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (velocityChange.x != 0 || velocityChange.z != 0)
                {
                    isSprinting = true;

                    if (isCrouched)
                    {
                        Crouch();
                    }

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.deltaTime;
                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        #endregion


    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if (isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if (isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    private void HeadBob()
    {
        if (isWalking)
        {
            // Calculates HeadBob speed during sprint
            if (isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Hallway/Walls/LeftWall" || collision.gameObject.tag == "Hallway/Walls/RightWall" ||
            collision.gameObject.tag == "Hallway/Walls/FarWall" || collision.gameObject.tag == "Hallway/Walls/CloseWall" ||
            collision.gameObject.tag == "Book" || collision.gameObject.tag == "Door")
        {
            hittingWall = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Hallway/Walls/LeftWall" || collision.gameObject.tag == "Hallway/Walls/RightWall" ||
            collision.gameObject.tag == "Hallway/Walls/FarWall" || collision.gameObject.tag == "Hallway/Walls/CloseWall" ||
            collision.gameObject.tag == "Book" || collision.gameObject.tag == "Door")
        {
            hittingWall = false;
        }
    }


    private void UpdateWallDistance()
    {
        float sphereRadius = 0.75f; // Sensitivity for obstacle proximity detection
        Vector3 origin = transform.position;

        // Define raycast lengths, adjust when needed
        float leftRayLength = 7f; // Distance to check to the left
        float rightRayLength = 7f; // Distance to check to the right
        float forwardRayLength = 7f; // Distance to check forward
        float backwardRayLength = 7f; // Distance to check backward

        // Define the ray directions relative to the player
        Vector3 leftRayDirection = -transform.right; // Left direction (negative right vector)
        Vector3 rightRayDirection = transform.right;  // Right direction (positive right vector)
        Vector3 forwardRayDirection = transform.forward; // Forward direction
        Vector3 backwardRayDirection = -transform.forward; // Backward direction

        // Raycast to detect obstacles on the left, right, forward, backward. Use Sphere Cast to be able to detect smaller obstacles
        RaycastHit leftHit, rightHit, forwardHit, backwardHit;
        bool leftWallDetected = Physics.SphereCast(origin, sphereRadius, leftRayDirection, out leftHit, leftRayLength);
        bool rightWallDetected = Physics.SphereCast(origin, sphereRadius, rightRayDirection, out rightHit, rightRayLength);
        bool forwardWallDetected = Physics.SphereCast(origin, sphereRadius, forwardRayDirection, out forwardHit, forwardRayLength);
        bool backwardWallDetected = Physics.SphereCast(origin, sphereRadius, backwardRayDirection, out backwardHit, backwardRayLength);

        // If a obstacle is detected on the left, right, front, or rear calculate the proximity
        float leftProximity = 0f;
        float rightProximity = 0f;
        float forwardProximity = 0f;
        float backwardProximity = 0f;
        if (leftWallDetected)
        {
            leftProximity = Mathf.Clamp01(1 - leftHit.distance / leftRayLength); // Normalize distance to 0-1
        }     
        if (rightWallDetected)
        {
            rightProximity = Mathf.Clamp01(1 - rightHit.distance / rightRayLength); // Normalize distance to 0-1
        }
        if (forwardWallDetected)
        {
            forwardProximity = Mathf.Clamp01(1 - forwardHit.distance / forwardRayLength); // Normalize distance to 0-1
        }
        if (backwardWallDetected)
        {
            backwardProximity = Mathf.Clamp01(1 - backwardHit.distance / backwardRayLength); // Normalize distance to 0-1
        }


        // Calculate reverb send volume based on proximity to obstacles 
        float reverbVolume = 0f;
        if (forwardWallDetected)
        {
            reverbVolume = Mathf.Clamp01(forwardProximity); // Normalize and map to 0-1
        }
        else if (backwardWallDetected)
        {
            reverbVolume = Mathf.Clamp01(backwardProximity); // Normalize and map to 0-1
        }
        else if (leftWallDetected || rightWallDetected)
        {
            // Lower reverb when moving sideways but still close to the obstacle
            reverbVolume = Mathf.Clamp01(Mathf.Max(leftProximity, rightProximity));
        }
        // Adjust the RTPC value for reverb send volume
        AkSoundEngine.SetRTPCValue("FootstepReverb", reverbVolume * 100); // Map proximity to reverb send volume (0-100 range)



        // Calculate left-right pan value
        float panValue = 0f;
        if (leftWallDetected && rightWallDetected)
        {
            // If both obstacles are detected, find a balance point to play through both left and right headphones
            panValue = (rightProximity - leftProximity) * 100; // -100 (left) to 100 (right)
        }
        else if (leftWallDetected)
        {
            // If only the left obstacle is detected, pan towards the left
            panValue = -leftProximity * 100;
        }
        else if (rightWallDetected)
        {
            // If only the right obstacle is detected, pan towards the right
            panValue = rightProximity * 100;
        }



        // Calculate front-rear pan value
        float frontRearValue = 0f;
        if (forwardWallDetected && backwardWallDetected)
        {
            frontRearValue = (forwardProximity - backwardProximity) * 100; // Balance between -100 (rear) to 100 (front)
        }
        else if (forwardWallDetected)
        {
            frontRearValue = forwardProximity * 100; // Pan front
        }
        else if (backwardWallDetected)
        {
            frontRearValue = -backwardProximity * 100; // Pan rear
        }



        float volumeValue = 0f;
        if (forwardWallDetected)
        {
            volumeValue = forwardProximity * 100; // Increase volume as the player gets closer to the front obstacle
        }
        else if (backwardWallDetected)
        {
            volumeValue = backwardProximity * 100; // Increase volume as the player gets closer to the back obstacle
        }



        // Send pan value to Wwise
        AkSoundEngine.SetRTPCValue("FootstepPan", panValue); // Multiply by 100 to match Wwise RTPC range
        AkSoundEngine.SetRTPCValue("FrontRearPan", frontRearValue); // Front-rear panning
        if (enableFrontRearVolumeIncrease == true)
        {
            AkSoundEngine.SetRTPCValue("ObstacleDistance", volumeValue); // Footstep volume based on proximity
        }
        


    }
}





