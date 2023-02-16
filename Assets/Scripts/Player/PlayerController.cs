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

    //Movement types
    private FlightController _flightMovement;
    private PlayerMovement _groundMovement;
    
    //State
    private Movement _moveType = Movement.Ground;

    private void Awake()
    {
        _input = new MoveInput();
        _rb = GetComponent<Rigidbody>();

        _flightMovement = GetComponent<FlightController>();
        _groundMovement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        _groundMovement.Initialize(_input, _rb, orientation);
        _flightMovement.Initialize(_input, _rb, orientation);
        
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
}

enum Movement
{
    Ground,
    Air,
}