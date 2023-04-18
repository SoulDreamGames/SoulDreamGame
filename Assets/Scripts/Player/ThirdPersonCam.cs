using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Reference Objects")]
    [HideInInspector] public Transform orientation;
    [HideInInspector] public Transform player;
    [HideInInspector] public Transform playerObj;

    [HideInInspector] public PlayerController pc;
    
    [SerializeField] private float rotationSpeed;

    [SerializeField]
    private CinemachineFreeLook[] cameraBehaviours = new CinemachineFreeLook[2];

    private float _startFOV;
    private float _fovMultiplier = 1.3f;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _startFOV = cameraBehaviours[1].m_Lens.FieldOfView;
    }

    public void SwapCamera(MovementType movement)
    {
        if (movement.Equals(MovementType.Ground))
        {
            cameraBehaviours[1].gameObject.SetActive(false);
            cameraBehaviours[0].gameObject.SetActive(true);
        }
        else //Switching to air
        {
            //Set current rot value to new component
            float currentValue = cameraBehaviours[0].m_XAxis.Value;
            cameraBehaviours[1].m_XAxis.Value = currentValue;
            
            cameraBehaviours[0].gameObject.SetActive(false);
            cameraBehaviours[1].gameObject.SetActive(true);
        }
    }

    private bool _isFixed = false;
    public void SwapToFixedTarget()
    {
        _isFixed = true;
        
        cameraBehaviours[0].gameObject.SetActive(false);
        cameraBehaviours[1].gameObject.SetActive(false);
    }

    public void SwapToPlayerTarget()
    {
        cameraBehaviours[0].gameObject.SetActive(true);
        _isFixed = false;
    }

    public void RotateCameraAfterLightningBreak(float degrees)
    {
        if (_isFixed) return;
        if (player == null) return;

        Debug.Log(string.Format("<color=#00FF00>{0}</color>", degrees));
        cameraBehaviours[1].m_XAxis.Value += degrees;
    }
    
    private void Update()
    {
        if (_isFixed) return;
        if (player == null) return;
        
        // Rotate orientation
        Vector3 lookAt = player.position -
                         new Vector3(transform.position.x, player.position.y, transform.position.z);
        
        Vector3 realLookAt = (player.position -
                       new Vector3(transform.position.x, transform.position.y, transform.position.z)).normalized;


        // Check boosting fov if player is flying
        if (pc.MoveType.Equals(MovementType.Air))
        {
            float realFovMultiplier = pc.IsBoosting ? _fovMultiplier : 1.0f;
            float lerpVelocity = pc.IsBoosting ? 0.6f : 0.6f * Time.deltaTime;
            cameraBehaviours[1].m_Lens.FieldOfView = Mathf.Lerp(cameraBehaviours[1].m_Lens.FieldOfView, _startFOV * realFovMultiplier, lerpVelocity);
        }

        transform.position = player.position - realLookAt * lookAt.magnitude;
        orientation.forward = pc.MoveType.Equals(MovementType.Ground) ? lookAt.normalized : realLookAt.normalized;

        //Rotate player
        float horizontalInput = pc.InputAxis.x;
        float verticalInput = pc.InputAxis.y;
        Vector3 input = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (input != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, input.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}
