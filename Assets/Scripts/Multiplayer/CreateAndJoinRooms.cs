using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField roomInput;
    public string mainSceneName = "Lobby";

    [SerializeField] private Button createRoom;
    [SerializeField] private Button joinRoom;
    
    void Awake()
    {
        //Button listeners
        createRoom.onClick.AddListener(CreateRoom);
        joinRoom.onClick.AddListener(JoinRoom);
    }

    public void CreateRoom()
    {
        createRoom.interactable = false;
        joinRoom.interactable = false;
        PhotonNetwork.CreateRoom(roomInput.text);
    }

    public void JoinRoom()
    {
        joinRoom.interactable = false;
        createRoom.interactable = false;
        PhotonNetwork.JoinRoom(roomInput.text);
    }

    public override void OnJoinedRoom()
    {
        //SceneManager.LoadScene(mainSceneName);
        PhotonNetwork.LoadLevel(mainSceneName);
    }
}
