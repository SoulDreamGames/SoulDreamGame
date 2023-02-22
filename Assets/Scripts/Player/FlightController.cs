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
    public float maxMoveSpeed;
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
    public Vector3 originalForward;

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

        _input.Fly.Attack.performed += OnAttack;

        //moveSpeed = initialMoveSpeed;

        _rb = rb;

        _orientation = orientation;
        _lastForward = _orientation.forward;
        originalForward = _lastForward;

        _playerController = pc;
        _initialMoveSpeed = pc.thresholdSpeed;
        maxMoveSpeed = pc.MaxMoveSpeed;
    }

    public void OnUpdate()
    {
    }

    public void OnFixedUpdate()
    {
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
                else
                {
                    brakeFactor = 0.0f;
                }
            }
        }

        //Debug.Log("Move speed prev: " + _playerController.moveSpeed);
        _playerController.moveSpeed =
            Mathf.Clamp(_playerController.moveSpeed, _playerController.thresholdSpeed, maxMoveSpeed);

        //Save current forward
        Vector3 forward = brakeFactor <= 0.0f ? _lastForward : _orientation.forward;
        _lastForward = forward;


        Vector3 moveDir = _playerController.inputAxis.y * forward;

        Vector3 movement = _playerController.moveSpeed * new Vector3(moveDir.x, 0.0f, moveDir.z)
                           + new Vector3(0.0f, moveDir.y, 0.0f) * hoverSpeed;

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
                
                _rb.velocity = Vector3.SmoothDamp(_rb.velocity, Vector3.zero, ref smoothStopVel, 1.0f);
                return;
            //Moving forward
            case >= 1.0f:
                _rb.velocity = new Vector3(movement.x, movement.y, movement.z);
                //_rb.AddForce(movement, ForceMode.Force);
                break;
            //Stopping
            case <= -1.0f:

                if (_rb.velocity.sqrMagnitude <= _initialMoveSpeed)
                {
                    brakeFactor = 0.0f;
                    return;
                }

                movement = _playerController.moveSpeed * forward;
                _rb.velocity -= new Vector3(movement.x, movement.y, movement.z) * Time.fixedDeltaTime;
                
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

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Attacking");
        Debug.Log(_playerController.inEnemyBounds);
        Debug.Log(_playerController.moveSpeed >= _playerController.enemySpeedThreshold);
        
        if (_playerController.inEnemyBounds 
            && _playerController.moveSpeed >= _playerController.enemySpeedThreshold)
        {
            Debug.Log("Attacking enemy");
            _playerController.enemyCollided.GetComponent<EnemyBehavior>().OnRespawn();
            _playerController.enemyCollided = null;
            _playerController.inEnemyBounds = false;
        }
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