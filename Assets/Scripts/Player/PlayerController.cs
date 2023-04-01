using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;
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
    [SerializeField, Tooltip("Energy lost per update when using hold attack")] 
    public float playerEnergyLost = 0.5f;
    [SerializeField, Tooltip("Energy lost on homing attack activation")] 
    public float playerEnergyLostOnHomingAttack = 25f;
    [SerializeField] private float _maxEnergy = 100.0f;
    [SerializeField, Tooltip("Homing attack radius")] 
    public float homingRadius = 10f;
    
    //GameManager
    [HideInInspector] public PlayersManager playersManager;
    
    //UI Components
    [Header("UI Components")]
    public SpeedBar SpeedUI;
    public LayerMask GroundMask;

    [HideInInspector] public PhotonView view;
    private InGameMenu _menu;

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
    public float PlayerEnergy { get; set; }
    public float MaxEnergy
    {
        get => _maxEnergy;
    }
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
    public bool IsAttacking { get; set; }
    public bool IsHomingAttacking { get; set; }

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
        PlayerEnergy = 0.0f;
    }

    void Start()
    {
        view = GetComponent<PhotonView>();
        _menu = FindObjectOfType<InGameMenu>();

        playersManager = FindObjectOfType<PlayersManager>();
        if(playersManager)
            playersManager.AddPlayer(this);
        
        if (!view.IsMine) return;

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
        if (!view.IsMine) return;
        if (!_menu.EnableInGameControls) InputAxis = Vector2.zero;
        
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
        if (!view.IsMine) return;
        
        switch (MoveType)
        {
            case MovementType.Ground:
                _groundMovement.OnFixedUpdate();
                break;
            case MovementType.Air:
                _flightMovement.OnFixedUpdate();
                break;
        }

        SpeedUI.UpdateUIBars();
    }

    //ToDo: add a list of effects for lightning break + homing attack
    public void DashTo(Vector3 targetPosition, Transform pDashEffect)
    {
        //Add a visual effect based on a prefab
        if (pDashEffect != null)
        {
            Transform dashEffect = Instantiate(pDashEffect, transform.position, quaternion.identity);
        }

        //Finally, move to desired position
        transform.position = targetPosition;
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
            Debug.Log("Enemy");

            if (IsAttacking & MoveSpeed >= EnemySpeedThreshold)
            {
                Debug.Log("Attacking enemy");
                var enemy = other.GetComponent<EnemyBehaviour>();
                if (enemy == null) return;

                bool isDead = enemy.ReceiveDamage(3);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;

        Debug.Log("Reset enemy and stop");
        
        //Reset velocity and energy in player
        MoveSpeed = 0.0f;
        PlayerEnergy = 0.0f;
        InputAxis = Vector2.zero;
        
        //ToDo: recheck this?
        if (MoveType.Equals(MovementType.Air)) SwitchState(MovementType.Ground);
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
