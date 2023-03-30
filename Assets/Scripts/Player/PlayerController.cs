using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GroundMovement))]
[RequireComponent(typeof(FlyMovement))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    //Components
    private MoveInput _input;
    private Rigidbody _rb;
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private Transform _orientation;
    private ThirdPersonCam _thirdPersonCam;

    //Movement types
    private FlyMovement _flightMovement;
    private GroundMovement _groundMovement;

    [Header("Movement")]
    //Speed attributes
    [SerializeField] private float _initialMoveSpeed = 5.0f;
    [SerializeField] private float _thresholdSpeed = 20.0f;
    [SerializeField] private float _maxMoveSpeed = 80.0f;
    //State
    [SerializeField] private MovementType _moveType = MovementType.Ground;
    //Angle limits on air
    public float RotXLimit = 60.0f;
    public float EnemySpeedThreshold = 15.0f;

    //UI Components
    [Header("UI Components")]
    public SpeedBar SpeedUI;
    public LayerMask GroundMask;

    private PhotonView _view;

    [Header("Debug Info")]
    [SerializeField] private Vector2 _inputAxis; // Input
    [SerializeField] private bool _godlike = false;
    #endregion

    #region Properties
    public float InitialMoveSpeed
    {
        get => _initialMoveSpeed;
    }
    public float ThresholdSpeed
    {
        get => _thresholdSpeed;
    }
    public float MaxMoveSpeed
    {
        get => _maxMoveSpeed;
    }
    public float MoveSpeed { get; set; }
    public MovementType MoveType
    {
        get => _moveType;
    }

    // Input
    public Vector2 InputAxis
    {
        get => _inputAxis;
        set => _inputAxis = value;
    }

    // Enemy bounds 
    public bool InEnemyBounds { get; set; }
    public GameObject EnemyCollided { get; set; }

    public GameObject PlayerObject
    {
        get => _playerObject;
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _input = new MoveInput();
        _rb = GetComponent<Rigidbody>();

        _flightMovement = GetComponent<FlyMovement>();
        _groundMovement = GetComponent<GroundMovement>();

        _thirdPersonCam = Camera.main.GetComponent<ThirdPersonCam>();

        MoveSpeed = InitialMoveSpeed;
    }

    void Start()
    {
        _view = GetComponent<PhotonView>();

        if (!_view.IsMine) return;

        SpeedUI = FindObjectOfType<SpeedBar>();
        SpeedUI.Player = this;
        Debug.Log("Speed UI: " + SpeedUI.name);
        MovementComponents components = new()
        {
            Input = _input,
            Rigidbody = _rb,
            Orientation = _orientation,
            PlayerController = this,
        };

        _groundMovement.Initialize(components);
        _flightMovement.Initialize(components);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_view.IsMine) return;
        
        switch (MoveType)
        {
            case MovementType.Ground:
                _groundMovement.OnUpdate();
                break;
            case MovementType.Air:
                _flightMovement.OnUpdate();
                break;
        }
#if UNITY_EDITOR
        if (_godlike) PerformGodlikeActions();
#endif
    }

    private void FixedUpdate()
    {
        if (!_view.IsMine) return;
        
        switch (MoveType)
        {
            case MovementType.Ground:
                _groundMovement.OnFixedUpdate();
                break;
            case MovementType.Air:
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
        Debug.Log("Collision without ground: " + collision.gameObject.name, collision.gameObject);
        Debug.Log("Layer is: " + collision.gameObject.layer);
        MoveSpeed = 0.0f;
        InputAxis = Vector2.zero;
        if (MoveType.Equals(MovementType.Air)) SwitchState(MovementType.Ground);
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
        if (MoveType.Equals(MovementType.Air)) SwitchState(MovementType.Ground);

        Invoke(nameof(ResetInBounds), 0.5f);
    }
    #endregion

    #region Functions
    public void SwitchState(MovementType newState)
    {
        if (newState.Equals(MovementType.Ground))
        {
            _flightMovement.ResetMovement();
            _thirdPersonCam.SwapCamera(MovementType.Ground);
        }
        else
        {
            _groundMovement.ResetMovement();
            _thirdPersonCam.SwapCamera(MovementType.Air);
        }

        _moveType = newState;
        Debug.Log("Rb vel after change: " + _rb.velocity);
    }

    private void ResetInBounds()
    {
        EnemyCollided = null;
        InEnemyBounds = false;
    }

    private void PerformGodlikeActions()
    {
        MoveUpByAnAmount();
        CheckGodlikeChangeStatus();
    }

    private void MoveUpByAnAmount()
    {
        if (Keyboard.current[Key.U].wasPressedThisFrame)
        {
            transform.position += Vector3.up * 10f;
        }
    }

    private void CheckGodlikeChangeStatus()
    {
        if (Keyboard.current[Key.G].wasPressedThisFrame)
        {
            switch (MoveType)
            {
                case MovementType.Ground:
                    SwitchState(MovementType.Air);
                    break;
                case MovementType.Air:
                    SwitchState(MovementType.Ground);
                    break;
            }
        }
    }
    #endregion
}
