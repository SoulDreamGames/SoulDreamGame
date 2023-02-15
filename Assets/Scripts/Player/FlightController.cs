using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoveInput;

public class FlightController : MonoBehaviour, IFlyActions
{
    public float forwardSpeed = 25f;
    public float strafeSpeed = 7.5f;
    public float hoverSpeed = 5f;
    private float moveSpeed = 5f;
    private float initialMoveSpeed = 20f;
    public float moveAccel = 0.1f;
    public float maxMoveSpeed = 100.0f;

    private float activeForwardSpeed;
    private float activeStrafeSpeed;
    private float activeMoveSpeed;

    private float brakeFactor = 1.0f;

    private float _hoverAxis;
    private Vector2 _movement;

    private MoveInput _input;

    public Transform orientation;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        _input = new MoveInput();
    }

    void Start()
    {
        moveSpeed = initialMoveSpeed;

        _input.Fly.Movement.performed += OnMovement;
        _input.Fly.Movement.canceled += OnMovement;

        _input.Fly.Hover.performed += OnHover;
        _input.Fly.Hover.canceled += OnHover;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    float smoothVel = 0.0f;
    // Update is called once per frame
    void Update()
    {
        if (_movement.y != 0)
        {
            moveSpeed += moveAccel * Time.deltaTime;
            moveSpeed = Mathf.Clamp(moveSpeed, initialMoveSpeed, maxMoveSpeed);
            brakeFactor = 1.0f;
            Debug.Log("Current speed: " + moveSpeed);
        }
        else
        {
            moveSpeed = Mathf.SmoothDamp(moveSpeed, 0.0f, ref smoothVel, 0.2f);
            if (moveSpeed > 0.01f)
                brakeFactor = -5.0f;
            else brakeFactor = 0.0f;
        }

        //ToDo: disable gravity when enabling this script, so hover makes sense
        activeForwardSpeed = _movement.y * forwardSpeed;
        activeStrafeSpeed = _movement.x * strafeSpeed;
        activeMoveSpeed = _hoverAxis * hoverSpeed; //Forward and back

        Vector3 moveDir = orientation.forward * activeForwardSpeed * Time.deltaTime
            + orientation.right * activeStrafeSpeed * Time.deltaTime;

        Vector3 movement = moveDir * moveSpeed * 10.0f * Time.deltaTime;

        if(brakeFactor == 0.0f)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        rb.AddForce(new Vector3(
            movement.x,
            activeMoveSpeed * Time.deltaTime,
            movement.z) * brakeFactor, ForceMode.VelocityChange);
    }

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void OnHover(InputAction.CallbackContext context)
    {
        _hoverAxis = context.ReadValue<float>();
    }
}
