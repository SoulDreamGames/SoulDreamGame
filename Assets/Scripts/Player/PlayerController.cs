using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GroundMovement))]
[RequireComponent(typeof(FlyMovement))]
public class PlayerController : MonoBehaviour, IPunObservable
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
    [SerializeField] private float _maxMoveSpeed = 100.0f;
    //State
    [SerializeField] public MovementType moveType = MovementType.Ground;
    //Angle limits on air
    public float RotXLimit = 60.0f;
    public float EnemySpeedThreshold = 15.0f;
    [SerializeField, Tooltip("Energy lost per update when using hold attack")] 
    public float playerEnergyLost = 0.5f;
    [SerializeField, Tooltip("Energy lost on homing attack activation")] 
    public float playerEnergyLostOnHomingAttack = 25f;
    [SerializeField, Tooltip("Energy lost on boost activation")] 
    public float playerEnergyLostOnBoost = 25f;
    [SerializeField, Tooltip("Energy lost on boost activation")]
    public float playerEnergyLostOnLightningBreak = 35f;
    [SerializeField] private float _maxEnergy = 100.0f;
    [SerializeField, Tooltip("Homing attack radius")] 
    public float homingRadius = 10f;
    
    //GameManager
    [HideInInspector] public PlayersManager playersManager;
    
    //UI Components
    [Header("UI Components")]
    public PlayerBarsUI SpeedUI;
    public LayerMask GroundMask;
    public LayerMask enemyMask;

    [HideInInspector] public PhotonView view;
    private InGameMenu _menu;

    [Header("Debug Info")]
    [SerializeField] private Vector2 _inputAxis; // Input
    [SerializeField] private bool _godlike = false;
    
    //Animator
    [HideInInspector] public Animator animator;
    [HideInInspector] public int moveSpeedID;
    [HideInInspector] public int jumpID;
    [HideInInspector] public int isFlyingID;
    [HideInInspector] public int isGroundedID;
    [HideInInspector] public float moveSpeedDamp;
    
    
    //CD attacked
    private bool _isInvulnerable = false;
    [SerializeField] private float invulnerabilityTime = 1.0f;
    
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
        get => moveType;
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
    public bool IsBoosting { get; set; }
    public bool IsLightningBreaking { get; set; }

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
        view = GetComponent<PhotonView>();

        _flightMovement = GetComponent<FlyMovement>();
        _groundMovement = GetComponent<GroundMovement>();

        _thirdPersonCam = Camera.main.GetComponent<ThirdPersonCam>();

        MoveSpeed = InitialMoveSpeed;
        PlayerEnergy = 0.0f;

        animator = GetComponent<Animator>();
        SetAnimatorProperties();
        
        //ToDo:
        //_animator.SetBool(_animIDGrounded, Grounded);
    }

    //Animator properties to hash
    void SetAnimatorProperties()
    {
        moveSpeedID = Animator.StringToHash("MoveSpeed");
        jumpID = Animator.StringToHash("Jump");
        isFlyingID = Animator.StringToHash("isFlying");
        isGroundedID = Animator.StringToHash("isGrounded");
    }
    
    void Start()
    {
        _menu = FindObjectOfType<InGameMenu>();

        playersManager = FindObjectOfType<PlayersManager>();
        if(playersManager)
            playersManager.AddPlayer(this);
        
        if (!view.IsMine) return;

        SpeedUI = FindObjectOfType<PlayerBarsUI>();
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
            Transform dashEffect = Instantiate(pDashEffect, transform.position, Quaternion.identity);
        }

        //Finally, move to desired position
        transform.position = targetPosition;
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void OnCollisionEnter(Collision collision)
    {
        if (!view.IsMine) return;
        
        Debug.Log("Can collide is: " + _canCollide);
        if (MoveType.Equals(MovementType.Air) && _canCollide)
        {
            PlayerEnergy = 0f;
            SwitchState(MovementType.Ground);
        }
        if ((GroundMask & (1 << collision.gameObject.layer)) != 0) return;
        // if (collision.gameObject.layer.Equals(groundMask.value)) return;

        //Reset speed and switch to ground
        Debug.Log("Collision without ground: " + collision.gameObject.name, collision.gameObject);
        Debug.Log("Layer is: " + collision.gameObject.layer);
        MoveSpeed = 0.0f;
        InputAxis = Vector2.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!view.IsMine) return;

        if (other.CompareTag("Water"))
        {
            PhotonNetwork.Instantiate("WaterDropsPs", transform.position + transform.up, quaternion.identity);
            HandleDeath();
        }
        
        if ((enemyMask & (1 << other.gameObject.layer)) == 0) return;
        
        //Enemy is triggering
        if ((IsAttacking || IsHomingAttacking) && MoveSpeed >= EnemySpeedThreshold)
        {
            var enemy = other.GetComponent<EnemyBehaviour>();
            if (enemy == null) return;
            
            enemy.ReceiveDamage(3);
            return;
        }
        else
        {
            if (_isInvulnerable) return;
            
            //If collision without attacking or not enough velocity
            var enemy = other.GetComponent<EnemyBehaviour>();
            if (enemy == null) return;
            
            //Player is attacked
            _isInvulnerable = true;
            Debug.Log("Dmg rcv: " +  enemy.ContactDamage);
            ReceiveDamage(enemy.ContactDamage);
            Invoke(nameof(ResetInvulnerability), invulnerabilityTime);   //Invulnerability cd
            
        }

        //Reset velocity and energy in player
        MoveSpeed = 0.0f;
        InputAxis = Vector2.zero;
    
        //ToDo: recheck this?
        if (MoveType.Equals(MovementType.Air)) SwitchState(MovementType.Ground);

    }
    
    private void ResetInvulnerability()
    {
        _isInvulnerable = false;
    }

    public void ReceiveDamage(float damage)
    {
        PlayerEnergy =  PlayerEnergy - damage;
        PhotonNetwork.Instantiate("BloodPS", transform.position, quaternion.identity);

        if (PlayerEnergy <= 0f)
        {
            HandleDeath();
        }
    }
    
    private void HandleDeath()
    {
        view.RPC("HandleDeathRPC", RpcTarget.All);
    }

    [PunRPC]
    public void HandleDeathRPC()
    {
        if (!view.IsMine) return;
        
        Debug.Log("player is dead");   
        _thirdPersonCam.SwapToFixedTarget();

        _flightMovement.ResetMovement();
        _groundMovement.ResetMovement();
            
        view.RPC("SetScaleForRespawn", RpcTarget.All, new object[]{ 0.0f, true} );
        playersManager.PlayerDied(this);
    }

    public void Respawn(Vector3 position)
    {
        if (!view.IsMine) return;
        
        Debug.Log("Respawn");
        view.RPC("SetScaleForRespawn", RpcTarget.All, new object[]{1.0f, false});
        
        //Reset transforms
        transform.position = position;
        transform.rotation = Quaternion.identity;
        _playerObject.transform.rotation = Quaternion.identity;

        _rb.useGravity = true;
        
        _thirdPersonCam.SwapToPlayerTarget();
    }

    [PunRPC]
    private void SetScaleForRespawn(float scale, bool hideObject)
    {
        transform.localScale = Vector3.one * scale;
        
        if(hideObject)
            transform.position = Vector3.zero;
    }
    
    #endregion

    #region Functions
    public void SwitchState(MovementType newState)
    {
        if (newState.Equals(MovementType.Ground))
        {
            _flightMovement.ResetMovement();
            _groundMovement.SetSubState(GroundMovement.InternalState.Falling);
            _thirdPersonCam.SwapCamera(MovementType.Ground);

            animator.SetBool(isFlyingID, false);
        }
        else
        {
            _groundMovement.ResetMovement();
            _flightMovement.SetSubState(FlyMovement.InternalState.Levitate);
            _thirdPersonCam.SwapCamera(MovementType.Air);
            
            animator.SetBool(isFlyingID, true);
            animator.SetBool(isGroundedID, false);

            //Reset collisions
            _canCollide = false;
            Invoke(nameof(DisableCollisionInvulnerability), 0.75f);
        }

        moveType = newState;
        Debug.Log("Rb vel after change: " + _rb.velocity);
    }

    private bool _canCollide = true;
    private void DisableCollisionInvulnerability()
    {
        _canCollide = true;
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

    //Sync playerAttacks
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsAttacking);
            stream.SendNext(IsHomingAttacking);
        }
        else if (stream.IsReading)
        {
            IsAttacking = (bool)stream.ReceiveNext();
            IsHomingAttacking = (bool)stream.ReceiveNext();
        }
    }
}
