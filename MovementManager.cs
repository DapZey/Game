using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public float playerMovementSpeed = 5;
    public float dashSpeed = 15;
    public float dashCooldown = 1;
    private float dashCooldownTimer = 0;

    float playerHorizontalInput;
    float playerVerticalInput;
    [HideInInspector] public Vector3 playerMovementDirection;

    public float jumpHeight;
    CharacterController playerInputController;

    float distanceFromGround;
    [SerializeField] LayerMask playerGroundMask;

    float gravityOnPlayer = -20f;
    Vector3 playerMovementVelocity;

    void Start()
    {
        playerInputController = GetComponent<CharacterController>();
    }

    void Update()
    {
        MovePlayerFromDirection();
        PlayerGroundGravityAffect();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerVerticalJumpMovement();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash());
        }

        dashCooldownTimer -= Time.deltaTime;
         float playerOverallMovementSpeed = playerMovementDirection.magnitude * playerMovementSpeed;

    // Print out the player's overall movement speed
    Debug.Log("Player's overall movement speed: " + playerOverallMovementSpeed);
    }

    void MovePlayerFromDirection()
{
    float playerHorizontalInput = 0;
    float playerVerticalInput = 0;

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

    float movementSpeed = IsPlayerOnGround() ? playerMovementSpeed : playerMovementSpeed * 0.9f;

    playerMovementDirection = (transform.forward * playerVerticalInput + transform.right * playerHorizontalInput).normalized;

    playerInputController.Move(playerMovementDirection * movementSpeed * Time.deltaTime);
}
    void PlayerVerticalJumpMovement()
    {
        if (IsPlayerOnGround())
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravityOnPlayer * jumpHeight);
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
        if (!IsPlayerOnGround())
        {
            playerMovementVelocity.y += gravityOnPlayer * Time.deltaTime;
        }
        else if (playerMovementVelocity.y < 0)
        {
            playerMovementVelocity.y = -2f;
        }

        playerInputController.Move(playerMovementVelocity * Time.deltaTime);
    }

    IEnumerator Dash()
{
    dashCooldownTimer = dashCooldown;

    float dashDuration = 0.3f;
    float elapsedTime = 0;

    Vector3 startPosition = transform.position;
    Vector3 dashDirection = Vector3.zero;

    // Store the initial velocity of the player
    Vector3 initialVelocity = playerMovementVelocity;

    if (Input.GetKey(KeyCode.W))
    {
        dashDirection += transform.forward;
    }
    if (Input.GetKey(KeyCode.D))
    {
        dashDirection += transform.right;
    }
    if (Input.GetKey(KeyCode.S))
    {
        dashDirection -= transform.forward;
    }
    if (Input.GetKey(KeyCode.A))
    {
        dashDirection -= transform.right;
    }

    if (dashDirection.magnitude > 0)
    {
        dashDirection = dashDirection.normalized;

        // Apply gravity to the stored initial velocity
        initialVelocity.y += gravityOnPlayer * dashDuration;

        while (elapsedTime < dashDuration)
        {
            // Move the player using CharacterController.Move
            playerInputController.Move(dashDirection * 0.08f * (dashDuration - elapsedTime) / dashDuration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Reset the player's velocity after the dash
        playerMovementVelocity = initialVelocity;
    }
}
}