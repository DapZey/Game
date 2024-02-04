using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class MovementManager : MonoBehaviour
{
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

    float gravityOnPlayer = -20f;
    Vector3 playerMovementVelocity;
    bool grounded;
    float jumpVelocity;
    float movementSpeed;
    bool isMovementLocked = false;
    public CinemachineBrain cinemachineBrain;
    
    
    void Start()
    {
        playerInputController = GetComponent<CharacterController>();
        jumpVelocity = Mathf.Sqrt(-2 * gravityOnPlayer * jumpHeight);
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void Update()
    {
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

    void MovePlayerFromDirection()
{
    if (!isMovementLocked){
        playerHorizontalInput = 0;
    playerVerticalInput = 0;

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

    movementSpeed = IsPlayerOnGround() ? playerMovementSpeed : playerMovementSpeed * 0.9f;

    playerMovementDirection = (transform.forward * playerVerticalInput + transform.right * playerHorizontalInput).normalized;

    playerInputController.Move(playerMovementDirection * movementSpeed * Time.deltaTime);
    } else {
        Debug.Log("teleportning...");
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

    // Check for input keys
    if (Input.GetKey(KeyCode.W))
        dashDirection += transform.forward;
    if (Input.GetKey(KeyCode.D))
        dashDirection += transform.right;
    if (Input.GetKey(KeyCode.S))
        dashDirection -= transform.forward;
    if (Input.GetKey(KeyCode.A))
        dashDirection -= transform.right;

    // If no directional input, default to forward
    if (dashDirection.magnitude == 0)
        dashDirection = transform.forward;

    return dashDirection;
}
}
