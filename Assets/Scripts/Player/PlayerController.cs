using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GroundMovement))]
[RequireComponent(typeof(FlyMovement))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    //Components
    private MoveInput _input;
    private Rigidbody _rb;
    [SerializeField] private Transform _orientation;
    private ThirdPersonCam _thirdPersonCam;

    //Movement types
    private FlyMovement _flightMovement;
    private GroundMovement _groundMovement;

    //Speed attributes 
    [Header("Movement")]
    public float InitialMoveSpeed = 5.0f;
    public float ThresholdSpeed = 20.0f;
    public float MaxMoveSpeed = 80.0f;
    public float MoveSpeed { get; set; }

    // Input
    public Vector2 InputAxis;

    //State
    public Movement MoveType = Movement.Ground;

    //Angle limits on air
    public float RotXLimit = 60.0f;
    public float EnemySpeedThreshold = 15.0f;

    //Enemy bounds 
    public bool InEnemyBounds { get; set; }
    public GameObject EnemyCollided { get; set; }

    //UI Components
    [Header("UI Components")]
    public SpeedBar SpeedUI;

    public LayerMask GroundMask;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _input = new MoveInput();
        _rb = GetComponent<Rigidbody>();

        _flightMovement = GetComponent<FlyMovement>();
        _groundMovement = GetComponent<GroundMovement>();

        MoveSpeed = InitialMoveSpeed;

        _thirdPersonCam = Camera.main.GetComponent<ThirdPersonCam>();
    }

    void Start()
    {
        _groundMovement.Initialize(_input, _rb, _orientation, this);
        _flightMovement.Initialize(_input, _rb, _orientation, this);
    }

    // Update is called once per frame
    void Update()
    {
        switch (MoveType)
        {
            case Movement.Ground:
                _groundMovement.OnUpdate();
                break;
            case Movement.Air:
                _flightMovement.OnUpdate();
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (MoveType)
        {
            case Movement.Ground:
                _groundMovement.OnFixedUpdate();
                break;
            case Movement.Air:
                _flightMovement.OnFixedUpdate();
                break;
        }

        SpeedUI.UpdateHealthBar();
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void OnCollisionEnter(Collision collision)
    {
        if ((GroundMask & (1 << collision.gameObject.layer)) != 0) return;
        // if (collision.gameObject.layer.Equals(groundMask.value)) return;

        //Reset speed and switch to ground
        Debug.Log("Collision without ground: " + collision.gameObject.name);
        Debug.Log("Layer is: " + collision.gameObject.layer);
        MoveSpeed = 0.0f;
        InputAxis = Vector2.zero;
        if (MoveType.Equals(Movement.Air)) SwitchState(Movement.Ground);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy bounds");
            InEnemyBounds = true;
            EnemyCollided = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;
        if (EnemyCollided == null) return;

        Debug.Log("Reset enemy and stop");
        MoveSpeed = 0.0f;
        InputAxis = Vector2.zero;
        if (MoveType.Equals(Movement.Air)) SwitchState(Movement.Ground);

        Invoke(nameof(ResetInBounds), 0.5f);
    }
    #endregion

    #region Functions
    public void SwitchState(Movement newState)
    {
        if (newState.Equals(Movement.Ground))
        {
            _rb.useGravity = true;
            _rb.velocity = new Vector3(0.0f, _rb.velocity.y, 0.0f);
            MoveSpeed = InitialMoveSpeed;

            _thirdPersonCam.SwapCamera(Movement.Ground);
        }
        else
        {
            _rb.useGravity = false; //When changing to flight, dont update moveSpeed
            Debug.Log("Current rb vel previous to change: " + _rb.velocity);
            MoveSpeed = _rb.velocity.magnitude;

            _thirdPersonCam.SwapCamera(Movement.Air);
        }

        MoveType = newState;
        Debug.Log("Rb vel after change: " + _rb.velocity);
    }

    private void ResetInBounds()
    {
        EnemyCollided = null;
        InEnemyBounds = false;
    }
    #endregion
}

public enum Movement
{
    Ground,
    Air,
}