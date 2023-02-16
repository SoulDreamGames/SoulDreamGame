using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class PlayerMovement : MonoBehaviour, IGroundActions
{
    [Header("Movement")] public float moveSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCd;
    public float airMultiplier;
    private bool canJump = true;
    
    [Header("Ground check")] public float playerHeight;
    public LayerMask groundMask;
    private bool isGrounded;

    private Transform _orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    
    //Components
    private MoveInput _input;
    private Rigidbody _rb;

    public void Initialize(MoveInput input, Rigidbody rb, Transform orientation)
    {
        _input = input;
        _input.Ground.Move.performed += OnMove;
        _input.Ground.Move.canceled += OnMove;

        _input.Ground.Jump.performed += OnJump;

        _rb = rb;
        _rb.freezeRotation = true;

        _orientation = orientation;
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
        MovePlayer();
    }

    private void MovePlayer()
    {
        //Calculate movement dir
        moveDirection = _orientation.forward * verticalInput + _orientation.right * horizontalInput;

        if (isGrounded)
        {
            _rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force); //ToDo: add 10.0f as 
            return;
        }
        
        //In air
        _rb.AddForce(moveDirection * moveSpeed * 10.0f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 vel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (vel.magnitude > moveSpeed)
        {
            Vector3 newVel = vel.normalized * moveSpeed;
            _rb.velocity = new Vector3(newVel.x, _rb.velocity.y, newVel.z);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        horizontalInput = direction.x;
        verticalInput = direction.y;
        //Debug.Log("Move");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canJump || !isGrounded) return;
        
        canJump = false;
        Jump();
        Invoke(nameof(ResetJump), jumpCd);
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
    }
}