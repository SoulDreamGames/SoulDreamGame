using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlyMovement : MonoBehaviour, IFlyActions
{
    //Speeds
    [Header("Speeds")]
    [SerializeField] private float _hoverSpeed = 10f;
    [SerializeField] private float _moveAccel = 2f;
    [SerializeField] private float _maxMoveSpeed = 100f;
    private float _initialMoveSpeed;
    private float _brakeFactor = 1.0f;

    //Smoothing properties
    private readonly float _tolerance = 1f;
    private Vector3 _smoothStopVel = Vector3.zero;
    private Vector3 _lastForward;
    public Vector3 OriginalForward;

    //Components
    private MoveInput _input;
    private Rigidbody _rb;
    private Transform _orientation;
    private PlayerController _playerController;

    public void Initialize(MoveInput input, Rigidbody rb, Transform orientation, PlayerController pc)
    {
        _input = input;
        _input.Fly.Movement.performed += OnMovement;
        _input.Fly.Movement.canceled += OnMovement;
        _input.Fly.Attack.performed += OnAttack;

        _rb = rb;

        _orientation = orientation;
        _lastForward = _orientation.forward;

        _playerController = pc;
        _initialMoveSpeed = pc.ThresholdSpeed;
        _maxMoveSpeed = pc.MaxMoveSpeed;
        OriginalForward = _lastForward;
    }

    public void OnUpdate()
    {
    }

    public void OnFixedUpdate()
    {
        if (_playerController.InputAxis.y != 0)
        {
            _playerController.MoveSpeed += _moveAccel * Time.fixedDeltaTime;
            _brakeFactor = 1.0f;
            //Debug.Log("Move speed: " + moveSpeed);
        }
        else
        {
            if (_brakeFactor != 0.0f)
            {
                _playerController.MoveSpeed -= 2f * _moveAccel * Time.fixedDeltaTime;
                _brakeFactor = _playerController.MoveSpeed > _tolerance ? -1.0f : 0.0f;
            }
        }

        //Debug.Log("Move speed prev: " + _playerController.moveSpeed);
        _playerController.MoveSpeed =
            Mathf.Clamp(_playerController.MoveSpeed, _playerController.ThresholdSpeed, _maxMoveSpeed);

        //Save current forward
        Vector3 forward = _brakeFactor <= 0.0f ? _lastForward : _orientation.forward;
        _lastForward = forward;

        Vector3 moveDir = _playerController.InputAxis.y * forward;
        Vector3 movement = _playerController.MoveSpeed * new Vector3(moveDir.x, 0.0f, moveDir.z)
                           + new Vector3(0.0f, moveDir.y, 0.0f) * _hoverSpeed;

        switch (_brakeFactor)
        {
            //Forward movement
            case 0.0f:

                if (_rb.velocity.sqrMagnitude < _tolerance)
                {
                    _rb.velocity = Vector3.zero;
                    _playerController.MoveSpeed = _initialMoveSpeed;
                    //Switch to ground movement when stopping
                    _playerController.SwitchState(Movement.Ground);
                    return;
                }
                
                _rb.velocity = Vector3.SmoothDamp(_rb.velocity, Vector3.zero, ref _smoothStopVel, 1.0f);
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
                    _brakeFactor = 0.0f;
                    return;
                }

                movement = _playerController.MoveSpeed * forward;
                _rb.velocity -= new Vector3(movement.x, movement.y, movement.z) * Time.fixedDeltaTime;
                
                _rb.velocity = new Vector3
                {
                    x = Mathf.Abs(_rb.velocity.x) < _playerController.InitialMoveSpeed ? 0f : _rb.velocity.x,
                    y = Mathf.Abs(_rb.velocity.y) < _playerController.InitialMoveSpeed ? 0f : _rb.velocity.y,
                    z = Mathf.Abs(_rb.velocity.z) < _playerController.InitialMoveSpeed ? 0f : _rb.velocity.z
                };

                break;
        }

        SpeedControl();

        //rb.MovePosition(rb.position + new Vector3(movement.x * Time.deltaTime, 0f, movement.z * Time.deltaTime));
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _playerController.InputAxis = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Attacking");
        Debug.Log(_playerController.InEnemyBounds);
        Debug.Log(_playerController.MoveSpeed >= _playerController.EnemySpeedThreshold);
        
        if (_playerController.InEnemyBounds 
            && _playerController.MoveSpeed >= _playerController.EnemySpeedThreshold)
        {
            Debug.Log("Attacking enemy");
            _playerController.EnemyCollided.GetComponent<EnemyBehavior>().OnRespawn();
            _playerController.EnemyCollided = null;
            _playerController.InEnemyBounds = false;
        }
    }

    private void SpeedControl()
    {
        Vector3 vel = _rb.velocity;

        if (vel.magnitude > _maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * _maxMoveSpeed;
            _rb.velocity = newVel;
        }
    }
}