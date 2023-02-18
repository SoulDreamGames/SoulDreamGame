using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Reference Objects")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public PlayerController pc;
    
    public float rotationSpeed;
    
    [HideInInspector]

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void Update(){

        // Rotate orientation
        Vector3 lookAt = player.position -
                         new Vector3(transform.position.x, player.position.y, transform.position.z);    //ToDo: with transform Y we get the desired direction for flying
        orientation.forward = lookAt.normalized;                                                        //Check camera switching and limited fovs with cinemachine

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
