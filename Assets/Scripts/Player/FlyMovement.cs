using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlyMovement : MonoBehaviour, IPlayerMovement, IFlyActions
{
    #region Variables
    //Speeds
    [Header("Speeds")]
    [SerializeField] private float _forwardSpeed = 25f;  // Forward movement
    [SerializeField] private float _strafeSpeed = 7.5f;  // Sideways movement
    [SerializeField] private float _hoverSpeed = 10f;    // Up-down movement

    [SerializeField] private float _forwardAccel = 2.5f;   // Forward acceleration
    [SerializeField] private float _strafeAccel = 2f;      // Sideways acceleration
    [SerializeField] private float _hoverAccel = 2f;       // Up-down acceleration

    private float _activeForwardSpeed = 0f;
    private float _activeStrafeSpeed = 0f;
    private float _activeHoverSpeed = 0f;

    [SerializeField] private float _moveAccel = 2f;
    [SerializeField] private float _maxMoveSpeed = 100f;
    private float _initialMoveSpeed;
    private float _brakeFactor = 1.0f;

    // Smoothing properties
    private readonly float _tolerance = 1f;
    private Vector3 _smoothStopVel = Vector3.zero;
    private Vector3 _lastForward;

    [Header("Debug Info")]
    [SerializeField] private Vector3 _originalForward;
    [SerializeField] private MovementComponents _movementComponents; // Components
    #endregion


    private void OnDrawGizmos()
    {
        var tr = _movementComponents.PlayerController.PlayerObject.transform;
        var pos = tr.position;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + 4.0f * tr.forward);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + 4.0f * tr.up);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + 4.0f * tr.right);
    }

    #region Functions
    public void Initialize(MovementComponents components)
    {
        components.Input.Fly.Movement.performed += OnMovement;
        components.Input.Fly.Movement.canceled += OnMovement;
        components.Input.Fly.Attack.performed += OnAttack;

        _lastForward = components.Orientation.forward;
        _originalForward = _lastForward;

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
#if true
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;
        var orientation = _movementComponents.Orientation;

        _activeForwardSpeed = Mathf.Lerp(_activeForwardSpeed, pc.InputAxis.y * _forwardSpeed, _forwardAccel * Time.deltaTime);
        _activeStrafeSpeed = Mathf.Lerp(_activeStrafeSpeed, pc.InputAxis.x * _strafeSpeed, _strafeAccel * Time.deltaTime);

        // TODO: Rotate player object when receiving sideways input movement

        Vector3 velocity = _activeForwardSpeed * orientation.forward + _activeStrafeSpeed * orientation.right;
        rb.velocity = velocity;

#else
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        if (pc.InputAxis.y != 0) // Moving forward or backwards
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
                    pc.SwitchState(MovementType.Ground);
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
#endif
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