using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlyMovement : MonoBehaviour, IFlyActions
{
    #region Variables
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
    private MovementComponents _movementComponents;
    private Rigidbody _rb;
    #endregion

    #region Functions
    public void Initialize(MovementComponents components)
    {
        components.Input.Fly.Movement.performed += OnMovement;
        components.Input.Fly.Movement.canceled += OnMovement;
        components.Input.Fly.Attack.performed += OnAttack;

        _lastForward = components.Orientation.forward;
        OriginalForward = _lastForward;

        var pc = components.PlayerController;
        _initialMoveSpeed = pc.ThresholdSpeed;
        _maxMoveSpeed = pc.MaxMoveSpeed;

        _movementComponents = components;
    }

    public void OnUpdate()
    {
        // Update does nothing at the moment
    }

    public void OnFixedUpdate()
    {
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        if (pc.InputAxis.y != 0)
        {
            pc.MoveSpeed += _moveAccel * Time.fixedDeltaTime;
            _brakeFactor = 1.0f;
        }
        else if (_brakeFactor != 0.0f)
        {
            pc.MoveSpeed -= 2f * _moveAccel * Time.fixedDeltaTime;
            _brakeFactor = pc.MoveSpeed > _tolerance ? -1.0f : 0.0f;
        }

        pc.MoveSpeed = Mathf.Clamp(pc.MoveSpeed, pc.ThresholdSpeed, _maxMoveSpeed);

        //Save current forward
        Vector3 forward = _brakeFactor <= 0.0f ? _lastForward : _movementComponents.Orientation.forward;
        _lastForward = forward;

        Vector3 moveDir = pc.InputAxis.y * forward;
        Vector3 movement = new()
        {
            x = pc.MoveSpeed * moveDir.x,
            y = _hoverSpeed * moveDir.y,
            z = pc.MoveSpeed * moveDir.z,
        };

        switch (_brakeFactor)
        {
            //Forward movement
            case 0.0f:
                if (rb.velocity.sqrMagnitude < _tolerance)
                {
                    rb.velocity = Vector3.zero;
                    pc.MoveSpeed = _initialMoveSpeed;
                    //Switch to ground movement when stopping
                    pc.SwitchState(Movement.Ground);
                    return;
                }

                rb.velocity = Vector3.SmoothDamp(rb.velocity, Vector3.zero, ref _smoothStopVel, 1.0f);
                return;

            //Moving forward
            case >= 1.0f:
                rb.velocity = movement;
                break;

            //Stopping
            case <= -1.0f:
                if (rb.velocity.sqrMagnitude <= _initialMoveSpeed)
                {
                    _brakeFactor = 0.0f;
                    return;
                }

                movement = pc.MoveSpeed * forward;
                rb.velocity -= movement * Time.fixedDeltaTime;

                rb.velocity = new Vector3
                {
                    x = Mathf.Abs(rb.velocity.x) < pc.InitialMoveSpeed ? 0f : rb.velocity.x,
                    y = Mathf.Abs(rb.velocity.y) < pc.InitialMoveSpeed ? 0f : rb.velocity.y,
                    z = Mathf.Abs(rb.velocity.z) < pc.InitialMoveSpeed ? 0f : rb.velocity.z
                };

                break;
        }

        SpeedControl();
    }

    private void SpeedControl()
    {
        Vector3 vel = _movementComponents.Rigidbody.velocity;

        if (vel.magnitude > _maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * _maxMoveSpeed;
            _movementComponents.Rigidbody.velocity = newVel;
        }
    }
    #endregion

    #region InputSystemCallbacks
    public void OnMovement(InputAction.CallbackContext context) =>
        _movementComponents.PlayerController.InputAxis = context.ReadValue<Vector2>();

    public void OnAttack(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        Debug.Log("Attacking");
        Debug.Log(pc.InEnemyBounds);
        Debug.Log(pc.MoveSpeed >= pc.EnemySpeedThreshold);

        if (pc.InEnemyBounds
            && pc.MoveSpeed >= pc.EnemySpeedThreshold)
        {
            Debug.Log("Attacking enemy");
            pc.EnemyCollided.GetComponent<EnemyBehavior>().OnRespawn();
            pc.EnemyCollided = null;
            pc.InEnemyBounds = false;
        }
    }
    #endregion
}