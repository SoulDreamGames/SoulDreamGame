using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class GroundMovement : MonoBehaviour, IGroundActions
{
    #region Variables
    [Header("Movement")]
    [SerializeField] private float _groundDrag = 3f;
    [SerializeField] private float _groundAcceleration = 1.5f;
    private float _initialMoveSpeed;
    private float _maxMoveSpeed;

    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private float _jumpCooldown = 1f;
    [SerializeField] private float _airMultiplier = 1f;
    private bool _canJump = true;
    private Vector3 _moveDirection;
    
    [Header("Ground check")]
    [SerializeField] private float _playerHeight = 2f;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _jumpPressed = false;
    
    //Components
    private MovementComponents _movementComponents;
    private PlayerController _playerController;
    #endregion

    #region Functions
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
        _playerController = _movementComponents.PlayerController;
    }
        
    public void OnUpdate()
    {
        //IsGrounded
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _movementComponents.PlayerController.GroundMask);

        SpeedControl();

        //Drag
        _movementComponents.Rigidbody.drag = _isGrounded ? _groundDrag : 0.0f;
    }

    public void OnFixedUpdate()
    {
        var pc = _movementComponents.PlayerController;
        if (pc.InputAxis.y == 0)
        {
            pc.MoveSpeed = 0f;
            return;
        }

        float direction = _isRunning ? 1f : -1f;
        pc.MoveSpeed += direction * _groundAcceleration * Time.fixedDeltaTime;
        pc.MoveSpeed = Mathf.Clamp(pc.MoveSpeed, _initialMoveSpeed, _maxMoveSpeed);
        MovePlayer();
    }

    private void MovePlayer()
    {
        //Calculate movement dir
        var orientation = _movementComponents.Orientation;
        var pc = _movementComponents.PlayerController;
        _moveDirection = orientation.forward * pc.InputAxis.y + orientation.right * pc.InputAxis.x;

        float inAirMultiplier = _isGrounded ? 1.0f : _airMultiplier;
        _movementComponents.Rigidbody.AddForce(pc.MoveSpeed * 10f * inAirMultiplier * _moveDirection, ForceMode.Force);
    }

    private void SpeedControl()
    {
        var rb = _movementComponents.Rigidbody;
        Vector3 vel = _movementComponents.Rigidbody.velocity;
        vel.y = 0.0f;

        if (vel.magnitude > 2.0f * _maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * _maxMoveSpeed;
            rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
        }
    }

    private void Jump()
    {
        var rb = _movementComponents.Rigidbody;
        Vector3 vel = rb.velocity;
        vel.y = 0.0f;
        rb.velocity = vel;
        rb.AddForce(_jumpForce * transform.up, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        _canJump = true;
        _jumpPressed = false;
        Debug.Log("Jump released");
    }
    #endregion

    #region InputSystemCallbacks
    public void OnMove(InputAction.CallbackContext context) =>
        _movementComponents.PlayerController.InputAxis = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_jumpPressed)
        {
            _jumpPressed = false;

            var pc = _movementComponents.PlayerController;
            if (pc.MoveSpeed < _maxMoveSpeed * 0.9f) return;

            Debug.Log("Switch state");
            pc.SwitchState(Movement.Air);

            return;
        }

        _jumpPressed = true;
        Debug.Log("Jump pressed");
        if (!_canJump || !_isGrounded) return;

        _canJump = false;
        Jump();
        Invoke(nameof(ResetJump), _jumpCooldown);
    }

    public void OnRun(InputAction.CallbackContext context) => _isRunning = context.performed;
    #endregion
}