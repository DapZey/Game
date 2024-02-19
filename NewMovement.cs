using Cinemachine;
using UnityEngine;
using System;
public class Logs : MonoBehaviour
{

    private float _jumpHeight;
    CharacterController _playerInputController;

    private float _gravityOnPlayer;
    private float _jumpVelocity;
    CinemachineBrain _cinemachineBrain;
    private Animator _anim;
    private Vector3 _vectorToMove;
    private bool _isMovementLocked;
    private float _movementLerpSpeed = 2f;
    private float _decelerationLerpSpeed = 0.5f; // Added deceleration lerp speed
    Vector3 _playerVelocity;
    public float playerMovementSpeed = 5;
    private bool _grounded;
    private int _runHash;
    private Transform _transform;
    private float _movingDelta;
    private float _fallingDelta;
    private float _jumpingDelta;
    Vector3 _dashMovement;
    private int _framesLeftForDash;
    float _dashStartTime;
    int _totalFramesForDash = 300; 
    float _ms = 0.001f;
    float _vectorMovedSumZ;
    float _vectorMovedSumX;
    Vector3 _dashTrajectory;
    public float dashCooldown = 1;
    private float _dashCooldownTimer;
    Vector3 _playerMovementVelocity;
    
    void Start()
    {
        _framesLeftForDash  = 50000001;
        _transform = this.transform;
        _isMovementLocked = false;
        _jumpHeight = 2f;
        _anim = GetComponent<Animator>();
        _gravityOnPlayer = -30f;
        _playerInputController = GetComponent<CharacterController>();
        _jumpVelocity = Mathf.Sqrt(-2 * _gravityOnPlayer * _jumpHeight);
        _cinemachineBrain = GetComponent<CinemachineBrain>();
        _runHash = Animator.StringToHash("Run");
    }

    // Update is called once per frame
    void Update()
    {
        _jumpingDelta = Time.deltaTime;
        _movingDelta = Time.deltaTime;
        _fallingDelta = Time.deltaTime;
        if (_fallingDelta > 0.04f)
        {
            _fallingDelta = (_fallingDelta * 2f) -0.04f;
        }
        if (_movingDelta > 0.04f)
        {
            _movingDelta = 0.04f;
        }

        if (_playerInputController.transform.position.y < 6.28f)
        {
            Debug.Log(Time.time);
        }
        Vector3 playerMovement = Vector3.zero;
        _grounded = IsPlayerOnGround();
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isMovementLocked = true;
            _vectorMovedSumZ = 0;
            _vectorMovedSumX = 0;
            _framesLeftForDash = 0;
            _dashStartTime = Time.time;
            _dashTrajectory = CalculateDashMovement();
            _dashMovement = _dashTrajectory / _totalFramesForDash;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
           playerMovement += PlayerVerticalJumpMovement();
        }
        playerMovement +=CheckDash(playerMovement);
        playerMovement +=MovePlayerFromDirection();
        playerMovement += PlayerGroundGravityAffect();
        _playerInputController.Move(playerMovement);
    }
    Vector3 PlayerVerticalJumpMovement()
    {
        if (_grounded)
        {
            if (_jumpingDelta < 0.04f)
            {
                _playerMovementVelocity.y = _jumpVelocity;
            }
            else
            {
                _playerMovementVelocity.y = _jumpVelocity /100f;
            }
        }

        return _playerMovementVelocity * _jumpingDelta;
    }
    Vector3 PlayerGroundGravityAffect()
    {
        if (!_grounded)
        {
            _playerMovementVelocity.y += _gravityOnPlayer * _fallingDelta;
        }
        else if (_playerMovementVelocity.y < 0)
        {
            _playerMovementVelocity.y = -2f;
            
        }

        return _playerMovementVelocity * _fallingDelta;
    }
    bool IsPlayerOnGround()
    {
        return _playerInputController.isGrounded;
    }
    Vector3 MovePlayerFromDirection()
    {
        Vector3 toMove = Vector3.zero;
        float playerVerticalInput = 0;
        float playerHorizontalInput = 0;
        if (!_isMovementLocked)
        {
            
    
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
                _anim.SetBool(_runHash, true);
            }
            else
            {
                _anim.SetBool(_runHash, false);
            }
    
            // Calculate movement direction and speed
            float movementSpeed = IsPlayerOnGround() ? playerMovementSpeed : playerMovementSpeed * 0.9f;
            Vector3 playerMovementDirection = (_transform.forward * playerVerticalInput + _transform.right * playerHorizontalInput).normalized;
            Vector3 targetVelocity = playerMovementDirection * movementSpeed;
    
            // Smoothly lerp between current velocity and target velocity
            _playerVelocity = Vector3.Lerp(_playerVelocity, targetVelocity, _movementLerpSpeed * _movingDelta);
    
            // Apply movement
            toMove += _playerVelocity * _movingDelta;
        }
            if (_grounded){
                _movementLerpSpeed = 15f;
            } else {
                _movementLerpSpeed = 2f;
            }
            _playerVelocity = Vector3.Lerp(_playerVelocity, Vector3.zero, _decelerationLerpSpeed * _movingDelta);
            toMove += _playerVelocity * _movingDelta;
            return toMove;
    }
    Vector3 CheckDash(Vector3 vectorToMove){
        if (_framesLeftForDash < _totalFramesForDash)
        {
            float elapsedTime = Time.time - _dashStartTime;
            float inc =(elapsedTime / _ms)-_framesLeftForDash;
            _framesLeftForDash += Mathf.FloorToInt(inc);
            if (_framesLeftForDash > 0 && _vectorMovedSumZ + (_dashMovement * Mathf.FloorToInt(inc)).z <= MathF.Abs(_dashTrajectory.z) && _vectorMovedSumZ + (_dashMovement * Mathf.FloorToInt(inc)).z >= -MathF.Abs(_dashTrajectory.z)
                && _vectorMovedSumX + (_dashMovement * Mathf.FloorToInt(inc)).x <= MathF.Abs(_dashTrajectory.x) && _vectorMovedSumX + (_dashMovement * Mathf.FloorToInt(inc)).x >= -MathF.Abs(_dashTrajectory.x)){
            
                vectorToMove += _dashMovement * Mathf.FloorToInt(inc);
                _vectorMovedSumZ += vectorToMove.z;
                _vectorMovedSumX += vectorToMove.x;
            }
            else if (_framesLeftForDash > 0){
                if (_vectorMovedSumZ > 0){
                    vectorToMove.z += MathF.Abs(_dashTrajectory.z)-_vectorMovedSumZ;
                }
                else{
                    vectorToMove.z += -MathF.Abs(_dashTrajectory.z) + Math.Abs( _vectorMovedSumZ);
                }
                if (_vectorMovedSumX > 0){
                    vectorToMove.x += MathF.Abs(_dashTrajectory.x)-_vectorMovedSumX;
                }
                else{
                    vectorToMove.x += -MathF.Abs(_dashTrajectory.x) + Math.Abs( _vectorMovedSumX);
                }
                _vectorMovedSumZ += vectorToMove.z;
                _vectorMovedSumX += vectorToMove.x;
            }
        }
        else {
            _isMovementLocked = false;
        }
        return vectorToMove;
    }
    Vector3 CalculateDashMovement()
    {
        _dashCooldownTimer = dashCooldown;

        // Define dash parameters
        float dashDuration = 0.3f;
        Vector3 dashDirection = GetDashDirection();

        if (dashDirection.magnitude > 0)
        {
            dashDirection = dashDirection.normalized;

            // Calculate total movement for the entire dash duration
            Vector3 totalMovement = dashDirection * dashDuration;

            return totalMovement * 18f;
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
