using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlightController : MonoBehaviour, IFlyActions
{
    //Speeds
    public float forwardSpeed = 25f;
    public float hoverSpeed = 5f;
    public float moveAccel = 0.1f;
    public float maxMoveSpeed = 40.0f;

    private float moveSpeed = 5f;
    private float initialMoveSpeed = 5f;

    private float activeForwardSpeed;
    private float activeStrafeSpeed;
    private float activeMoveSpeed;

    private float brakeFactor = 1.0f;

    private float _hoverAxis;
    private Vector2 _movement;

    //Smooth properties
    private float smoothVel = 0.0f;
    private Vector3 smoothStopVel = Vector3.zero;
    
    private float tolerance = 1f;
    private Vector3 _lastForward;

    //Components
    private MoveInput _input;
    private Transform _orientation;
    private Rigidbody _rb;

    public void Initialize(MoveInput input, Rigidbody rb, Transform orientation)
    {
        _input = input;

        _input.Fly.Movement.performed += OnMovement;
        _input.Fly.Movement.canceled += OnMovement;

        _input.Fly.Hover.performed += OnHover;
        _input.Fly.Hover.canceled += OnHover;
        
        moveSpeed = initialMoveSpeed;

        _rb = rb;

        _orientation = orientation;
        _lastForward = _orientation.forward;
    }

    public void OnUpdate()
    {
        //Unused
    }

    public void OnFixedUpdate()
    {
        
        if (_movement.y != 0)
        {
            moveSpeed += moveAccel * Time.deltaTime;
            moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
            brakeFactor = 1.0f;
            //Debug.Log("Move speed: " + moveSpeed);
        }
        else
        {
            if (brakeFactor != 0.0f)
            {
                moveSpeed -= 2f * moveAccel * Time.deltaTime;

                if (moveSpeed > tolerance)
                {
                    //Debug.Log("Decreasing speed: " + moveSpeed);
                    brakeFactor = -1.0f;
                }
                else brakeFactor = 0.0f;
            }
        }
        
        //Axis movements
        if (brakeFactor != -1.0f) activeForwardSpeed = _movement.y * forwardSpeed;
        activeMoveSpeed = _hoverAxis * hoverSpeed; //Forward and back

        //Save current forward
        Vector3 forward = brakeFactor <= 0.0f ? _lastForward : _orientation.forward;
        _lastForward = forward;

        Vector3 moveDir = forward * activeForwardSpeed * Time.fixedDeltaTime;

        Vector3 movement = moveDir * moveSpeed * 10.0f;

        //Hover movement
        //rb.MovePosition(rb.position + new Vector3(0f, activeMoveSpeed * Time.deltaTime, 0f));
        _rb.velocity = new Vector3(_rb.velocity.x, activeMoveSpeed, _rb.velocity.z);

        switch (brakeFactor)
        {
            //Forward movement
            case 0.0f:
                
                if (_rb.velocity.sqrMagnitude < tolerance)
                {
                    _rb.velocity = Vector3.zero;
                    moveSpeed = initialMoveSpeed;
                    return;
                }
                
                Debug.Log("Stopped smooth");
                _rb.velocity = Vector3.SmoothDamp(_rb.velocity, Vector3.zero, ref smoothStopVel, 1.0f);
                return;
            //Moving forward
            case >= 1.0f:
                _rb.velocity = new Vector3(movement.x, _rb.velocity.y, movement.z);
                break;
            //Stopping
            case <= -1.0f:
                
                if (_rb.velocity.sqrMagnitude <= initialMoveSpeed)
                {
                    Debug.Log("Stopped");
                    brakeFactor = 0.0f;
                    return;
                }
                
                _rb.velocity -=  new Vector3(movement.x, 0f, movement.z) * Time.fixedDeltaTime;

                _rb.velocity = new Vector3
                {
                    x = Mathf.Abs(_rb.velocity.x) < initialMoveSpeed ? 0f : _rb.velocity.x,
                    y = Mathf.Abs(_rb.velocity.y) < initialMoveSpeed ? 0f : _rb.velocity.y,
                    z = Mathf.Abs(_rb.velocity.z) < initialMoveSpeed ? 0f : _rb.velocity.z
                };
                Debug.Log("Current velocity is: " + _rb.velocity);
                break;
        }

        //rb.MovePosition(rb.position + new Vector3(movement.x * Time.deltaTime, 0f, movement.z * Time.deltaTime));
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void OnHover(InputAction.CallbackContext context)
    {
        _hoverAxis = context.ReadValue<float>();
    }
}