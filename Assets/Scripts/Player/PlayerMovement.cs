using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class PlayerMovement : MonoBehaviour, IGroundActions
{
    [Header("Movement")]
    public float groundDrag;

    private float initialMoveSpeed;
    private float maxMoveSpeed;
    public float groundAcceleration = 0.1f;

    public float jumpForce;
    public float jumpCd;
    public float airMultiplier;
    private bool canJump = true;
    
    [Header("Ground check")] public float playerHeight;
    public LayerMask groundMask;
    private bool isGrounded;

    private Transform _orientation;

    private Vector3 moveDirection;

    private bool _isRunning;
    private bool _jumpPressed = false;
    
    //Components
    private MoveInput _input;
    private Rigidbody _rb;
    private PlayerController _playerController;

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
        initialMoveSpeed = pc.initialMoveSpeed;
        maxMoveSpeed = pc.thresholdSpeed;
    }

    public void OnUpdate()
    {
        //IsGrounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);

        SpeedControl();

        //Drag
        _rb.drag = isGrounded ? groundDrag : 0.0f;
    }

    public void OnFixedUpdate()
    {
        if (_playerController.inputAxis.y == 0)
        {
            _playerController.moveSpeed = 0f;
            return;
        }
        
        if (_isRunning)
        {
            _playerController.moveSpeed += groundAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            _playerController.moveSpeed -= groundAcceleration * Time.fixedDeltaTime;
        }

        _playerController.moveSpeed = Mathf.Clamp(_playerController.moveSpeed, initialMoveSpeed, maxMoveSpeed);

        //Debug.Log("Move speed is: " + _playerController.moveSpeed);

        MovePlayer();
    }

    private void MovePlayer()
    {
        //Calculate movement dir
        moveDirection = _orientation.forward * _playerController.inputAxis.y + _orientation.right * _playerController.inputAxis.x;

        if (isGrounded)
        {
            _rb.AddForce(_playerController.moveSpeed * 10f * moveDirection, ForceMode.Force);
            return;
        }
        
        //In air
        _rb.AddForce(_playerController.moveSpeed * 10.0f * airMultiplier * moveDirection, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 vel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (vel.magnitude > 2.0f * maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * maxMoveSpeed;
            _rb.velocity = new Vector3(newVel.x, _rb.velocity.y, newVel.z);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _playerController.inputAxis = context.ReadValue<Vector2>();
        //Debug.Log("Move");
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (_jumpPressed)
        {
            Debug.Log("Switch state");
            _jumpPressed = false;

            if (_playerController.moveSpeed < maxMoveSpeed * 0.9f) return;
            
            //Debug.Log("Speed is: " + _playerController.moveSpeed + " and threshold is " + maxMoveSpeed * 0.8f);
            _playerController.SwitchState(Movement.Air);

            return;
        }
        _jumpPressed = true;
        Debug.Log("Jump pressed");
        if (!canJump || !isGrounded) return;

        canJump = false;
        Jump();
        Invoke(nameof(ResetJump), jumpCd);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        _isRunning = context.performed;
    }

    private void Jump()
    {
        var vel = _rb.velocity;
        vel.y = 0.0f;
        _rb.velocity = vel;

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
        _jumpPressed = false;
        Debug.Log("Jump released");
    }
}