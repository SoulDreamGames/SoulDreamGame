using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Reference Objects")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;

    public PlayerController pc;
    
    public float rotationSpeed;

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
#if false
            float currentValue = cameraBehaviours[0].m_XAxis.Value;
            
            cameraBehaviours[1].m_XAxis.Value = currentValue;
            cameraBehaviours[1].m_XAxis.m_MaxValue = currentValue + pc.RotXLimit;
            cameraBehaviours[1].m_XAxis.m_MinValue = currentValue - pc.RotXLimit;
#endif
                
            cameraBehaviours[0].gameObject.SetActive(false);
            cameraBehaviours[1].gameObject.SetActive(true);
        }
    }
    private void Update()
    {

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
