using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCd;
    public float airMultiplier;
    private bool canJump = true;

    [Header("Keybinds")] public KeyCode jumpKey = KeyCode.Space; //ToDo: change to new input system


    [Header("Ground check")] public float playerHeight;
    public LayerMask groundMask;
    private bool isGrounded;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        //IsGrounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);

        GetInput();
        SpeedControl();

        //Drag
        rb.drag = isGrounded ? groundDrag : 0.0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && canJump && isGrounded)
        {
            canJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCd);
        }
    }

    private void MovePlayer()
    {
        //Calculate movement dir
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force); //ToDo: add 10.0f as 
            return;
        }
        
        //In air
        rb.AddForce(moveDirection.normalized * moveSpeed * 10.0f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (vel.magnitude > moveSpeed)
        {
            Vector3 newVel = vel.normalized * moveSpeed;
            rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
        }
    }

    private void Jump()
    {
        var vel = rb.velocity;
        vel.y = 0.0f;
        rb.velocity = vel;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }
}