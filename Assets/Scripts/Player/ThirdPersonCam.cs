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
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    private void Update(){

        // Rotate orientation
        Vector3 lookAt = player.position -
                         new Vector3(transform.position.x, player.position.y, transform.position.z);
        
        Vector3 realLookAt = (player.position -
                       new Vector3(transform.position.x, transform.position.y, transform.position.z)).normalized;


        //Limit angles on air movement
        if (pc.MoveType.Equals(MovementType.Air))
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
        orientation.forward = pc.MoveType.Equals(MovementType.Ground) ? lookAt.normalized : realLookAt.normalized;

        //Rotate player
        float horizontalInput = pc.InputAxis.x;
        float verticalInput = pc.InputAxis.y;
        Vector3 input = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (input != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, input.normalized, Time.deltaTime * rotationSpeed);

            if (pc.MoveType.Equals(MovementType.Air))
            {

            }
        }
    }
}
