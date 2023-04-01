using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    public List<GameObject> playerPrefab = new List<GameObject>();

    private Vector3 spawnPosition;
    public List<CinemachineFreeLook> cameras;
    public ThirdPersonCam cam;
    private GameObject _player;

    [SerializeField] private MiniMap minimap;
    void Awake()
    {
        spawnPosition = transform.position;
        
        //Get selected skin and instantiate matching prefab
        int selectedSkin = Math.Min(PlayerPrefs.GetInt("Skin", 0), playerPrefab.Count -1);
        GameObject selectedPrefab = playerPrefab[selectedSkin];
        _player = PhotonNetwork.Instantiate(selectedPrefab.name, spawnPosition, Quaternion.identity);
        Debug.Log("Skin was: " + selectedSkin);
    }

    private void Start()
    {
        foreach (var followCamera in cameras)
        {
            followCamera.Follow = _player.transform;
            followCamera.LookAt = _player.transform;
        }

        //ToDo: improve this using a better system
        int n = transform.childCount;
        cam.orientation = _player.transform.GetChild(1);
        cam.pc = _player.GetComponent<PlayerController>();
        cam.player = _player.transform;
        cam.playerObj = _player.transform.GetChild(0);

        minimap.playerPosition = _player.transform.GetChild(0);

    }
}
