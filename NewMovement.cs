using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

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

    float gravityOnPlayer = -20f;
    Vector3 playerMovementVelocity;
    bool grounded;
    float movementSpeed;
    bool isMovementLocked = false;
     Vector3 dashMovement;
    int framesLeftForDash = 50000001;
    float dashStartTime;
    int totalFramesForDash = 300; 
    float ms = 0.001f;
    float vectorMovedSumZ = 0;
    float vectorMovedSumX = 0;
    Vector3 dashTrajectory;
    void Start()
    {
        playerInputController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
{
    Vector3 vectorToMove = Vector3.zero;
    // Add movement from player input
    if (Input.GetKeyDown(KeyCode.LeftShift))
    {
        isMovementLocked = true;
        vectorMovedSumZ = 0;
        vectorMovedSumX = 0;
        framesLeftForDash = 0;
        dashStartTime = Time.time;
        dashTrajectory = CalculateDashMovement();
        dashMovement = dashTrajectory / totalFramesForDash;
        Debug.Log(dashTrajectory);
    }
    vectorToMove +=checkDash(vectorToMove);
    vectorToMove += MovePlayerFromDirection();
      playerInputController.Move(vectorToMove);
}
Vector3 checkDash(Vector3 vectorToMove){
    if (framesLeftForDash < totalFramesForDash)
    {
        float elapsedTime = Time.time - dashStartTime;
        float inc =(elapsedTime / ms)-framesLeftForDash;
        framesLeftForDash += Mathf.FloorToInt(inc);
        if (framesLeftForDash > 0 && vectorMovedSumZ + (dashMovement * Mathf.FloorToInt(inc)).z <= MathF.Abs(dashTrajectory.z) && vectorMovedSumZ + (dashMovement * Mathf.FloorToInt(inc)).z >= -MathF.Abs(dashTrajectory.z)
        && vectorMovedSumX + (dashMovement * Mathf.FloorToInt(inc)).x <= MathF.Abs(dashTrajectory.x) && vectorMovedSumX + (dashMovement * Mathf.FloorToInt(inc)).x >= -MathF.Abs(dashTrajectory.x)){
            
                vectorToMove += dashMovement * Mathf.FloorToInt(inc);
                vectorMovedSumZ += vectorToMove.z;
                vectorMovedSumX += vectorToMove.x;
                Debug.Log(vectorMovedSumX);
                Debug.Log(vectorMovedSumZ);
        }
        else if (framesLeftForDash > 0){
            if (vectorMovedSumZ > 0){
                vectorToMove.z += MathF.Abs(dashTrajectory.z)-vectorMovedSumZ;
            }
            else{
                vectorToMove.z += -MathF.Abs(dashTrajectory.z) + Math.Abs( vectorMovedSumZ);
            }
            if (vectorMovedSumX > 0){
                vectorToMove.x += MathF.Abs(dashTrajectory.x)-vectorMovedSumX;
            }
            else{
                vectorToMove.x += -MathF.Abs(dashTrajectory.x) + Math.Abs( vectorMovedSumX);
            }
            vectorMovedSumZ += vectorToMove.z;
            vectorMovedSumX += vectorToMove.x;
        }
    }
    else {
        isMovementLocked = false;
    }
    return vectorToMove;
}
    Vector3 MovePlayerFromDirection()
    {
        if (!isMovementLocked)
        {
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

            movementSpeed = grounded ? playerMovementSpeed : playerMovementSpeed * 0.9f;

            Vector3 movementVector = (transform.forward * playerVerticalInput + transform.right * playerHorizontalInput).normalized * movementSpeed * Time.deltaTime;
            playerMovementDirection = movementVector;
            return movementVector;
        }
        else
        {
            Debug.Log("locked");
            return Vector3.zero;
        }
    }
   Vector3 CalculateDashMovement()
{
    dashCooldownTimer = dashCooldown;

    // Define dash parameters
    float dashDuration = 0.3f;
    Vector3 dashDirection = GetDashDirection();

    if (dashDirection.magnitude > 0)
    {
        dashDirection = dashDirection.normalized;

        // Calculate total movement for the entire dash duration
        Vector3 totalMovement = 18.0f * dashDirection * dashDuration;

        // Reset the player's velocity after the dash
        playerMovementVelocity = totalMovement;

        return totalMovement;
    }

    // If no dash direction is provided, return zero vector
    return Vector3.zero;
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