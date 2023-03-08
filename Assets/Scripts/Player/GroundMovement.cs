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
    private MoveInput _input;
    private Rigidbody _rb;
    private Transform _orientation;
    private PlayerController _playerController;
    #endregion

    #region Functions
    public void Initialize(MoveInput input, Rigidbody rb, Transform orientation, PlayerController pc)
    {
        _input = input;
        _input.Ground.Move.performed += OnMove;
        _input.Ground.Move.canceled += OnMove;
        _input.Ground.Run.performed += OnRun;
        _input.Ground.Run.canceled += OnRun;
        _input.Ground.Jump.performed += OnJump;

        _rb = rb;
        _rb.freezeRotation = true;

        _orientation = orientation;

        _playerController = pc;
        _initialMoveSpeed = pc.InitialMoveSpeed;
        _maxMoveSpeed = pc.ThresholdSpeed;
    }

    
    public void OnUpdate()
    {
        //IsGrounded
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _playerController.GroundMask);

        SpeedControl();

        //Drag
        _rb.drag = _isGrounded ? _groundDrag : 0.0f;
    }

    public void OnFixedUpdate()
    {
        if (_playerController.InputAxis.y == 0)
        {
            _playerController.MoveSpeed = 0f;
            return;
        }
        
        if (_isRunning)
        {
            _playerController.MoveSpeed += _groundAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            _playerController.MoveSpeed -= _groundAcceleration * Time.fixedDeltaTime;
        }

        _playerController.MoveSpeed = Mathf.Clamp(_playerController.MoveSpeed, _initialMoveSpeed, _maxMoveSpeed);

        //Debug.Log("Move speed is: " + _playerController.moveSpeed);

        MovePlayer();
    }

    private void MovePlayer()
    {
        //Calculate movement dir
        _moveDirection = _orientation.forward * _playerController.InputAxis.y + _orientation.right * _playerController.InputAxis.x;

        if (_isGrounded)
        {
            _rb.AddForce(_playerController.MoveSpeed * 10f * _moveDirection, ForceMode.Force);
            return;
        }
        
        //In air
        _rb.AddForce(_playerController.MoveSpeed * 10.0f * _airMultiplier * _moveDirection, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 vel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (vel.magnitude > 2.0f * _maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * _maxMoveSpeed;
            _rb.velocity = new Vector3(newVel.x, _rb.velocity.y, newVel.z);
        }
    }

    private void Jump()
    {
        var vel = _rb.velocity;
        vel.y = 0.0f;
        _rb.velocity = vel;

        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        _canJump = true;
        _jumpPressed = false;
        Debug.Log("Jump released");
    }

    #region InputSystemCallbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        _playerController.InputAxis = context.ReadValue<Vector2>();
        //Debug.Log("Move");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_jumpPressed)
        {
            Debug.Log("Switch state");
            _jumpPressed = false;

            if (_playerController.MoveSpeed < _maxMoveSpeed * 0.9f) return;

            //Debug.Log("Speed is: " + _playerController.moveSpeed + " and threshold is " + maxMoveSpeed * 0.8f);
            _playerController.SwitchState(Movement.Air);

            return;
        }
        _jumpPressed = true;
        Debug.Log("Jump pressed");
        if (!_canJump || !_isGrounded) return;

        _canJump = false;
        Jump();
        Invoke(nameof(ResetJump), _jumpCooldown);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        _isRunning = context.performed;
    }
    #endregion

    #endregion
}