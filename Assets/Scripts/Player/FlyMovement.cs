using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlyMovement : MonoBehaviour, IPlayerMovement, IFlyActions
{
    #region Variables
    //Speeds
    [Header("Speeds")]
    [SerializeField, Tooltip("Forward movement speed " +
        "(Max movement speed when the boost is NOT applied)")] private float _forwardSpeed = 65f;   // Forward movement
    [SerializeField, Tooltip("Sideways movement speed")] private float _strafeSpeed = 7.5f;  // Sideways movement
    [SerializeField, Tooltip("Up-down movement speed")] private float _hoverSpeed = 10f;     // Up-down movement

    private float _initialMoveSpeed;
    [SerializeField, Tooltip("Max movement speed when the boost IS applied")] private float _maxMoveSpeed = 100f;

    private float _activeForwardSpeed = 0f;
    private float _activeStrafeSpeed = 0f;
    private float _activeHoverSpeed = 0f;

    [Header("Accelerations")]
    [SerializeField, Tooltip("Forward movement acceleration")] private float _forwardAccel = 2.5f;  // Forward acceleration
    [SerializeField, Tooltip("Sideways movement acceleration")] private float _strafeAccel = 2f;    // Sideways acceleration
    [SerializeField, Tooltip("Up-down movement acceleration")] private float _hoverAccel = 2f;      // Up-down acceleration
    [SerializeField, Range(0f, 1f), Tooltip("Acceleration damping")] private float _accelDamping = 0.6f;

    private float _brakeFactor = 1.0f;

    // Smoothing properties
    [Header("Smoothing properties")]
    [SerializeField, Tooltip("Speed loss rate when no forward/back input is detected")] private float _speedLossRate = 4f;
    [SerializeField, Range(0f, 1f),
        Tooltip("How much the player has to stop after changing to Ground movement. " +
        "This should not be 1 never")] private float _tolerance = 0.3f;

    [Header("Debug Info")]
    [SerializeField] private float _usedTolerance = 1f;
    [SerializeField] private Vector3 _lastForward;
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
        _activeForwardSpeed = _initialMoveSpeed;

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

        if (CheckGroundReturning()) return;
        ApplyVelocity(pc.InputAxis);
        SpeedControl();

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

    public void ResetMovement()
    {
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        rb.useGravity = true;
        rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
        pc.MoveSpeed = pc.InitialMoveSpeed;

        _activeForwardSpeed = _initialMoveSpeed;
        _activeStrafeSpeed = 0.0f;
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

    private bool CheckGroundReturning()
    {
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        if (pc.InputAxis.y == 0)
        {
            // Slowly decrement velocity if no forward/back input is detected
            pc.MoveSpeed -= _speedLossRate * Time.fixedDeltaTime;
            rb.velocity = rb.velocity.normalized * pc.MoveSpeed;
            if (pc.MoveSpeed < pc.ThresholdSpeed - _tolerance)
            {
                //Switch to ground movement when stopping
                pc.SwitchState(MovementType.Ground);
                return true;
            }
        }

        return false;
    }

    private void ApplyVelocity(in Vector2 input)
    {
        var pc = _movementComponents.PlayerController;
        var orientation = _movementComponents.Orientation;

        float realDamping = 1.0f - _accelDamping;
        _activeForwardSpeed = Mathf.Lerp(_activeForwardSpeed, input.y * _forwardSpeed, _forwardAccel * realDamping * Time.fixedDeltaTime);
        _activeStrafeSpeed = Mathf.Lerp(_activeStrafeSpeed, input.x * _strafeSpeed, _strafeAccel * realDamping * Time.fixedDeltaTime);

        // TODO: Rotate player object when receiving sideways input movement

        Vector3 velocity = _activeForwardSpeed * orientation.forward + _activeStrafeSpeed * orientation.right;
        _movementComponents.Rigidbody.velocity = velocity;

        // Minimum speed tolerance before changing to Ground movement
        if (input.y != 0)
            _usedTolerance = 1f;
        else
            _usedTolerance = Mathf.Lerp(_usedTolerance, 0.25f, Time.fixedDeltaTime);
        pc.MoveSpeed = Mathf.Clamp(velocity.magnitude, pc.ThresholdSpeed - _usedTolerance, _maxMoveSpeed);
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

        if (pc.InEnemyBounds && pc.MoveSpeed >= pc.EnemySpeedThreshold)
        {
            Debug.Log("Attacking enemy");

            var enemy = pc.EnemyCollided.GetComponent<EnemyBehaviour>();
            pc.EnemyCollided = null;
            pc.InEnemyBounds = false;

            if (enemy == null) return;

            bool isDead = enemy.ReceiveDamage(3);
            if (isDead)
                enemy.OnDeath();
        }
    }
#endregion
}