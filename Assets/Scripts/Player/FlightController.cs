using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlightController : MonoBehaviour, IFlyActions
{
    //Speeds
    public float hoverSpeed = 5f;
    public float moveAccel = 0.1f;
    public float maxMoveSpeed = 40.0f;
    private float _initialMoveSpeed;

    private float activeForwardSpeed;
    private float activeStrafeSpeed;
    private float activeMoveSpeed;

    private float brakeFactor = 1.0f;

    private float _hoverAxis;

    //Smooth properties
    private Vector3 smoothStopVel = Vector3.zero;

    private float tolerance = 1f;
    private Vector3 _lastForward;

    //Components
    private MoveInput _input;
    private Transform _orientation;
    private Rigidbody _rb;
    private PlayerController _playerController;

    public void Initialize(MoveInput input, Rigidbody rb, Transform orientation, PlayerController pc)
    {
        _input = input;

        _input.Fly.Movement.performed += OnMovement;
        _input.Fly.Movement.canceled += OnMovement;

        // _input.Fly.Hover.performed += OnHover;
        // _input.Fly.Hover.canceled += OnHover;

        //moveSpeed = initialMoveSpeed;

        _rb = rb;

        _orientation = orientation;
        _lastForward = _orientation.forward;

        _playerController = pc;
        _initialMoveSpeed = pc.thresholdSpeed;
    }

    public void OnUpdate()
    {
    }

    public void OnFixedUpdate()
    {
        Debug.Log("Axis input: " + _playerController.inputAxis);
        if (_playerController.inputAxis.y != 0)
        {
            _playerController.moveSpeed += moveAccel * Time.fixedDeltaTime;
            brakeFactor = 1.0f;
            //Debug.Log("Move speed: " + moveSpeed);
        }
        else
        {
            if (brakeFactor != 0.0f)
            {
                _playerController.moveSpeed -= 2f * moveAccel * Time.fixedDeltaTime;

                if (_playerController.moveSpeed > tolerance)
                {
                    //Debug.Log("Decreasing speed: " + moveSpeed);
                    brakeFactor = -1.0f;
                }
                else brakeFactor = 0.0f;
            }
        }

        _playerController.moveSpeed =
            Mathf.Clamp(_playerController.moveSpeed, _playerController.thresholdSpeed, maxMoveSpeed);

        //Axis movements
        //if (brakeFactor != -1.0f) activeForwardSpeed = axisInput.y * Time.fixedDeltaTime;
        activeMoveSpeed = _hoverAxis * hoverSpeed; //Forward and back

        //Save current forward
        Vector3 forward = brakeFactor <= 0.0f ? _lastForward : _orientation.forward;
        _lastForward = forward;

        Vector3 moveDir = _playerController.inputAxis.y * forward;

        Vector3 movement = _playerController.moveSpeed * moveDir;
        Debug.Log("Applied speed is: " + _playerController.moveSpeed);
        Debug.Log("Current rb vel is: " + _rb.velocity);

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
                    _playerController.moveSpeed = _initialMoveSpeed;
                    //Switch to ground movement when stopping
                    _playerController.SwitchState(Movement.Ground);
                    return;
                }

                Debug.Log("Stopped smooth");
                _rb.velocity = Vector3.SmoothDamp(_rb.velocity, Vector3.zero, ref smoothStopVel, 1.0f);
                return;
            //Moving forward
            case >= 1.0f:
                _rb.velocity = new Vector3(movement.x, _rb.velocity.y, movement.z);
                //_rb.AddForce(movement, ForceMode.Force);
                break;
            //Stopping
            case <= -1.0f:

                if (_rb.velocity.sqrMagnitude <= _initialMoveSpeed)
                {
                    Debug.Log("Stopped");
                    brakeFactor = 0.0f;
                    return;
                }

                movement = _playerController.moveSpeed * forward;
                _rb.velocity -= new Vector3(movement.x, 0f, movement.z) * Time.fixedDeltaTime;
                Debug.Log("RB velocity: " + _rb.velocity);
                _rb.velocity = new Vector3
                {
                    x = Mathf.Abs(_rb.velocity.x) < _playerController.initialMoveSpeed ? 0f : _rb.velocity.x,
                    y = Mathf.Abs(_rb.velocity.y) < _playerController.initialMoveSpeed ? 0f : _rb.velocity.y,
                    z = Mathf.Abs(_rb.velocity.z) < _playerController.initialMoveSpeed ? 0f : _rb.velocity.z
                };

                break;
        }

        SpeedControl();

        //rb.MovePosition(rb.position + new Vector3(movement.x * Time.deltaTime, 0f, movement.z * Time.deltaTime));
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _playerController.inputAxis = context.ReadValue<Vector2>();
    }

    public void OnHover(InputAction.CallbackContext context)
    {
        //ToDo: quit this and use the forward for orientation, but restricting X and Y
        _hoverAxis = context.ReadValue<float>();
        Debug.Log("Hover is: " + _hoverAxis);
    }

    private void SpeedControl()
    {
        Vector3 vel = _rb.velocity;

        if (vel.magnitude > maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * maxMoveSpeed;
            _rb.velocity = newVel;
        }
    }
}