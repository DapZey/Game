using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Cinemachine;

public class NewMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float playerMovementSpeed = 5;
    public float dashSpeed = 250f;
    public float dashCooldown = 1;
    private float dashCooldownTimer = 0;

    float playerHorizontalInput;
    float playerVerticalInput;
    [HideInInspector] public Vector3 playerMovementDirection;

    public float jumpHeight;
    CharacterController playerInputController;
    [SerializeField] LayerMask playerGroundMask;

    float gravityOnPlayer = -30f;
    Vector3 playerMovementVelocity;
    bool grounded;
    float jumpVelocity;
    float movementSpeed;
    bool isMovementLocked = false;
    public CinemachineBrain cinemachineBrain;
    private Animator anim;
    float start;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        playerInputController = GetComponent<CharacterController>();
        jumpVelocity = Mathf.Sqrt(-2 * gravityOnPlayer * jumpHeight);
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        if (playerInputController.transform.position.z > 10){
            Debug.Log(Time.time - start);
        }
        if (Input.GetKeyDown(KeyCode.W)){
            start = Time.time;
        }
        grounded = IsPlayerOnGround();
        MovePlayerFromDirection();
        PlayerGroundGravityAffect();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerVerticalJumpMovement();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(PowerfulPlayerTeleport());
        }


        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash());
        }

        dashCooldownTimer -= Time.deltaTime;
    }
public float movementLerpSpeed = 10f;
public float decelerationLerpSpeed = 0.5f; // Added deceleration lerp speed
Vector3 playerVelocity;
       
void MovePlayerFromDirection()
{
    if (!isMovementLocked)
    {
        // Input handling
        float playerVerticalInput = 0;
        float playerHorizontalInput = 0;

        if (Input.GetKey(KeyCode.W))
        {
            playerVerticalInput = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            playerVerticalInput = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            playerHorizontalInput = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            playerHorizontalInput = 1;
        }

        // Animation control
        if (playerVerticalInput == 1 && playerHorizontalInput == 0)
        {
            Debug.Log("moving forward");
            anim.SetBool("Run", true);
        }
        else
        {
            anim.SetBool("Run", false);
        }

        // Calculate movement direction and speed
        float movementSpeed = IsPlayerOnGround() ? playerMovementSpeed : playerMovementSpeed * 0.9f;
        Vector3 playerMovementDirection = (transform.forward * playerVerticalInput + transform.right * playerHorizontalInput).normalized;
        Vector3 targetVelocity = playerMovementDirection * movementSpeed;

        // Smoothly lerp between current velocity and target velocity
        playerVelocity = Vector3.Lerp(playerVelocity, targetVelocity, movementLerpSpeed * Time.deltaTime);

        // Apply movement
        playerInputController.Move(playerVelocity * Time.deltaTime);
    }
    else
    {
        Debug.Log("teleporting...");
    }

    // Decelerate when no movement keys are pressed
    if (playerVerticalInput == 0 && playerHorizontalInput == 0)
    {
        if (grounded){
            movementLerpSpeed = 15f;
        } else {
            movementLerpSpeed = 5f;
        }
        playerVelocity = Vector3.Lerp(playerVelocity, Vector3.zero, decelerationLerpSpeed * Time.deltaTime);
        playerInputController.Move(playerVelocity * Time.deltaTime);
    }
}

    void PlayerVerticalJumpMovement()
    {
        if (grounded)
        {
            playerMovementVelocity.y = jumpVelocity;
        }

        playerInputController.Move(playerMovementVelocity * Time.deltaTime);
    }

    bool IsPlayerOnGround()
    {
        return playerInputController.isGrounded;
    }

    void PlayerGroundGravityAffect()
    {
        if (!grounded)
        {
            playerMovementVelocity.y += gravityOnPlayer * Time.deltaTime;
        }
        else if (playerMovementVelocity.y < 0)
        {
            playerMovementVelocity.y = -2f;
            
        }

        playerInputController.Move(playerMovementVelocity * Time.deltaTime);
    }
    void PlayerMovementLock(float durationSeconds)
    {
        // Set movement lock status
        isMovementLocked = true;

        // If locking movement
        if (isMovementLocked)
        {
            Invoke("UnlockMovement", durationSeconds);
        }
    }

    // Method to unlock movement
    void UnlockMovement()
    {
        isMovementLocked = false;
    }
    IEnumerator PowerfulPlayerTeleport()
{
    // Lock player movement for a duration
    PlayerMovementLock(1f);

    // Wait until the lock is finished
    yield return new WaitForSeconds(1f);


    // Normalize the resulting direction vector
    if (cinemachineBrain != null && cinemachineBrain.ActiveVirtualCamera != null)
        {
            // Get the forward direction of the active virtual camera
            Vector3 cameraForward = cinemachineBrain.ActiveVirtualCamera.State.FinalOrientation * Vector3.forward;

            // Calculate teleport direction based on the camera's forward direction
            Vector3 teleportDirection = cameraForward;

            // Apply teleport velocity to move the player in the calculated direction
            playerInputController.Move(teleportDirection * 50);
        }
}
    IEnumerator Dash()
{
    dashCooldownTimer = dashCooldown;

    // Define dash parameters
    float dashDuration = 0.3f;
    Vector3 dashDirection = GetDashDirection();
    Vector3 initialVelocity = playerMovementVelocity;

    if (dashDirection.magnitude > 0)
    {
        dashDirection = dashDirection.normalized;

        // Apply gravity to the initial velocity
        initialVelocity.y += gravityOnPlayer * dashDuration;

        float elapsedTime = 0;

        while (elapsedTime < dashDuration)
        {
            // Calculate the movement distance for this frame
          Vector3 distanceToMove = 18 * dashDirection * Time.deltaTime;

            // Move the player using CharacterController.Move
            playerInputController.Move(distanceToMove);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset the player's velocity after the dash
        playerMovementVelocity = initialVelocity;
    }
}

Vector3 GetDashDirection()
{
    Vector3 dashDirection = Vector3.zero;

    // Check for input keys and update dashDirection accordingly
    dashDirection += (Input.GetKey(KeyCode.W) ? transform.forward : Vector3.zero);
    dashDirection += (Input.GetKey(KeyCode.D) ? transform.right : Vector3.zero);
    dashDirection -= (Input.GetKey(KeyCode.S) ? transform.forward : Vector3.zero);
    dashDirection -= (Input.GetKey(KeyCode.A) ? transform.right : Vector3.zero);

    // If no directional input, default to forward
    if (dashDirection.magnitude == 0)
        dashDirection = transform.forward;

    return dashDirection;
}
}
