using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class GroundMovement : MonoBehaviour, IPlayerMovement, IGroundActions
{
    #region Variables
    public enum InternalState
    {
        Idle, Moving, Jumping, Falling
    }
    private InternalState _internalState = InternalState.Idle;

    [Header("Movement")]
    [SerializeField] private float _groundDrag = 3f;
    [SerializeField] private float _groundAcceleration = 1.5f;
    private float _initialMoveSpeed;
    private float _maxMoveSpeed;

    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private float _jumpCooldown = 1f;
    [SerializeField] private float _airMultiplier = 1f;
    [SerializeField] private float _fallingAcceleration = 2.5f;
    private bool _canJump = true;

    [Header("Ground check")]
    [SerializeField] private float _playerHeight = 2f;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _jumpPressed = false;

    [Header("Debug Info")]
    [SerializeField] private MovementComponents _movementComponents; // Components
    #endregion

    #region Functions
    #region IPlayerMovement
    public void Initialize(MovementComponents components)
    {
        components.Input.Ground.Move.performed += OnMove;
        components.Input.Ground.Move.canceled += OnMove;
        components.Input.Ground.Run.performed += OnRun;
        components.Input.Ground.Run.canceled += OnRun;
        components.Input.Ground.Jump.performed += OnJump;

        components.Rigidbody.freezeRotation = true;

        var pc = components.PlayerController;
        _initialMoveSpeed = pc.InitialMoveSpeed;
        _maxMoveSpeed = pc.ThresholdSpeed;

        _movementComponents = components;
    }

    public void OnUpdate()
    {
        // Check if the player is grounded
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _movementComponents.PlayerController.GroundMask);
        
        Debug.Log("Ground check");
        //Set Grounded to animator
        var pc = _movementComponents.PlayerController;
        pc.animator.SetBool(pc.isGroundedID, _isGrounded);
        
        // Apply ground drag to the player rigidbody if it's grounded
        _movementComponents.Rigidbody.drag = _isGrounded ? _groundDrag : 0.0f;
    }

    public void OnFixedUpdate()
    {
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        if (IsIdling(pc)) return;
        HandleFalling(pc, rb);
        MovePlayer(pc);
        SpeedControl(rb);
    }

    public void ResetMovement()
    {
        var rb = _movementComponents.Rigidbody;
        rb.useGravity = false; //When changing to flight, dont update moveSpeed
        Debug.Log("Current rb vel previous to change: " + rb.velocity);
        _movementComponents.PlayerController.MoveSpeed = rb.velocity.magnitude;
    }
    #endregion

    public void SetSubState(InternalState newSubState)
    {
        if (_internalState.Equals(newSubState)) return;
        
        _internalState = newSubState;
        
        switch(_internalState)
        {
            case InternalState.Idle:
                _movementComponents.PlayerController.audioManager.StopPlayingAll();
                break;
            case InternalState.Moving:
                CheckAndPlayMoveAudio();
                break;
            case InternalState.Jumping:
                break;
            case InternalState.Falling:
                PlayFallingAudio();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleFalling(in PlayerController pc, in Rigidbody rb)
    {
        if (!_isGrounded && rb.velocity.y < 0f)
        {
            SetSubState(InternalState.Falling);
            
            float gravityAccel = 0.2f * Physics.gravity.magnitude;
            pc.MoveSpeed += _fallingAcceleration * gravityAccel * Time.fixedDeltaTime;
            pc.MoveSpeed = Mathf.Clamp(pc.MoveSpeed, 0f, _maxMoveSpeed);
        }
    }

    private bool IsIdling(in PlayerController pc)
    {
        if (pc.InputAxis.y == 0 && _isGrounded)
        {
            if (_internalState != InternalState.Idle) Debug.Log("Enter Idling");

            SetSubState(InternalState.Idle);
            pc.MoveSpeed = 0f;
            
            //Smoothly decrease speed to 0
            pc.animator.SetFloat(pc.moveSpeedID, 
                Mathf.SmoothDamp(pc.animator.GetFloat(pc.moveSpeedID), 
                    0f, ref pc.moveSpeedDamp, 0.1f));
            
            return true;
        }

        return false;
    }
    
    private void MovePlayer(in PlayerController pc)
    {
        Debug.Log("Move player");
        if (pc.InputAxis != Vector2.zero)
        {
            // We only consider player is moving state if it's grounded,
            // although we want to be able to move it at any moment
            if (_isGrounded)
                SetSubState(InternalState.Moving);

            float direction = _isRunning ? 1f : -1f;
            pc.MoveSpeed += direction * _groundAcceleration * Time.fixedDeltaTime;
            pc.MoveSpeed = Mathf.Clamp(pc.MoveSpeed, _initialMoveSpeed, _maxMoveSpeed);

            Debug.Log("MoveSpeed now is: " +  pc.MoveSpeed);
            //Set MoveSpeed to animator
            var currentSpeed = (pc.MoveSpeed - _initialMoveSpeed) / (_maxMoveSpeed - _initialMoveSpeed) + 1f; //Between 5 and 10 is 1-2
            Debug.Log("Current speed now is: " + currentSpeed);
            pc.animator.SetFloat(pc.moveSpeedID, 
                Mathf.SmoothDamp(pc.animator.GetFloat(pc.moveSpeedID), 
                currentSpeed, ref pc.moveSpeedDamp, 0.1f)); 
            
            //Calculate movement dir
            var orientation = _movementComponents.Orientation;
            Vector3 moveDirection = orientation.forward * pc.InputAxis.y + orientation.right * pc.InputAxis.x;

            float inAirMultiplier = _isGrounded ? 1.0f : _airMultiplier;
            _movementComponents.Rigidbody.AddForce(pc.MoveSpeed * 10f * inAirMultiplier * moveDirection, ForceMode.Force);
        }
    }

    private void SpeedControl(in Rigidbody rb)
    {
        Vector3 vel = rb.velocity;
        vel.y = 0.0f;

        if (vel.magnitude > 2.0f * _maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * _maxMoveSpeed;
            rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
        }
    }

    private void Jump()
    {
        //Trigger Jump on animator
        var pc = _movementComponents.PlayerController;
        pc.animator.SetTrigger(pc.jumpID);
        
        var rb = _movementComponents.Rigidbody;
        Vector3 vel = rb.velocity;
        vel.y = 0.0f;
        rb.velocity = vel;
        rb.AddForce(_jumpForce * transform.up, ForceMode.Impulse);

        SetSubState(InternalState.Jumping);
    }

    private void ResetJump()
    {
        _canJump = true;
        _jumpPressed = false;
        Debug.Log("Jump released");
    }

    public void CheckAndPlayMoveAudio()
    {
        if (!_isGrounded) return;
        if (_movementComponents.PlayerController.InputAxis != Vector2.zero)
        {
            _movementComponents.PlayerController.audioManager.PlayAudioLoop(_isRunning ? "StepsRun" : "StepsWalk");
            return;
        }
        //If equals zero, then it is stopped
        _movementComponents.PlayerController.audioManager.StopPlayingAll();
    }

    public void PlayFallingAudio()
    {
        _movementComponents.PlayerController.audioManager.PlayAudioLoop("WindBase");
    }
    
    #endregion

    #region InputSystemCallbacks

    public void OnMove(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (pc.isDead) return;
        if (!pc.MoveType.Equals(MovementType.Ground)) return;
        
        _movementComponents.PlayerController.InputAxis = context.ReadValue<Vector2>();

        if (!_isGrounded) return;
        CheckAndPlayMoveAudio();
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (pc.isDead) return;
        if (!pc.MoveType.Equals(MovementType.Ground)) return;
     
        var rb = _movementComponents.Rigidbody;
        
        void StartFlying()
        {
            Debug.Log("Switch state");
            
            //Set Grounded false to animator and jump again to fly
            var pc = _movementComponents.PlayerController;
            
            pc.SwitchState(MovementType.Air);
            
            _isGrounded = false;
            pc.animator.SetBool(pc.isGroundedID, _isGrounded);
            pc.animator.SetTrigger(pc.jumpID);
            
            rb.AddForce(_jumpForce * 40.0f * _movementComponents.Orientation.up, ForceMode.Force);
        }
        
        void PerformJump()
        {
            _jumpPressed = true;
            Debug.Log("Jump pressed");
            if (!_canJump || !_isGrounded) return;

            _canJump = false;
            Jump();
            Invoke(nameof(ResetJump), _jumpCooldown);
        }

        bool enoughSpeed = pc.MoveSpeed >= _maxMoveSpeed * 0.9f;
        switch (_internalState)
        {
            case InternalState.Falling:
                if (enoughSpeed)
                {
                    StartFlying();
                }
                break;
            case InternalState.Jumping:
                StartFlying();
                break;
            case InternalState.Moving:
            case InternalState.Idle:
                if (enoughSpeed)
                {
                    StartFlying();
                }
                else
                {
                    if (_jumpPressed)
                    {
                        _jumpPressed = false;
                        StartFlying();
                        return;
                    }

                    PerformJump();
                }
                break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (pc.isDead) return;
        if (!pc.MoveType.Equals(MovementType.Ground)) return;
        
        _isRunning = context.performed;
        CheckAndPlayMoveAudio();
    }

    #endregion
}