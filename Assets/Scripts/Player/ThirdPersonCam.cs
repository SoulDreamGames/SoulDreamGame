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
    CinemachineFreeLook[] cameraBehaviours = new CinemachineFreeLook[2];
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public void SwapCamera(Movement movement)
    {
        if (movement.Equals(Movement.Ground))
        {
            cameraBehaviours[1].gameObject.SetActive(false);
            cameraBehaviours[0].gameObject.SetActive(true);
        }
        else //Switching to air
        {
            //Set current rot value to new component
            float currentValue = cameraBehaviours[0].m_XAxis.Value;
            
            cameraBehaviours[1].m_XAxis.Value = currentValue;
            cameraBehaviours[1].m_XAxis.m_MaxValue = currentValue + pc.rotXLimit;
            cameraBehaviours[1].m_XAxis.m_MinValue = currentValue - pc.rotXLimit;
                
            cameraBehaviours[0].gameObject.SetActive(false);
            cameraBehaviours[1].gameObject.SetActive(true);
        }
    }
    private void Update(){

        // Rotate orientation
        Vector3 lookAt = player.position -
                         new Vector3(transform.position.x, player.position.y, transform.position.z);
        
        Vector3 realLookAt = (player.position -
                       new Vector3(transform.position.x, transform.position.y, transform.position.z)).normalized;


        //Limit angles on air movement
        if (pc._moveType.Equals(Movement.Air))
        {
            // realLookAt = realLookAt.normalized;
            // Vector3 _originalForward = pc.GetFlightForward();
            //
            // Debug.Log("Forward from orientation: " + realLookAt);
            //
            // float angleY = Vector3.SignedAngle(_originalForward, realLookAt, Vector3.up);
            // float angleX =
            //     Vector3.SignedAngle(_originalForward, realLookAt, transform.right);
            //
            // //Angle limits
            // if (angleX > pc.rotXLimit) angleX = pc.rotXLimit;
            // else if (angleX < -pc.rotXLimit) angleX = -pc.rotXLimit;
            //
            // if (angleY > pc.rotYLimit) angleY = pc.rotYLimit;
            // else if (angleY < -pc.rotYLimit) angleY = pc.rotYLimit;
            //
            // //Get new forward with limited angles
            // realLookAt = Quaternion.Euler(angleX, angleY, 0.0f) * _originalForward;
            // Debug.Log("New forward: " + realLookAt);
        }

        transform.position = player.position - realLookAt * lookAt.magnitude;
        orientation.forward = pc._moveType.Equals(Movement.Ground) ? lookAt.normalized : realLookAt.normalized;

        //Rotate player
        float horizontalInput = pc.inputAxis.x;
        float verticalInput = pc.inputAxis.y;
        Vector3 input = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (input != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, input.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}
