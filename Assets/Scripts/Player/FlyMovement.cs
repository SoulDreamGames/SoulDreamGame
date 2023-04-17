using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class FlyMovement : MonoBehaviour, IPlayerMovement, IFlyActions
{
    #region Variables
    public enum InternalState
    {
        Levitate, Moving, LightningBreak
    }
    private InternalState _internalState = InternalState.Levitate;

    [Header("Speeds")]
    [SerializeField, Tooltip("Forward movement speed " +
        "(Max movement speed when the boost is NOT applied)")]
    private float _forwardSpeed = 65f;   // Forward movement
    [SerializeField, Tooltip("Sideways movement speed")] private float _strafeSpeed = 7.5f;  // Sideways movement
    [SerializeField, Tooltip("Up-down movement speed")] private float _hoverSpeed = 10f;     // Up-down movement
    private float _activeForwardSpeed = 0f;
    private float _activeStrafeSpeed = 0f;
    private float _activeHoverSpeed = 0f;
    private float _initialMoveSpeed;

    [Header("Boost options")]
    [SerializeField, Tooltip("Max movement speed when the boost IS applied")] private float _maxMoveSpeed = 100f;
    [SerializeField, Tooltip("Boosting time")] private float _boostActiveTime = 3f;
    private float _timeSinceLastBoost = 0f;

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
        "This should not be 1 never")]
    private float _tolerance = 0.3f;

    [Header("Levitating")]
    [SerializeField,
        Tooltip("How much time the player is allowed to be levitating " +
        "before it returns to Ground movement.")]
    private float _levitatingTime = 5f;
    private float _timeSinceStartedLevitating = 0f;

    [Header("Debug Info")]
    [SerializeField] private float _usedTolerance = 1f;
    [SerializeField] private Vector3 _lastForward;
    [SerializeField] private Vector3 _originalForward;
    [SerializeField] private MovementComponents _movementComponents; // Components
    #endregion


    // private void OnDrawGizmos()
    // {
    //     if (!UnityEditor.EditorApplication.isPlaying) return;
    //     
    //     var tr = _movementComponents.PlayerController.PlayerObject.transform;
    //     var pos = tr.position;
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawLine(pos, pos + 4.0f * tr.forward);
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawLine(pos, pos + 4.0f * tr.up);
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(pos, pos + 4.0f * tr.right);
    // }

    #region Functions
    #region IPlayerMovement
    public void Initialize(MovementComponents components)
    {
        components.Input.Fly.Movement.performed += OnMovement;
        components.Input.Fly.Movement.canceled += OnMovement;
        components.Input.Fly.Attack.performed += OnAttack;
        components.Input.Fly.Attack.canceled += OnAttack;
        components.Input.Fly.HomingAttack.performed += OnHomingAttack;
        components.Input.Fly.Boost.performed += OnBoost;
        components.Input.Fly.LightningBreak.performed += OnLightningBreak;

        _lastForward = components.Orientation.forward;
        _originalForward = _lastForward;

        var pc = components.PlayerController;
        _initialMoveSpeed = pc.InitialMoveSpeed;
        _maxMoveSpeed = pc.MaxMoveSpeed;
        _activeForwardSpeed = _initialMoveSpeed;

        _movementComponents = components;
    }

    public void OnUpdate()
    {
        HandleBoost();
    }

    public void OnFixedUpdate()
    {
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        SelectInternalState(pc, rb);
        if (CheckGroundReturning(pc)) return;
        ApplyVelocity(pc.InputAxis);
        SpeedControl(rb);
        HandleEnergy();
    }

    public void ResetMovement()
    {
        var rb = _movementComponents.Rigidbody;
        var pc = _movementComponents.PlayerController;

        rb.useGravity = true;
        rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
        pc.MoveSpeed = pc.InitialMoveSpeed;
        pc.IsBoosting = false;

        _activeForwardSpeed = _initialMoveSpeed;
        _activeStrafeSpeed = 0.0f;
    }
    #endregion

    public void SetSubState(InternalState newSubState)
    {
        _internalState = newSubState;
    }

    private void HandleEnergy()
    {
        var pc = _movementComponents.PlayerController;

        //When attacking, lost energy per update
        if (pc.PlayerEnergy < 0.0f)
        {
            //If energy is 0, stop attack
            pc.PlayerEnergy = 0.0f;
            pc.IsAttacking = false;
            return;
        }

        if (pc.IsAttacking)
        {
            Debug.Log("Currently attacking");
            pc.PlayerEnergy -= pc.playerEnergyLost;
            return;
        }

        //If not attacking and its moving, then increase energy based on velocity
        if (pc.MoveSpeed > 0.0f)
        {
            pc.PlayerEnergy += pc.MoveSpeed / pc.MaxMoveSpeed;
            if (pc.PlayerEnergy >= pc.MaxEnergy)
            {
                pc.PlayerEnergy = pc.MaxEnergy;
            }
        }
    }

    private void HandleBoost()
    {
        var pc = _movementComponents.PlayerController;
        if (!pc.IsBoosting) return;

        _timeSinceLastBoost += Time.deltaTime;
        if (_timeSinceLastBoost >= _boostActiveTime)
        {
            pc.IsBoosting = false;
            CheckAndPlayMoveAudio();
        }
    }

    private void SpeedControl(in Rigidbody rb)
    {
        Vector3 vel = rb.velocity;

        if (vel.magnitude > _maxMoveSpeed)
        {
            Vector3 newVel = vel.normalized * _maxMoveSpeed;
            rb.velocity = newVel;
        }
    }

    private void SelectInternalState(in PlayerController pc, in Rigidbody rb)
    {
        if (_internalState == InternalState.LightningBreak) return;

        if (pc.InputAxis.y == 0)
        {
            // Slowly decrement velocity if no forward/back input is detected
            pc.MoveSpeed -= _speedLossRate * Time.fixedDeltaTime;
            pc.MoveSpeed = Mathf.Clamp(pc.MoveSpeed, 0f, pc.MoveSpeed);
            rb.velocity = rb.velocity.normalized * pc.MoveSpeed;
            if (pc.MoveSpeed < pc.ThresholdSpeed - _tolerance)
            {
                _internalState = InternalState.Levitate;
                pc.animator.SetFloat(pc.moveSpeedID, 0f);
            }
        }
        else
        {
            _timeSinceStartedLevitating = 0f;
            _internalState = InternalState.Moving;
        }
    }

    private bool CheckGroundReturning(in PlayerController pc)
    {
        if (_internalState != InternalState.Levitate) return false;

        _timeSinceStartedLevitating += Time.fixedDeltaTime;
        if (_timeSinceStartedLevitating < _levitatingTime) return false;

        //Switch to ground movement after too much time levitating
        _timeSinceStartedLevitating = 0f;

        pc.SwitchState(MovementType.Ground);
        return true;
    }

    private void ApplyVelocity(in Vector2 input)
    {
        var pc = _movementComponents.PlayerController;
        var orientation = _movementComponents.Orientation;

        if (_internalState == InternalState.Levitate)
        {
            _movementComponents.PlayerController.MoveSpeed = 0f;
            return;
        }

        if (_internalState == InternalState.LightningBreak)
        {
            if (input != Vector2.zero)
                PerformLightningBreak(pc, orientation);
            return;
        }

        float realForwardSpeed = pc.IsBoosting ? _maxMoveSpeed : _forwardSpeed;
        float realDamping = 1.0f - _accelDamping;
        _activeForwardSpeed = Mathf.Lerp(_activeForwardSpeed, input.y * realForwardSpeed, _forwardAccel * realDamping * Time.fixedDeltaTime);
        _activeStrafeSpeed = Mathf.Lerp(_activeStrafeSpeed, input.x * _strafeSpeed, _strafeAccel * realDamping * Time.fixedDeltaTime);

        // TODO: Rotate player object when receiving sideways input movement

        Vector3 velocity = _activeForwardSpeed * orientation.forward + _activeStrafeSpeed * orientation.right;
        _movementComponents.Rigidbody.velocity = velocity;

        // Minimum speed tolerance before changing to Ground movement
        _usedTolerance = input.y == 0f ? Mathf.Lerp(_usedTolerance, 0.25f, Time.fixedDeltaTime) : 1f;
        pc.MoveSpeed = Mathf.Clamp(velocity.magnitude, _initialMoveSpeed - _usedTolerance, _maxMoveSpeed);
        
        //Set MoveSpeed to animator
        var currentSpeed = (pc.MoveSpeed - _initialMoveSpeed) / (_maxMoveSpeed - _initialMoveSpeed) + 1f;
        Debug.Log("Current speed now is: " + currentSpeed);
        pc.animator.SetFloat(pc.moveSpeedID, 
            Mathf.SmoothDamp(pc.animator.GetFloat(pc.moveSpeedID), 
                currentSpeed, ref pc.moveSpeedDamp, 0.1f));
    }

    private void PerformLightningBreak(in PlayerController pc, in Transform orientation)
    {
        Vector3 pos = transform.position;
        Vector2 dashInput = 15f * pc.InputAxis;
        Vector3 dashPos = pos + dashInput.x * orientation.right + dashInput.y * orientation.up;
        pc.DashTo(dashPos, null);
        pc.PlayerEnergy -= pc.playerEnergyLostOnBoost;
        ResetLightningBreak();
    }

    private void ResetHomingAttack()
    {
        var pc = _movementComponents.PlayerController;
        pc.IsHomingAttacking = false;
    }

    private void ResetLightningBreak()
    {
        var pc = _movementComponents.PlayerController;
        pc.IsLightningBreaking = false;
        _internalState = InternalState.Moving;
    }
    
    public void CheckAndPlayMoveAudio()
    {
        if (_movementComponents.PlayerController.InputAxis != Vector2.zero)
        {
            _movementComponents.PlayerController.audioManager.PlayAudioLoop("FlightBase");
            return;
        }
        //If equals zero, then it is stopped
        _movementComponents.PlayerController.audioManager.StopPlayingAll();
    }

    private void PlayBoostAudio()
    {
        _movementComponents.PlayerController.audioManager.PlayAudioLoop("FlightFast");
    }
    
    public void PlayRandomAttackAudio()
    {
        int rand = Random.Range(1, 5); //Random [1-4]
        string attackAudio = "Slash" + rand;
        _movementComponents.PlayerController.audioManager.PlayAudioButton(attackAudio);
    }
    
    
    #endregion

    #region InputSystemCallbacks

    public void OnMovement(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (!pc.MoveType.Equals(MovementType.Air)) return;
        
        _movementComponents.PlayerController.InputAxis = context.ReadValue<Vector2>();
        CheckAndPlayMoveAudio();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;

        if (!pc.MoveType.Equals(MovementType.Air)) return;
        if (context.performed && pc.PlayerEnergy > 0.0f)
        {
            Debug.Log("Attack pressed: ");
            _movementComponents.PlayerController.IsAttacking = true;
            return;
        }

        Debug.Log("Attack released");
        _movementComponents.PlayerController.IsAttacking = false;
    }

    public void OnHomingAttack(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (!pc.MoveType.Equals(MovementType.Air)) return;
        if (pc.IsHomingAttacking) return;
        if (pc.PlayerEnergy < pc.playerEnergyLostOnHomingAttack) return;

        Debug.Log("Attack homing");
        if (!pc.playersManager) return;
        
        Vector3 nearestEnemyPosition = pc.playersManager.GetLocalNearestEnemy();
        if (float.IsPositiveInfinity(nearestEnemyPosition.x)) return;
        
        //Get nearest enemy to anywhere
        pc.IsHomingAttacking = true;
        pc.DashTo(nearestEnemyPosition, null);
        pc.PlayerEnergy -= pc.playerEnergyLostOnHomingAttack;

        //ToDo: reset movement speed to zero on this case
        
        Invoke(nameof(ResetHomingAttack), 1.0f);
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (!pc.MoveType.Equals(MovementType.Air)) return;
        if (pc.PlayerEnergy < pc.playerEnergyLostOnBoost) return;

        pc.IsBoosting = true;
        _timeSinceLastBoost = 0f;
        _activeForwardSpeed = _maxMoveSpeed;
        pc.PlayerEnergy -= pc.playerEnergyLostOnBoost;

        //Boost Audio
        PlayBoostAudio();
        
        // Boost particle system.
        pc.CloudPS.transform.position = pc.transform.position;
        pc.CloudPS.Clear();
        pc.CloudPS.Play();
        
    }

    public void OnLightningBreak(InputAction.CallbackContext context)
    {
        var pc = _movementComponents.PlayerController;
        if (!pc.MoveType.Equals(MovementType.Air)) return;
        if (pc.IsLightningBreaking) return;
        if (pc.PlayerEnergy < pc.playerEnergyLostOnLightningBreak) return;

        pc.IsLightningBreaking = true;
        _internalState = InternalState.LightningBreak;
        Invoke(nameof(ResetLightningBreak), 1.0f);
    }
    #endregion
}