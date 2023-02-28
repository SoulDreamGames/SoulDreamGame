using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;

    private Vector3 spawnPosition;
    public List<CinemachineFreeLook> cameras;
    public ThirdPersonCam cam;
    private GameObject player;
    void Awake()
    {
        spawnPosition = transform.position;
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
    }

    private void Start()
    {
        foreach (var followCamera in cameras)
        {
            followCamera.Follow = player.transform;
            followCamera.LookAt = player.transform;
        }

        //ToDo: improve this using a better system
        int n = transform.childCount;
        cam.orientation = player.transform.GetChild(1);
        cam.pc = player.GetComponent<PlayerController>();
        cam.player = player.transform;
        cam.playerObj = player.transform.GetChild(0);
    }
}
