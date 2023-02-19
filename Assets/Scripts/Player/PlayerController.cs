using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Components
    private MoveInput _input;
    private Rigidbody _rb;
    public Transform orientation;
    public ThirdPersonCam thirdPersonCam;

    //Movement types
    private FlightController _flightMovement;
    private PlayerMovement _groundMovement;
    
    //Speed attributes 
    [Header("Movement")]
    public float initialMoveSpeed = 5.0f;
    [HideInInspector]
    public float moveSpeed;
    public float thresholdSpeed = 20.0f;

    public Vector2 inputAxis;
    
    //State
    public Movement _moveType = Movement.Ground;
    
    //Angle limits on air
    public float rotXLimit = 30.0f;
    public float rotYLimit = 30.0f;

    private void Awake()
    {
        _input = new MoveInput();
        _rb = GetComponent<Rigidbody>();

        _flightMovement = GetComponent<FlightController>();
        _groundMovement = GetComponent<PlayerMovement>();
        
        moveSpeed = initialMoveSpeed;

        thirdPersonCam = Camera.main.GetComponent<ThirdPersonCam>();
    }

    void Start()
    {
        _groundMovement.Initialize(_input, _rb, orientation, this);
        _flightMovement.Initialize(_input, _rb, orientation, this);
        
        //_rb.useGravity = false; //ToDo: add this on change
    }

    // Update is called once per frame
    void Update()
    {
        switch (_moveType)
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
        switch (_moveType)
        {
            case Movement.Ground:
                _groundMovement.OnFixedUpdate();
                break;
            case Movement.Air:
                _flightMovement.OnFixedUpdate();
                break;
        }
        
    }
    
    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    public void SwitchState(Movement newState)
    {
        if (newState.Equals(Movement.Ground))
        {
            _rb.useGravity = true;
            moveSpeed = initialMoveSpeed;
            _rb.velocity = new Vector3(0.0f, _rb.velocity.y, 0.0f);
            
            thirdPersonCam.SwapCamera(Movement.Ground);
        }
        else
        {
            Debug.Log("Current rb vel previous to change: " + _rb.velocity);
            moveSpeed = _rb.velocity.magnitude;
            _rb.useGravity = false; //When changing to flight, dont update moveSpeed
            
            thirdPersonCam.SwapCamera(Movement.Air);
        }
        
        _moveType = newState;
        Debug.Log("Rb vel after change: " + _rb.velocity);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer.Equals("Ground")) return;
        else
        {
            //Reset speed and switch to ground
            moveSpeed = 0.0f;
            if(_moveType.Equals(Movement.Air)) SwitchState(Movement.Ground);
        }
    }

    public Vector3 GetFlightForward()
    {
        return _flightMovement.originalForward;
    }
}

public enum Movement
{
    Ground,
    Air,
}