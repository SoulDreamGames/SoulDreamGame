using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class CreateAndJoinRoomsMenu : MonoBehaviourPunCallbacks
{
    public InputField roomInput;
    public string mainSceneName = "Lobby";

    [SerializeField] private Button createRoom;
    [SerializeField] private Button joinRoom;

    [SerializeField] private Text usernameText;
    
    void Awake()
    {
        //Button listeners
        createRoom.onClick.AddListener(CreateRoom);
        joinRoom.onClick.AddListener(JoinRoom);
    }

    void Start()
    {
        //Set username on screen
        usernameText.text = PlayerPrefs.GetString("username");
    }

    public void CreateRoom()
    {
        createRoom.interactable = false;
        joinRoom.interactable = false;
        PhotonNetwork.CreateRoom(roomInput.text);
    }

    public void JoinRoom()
    {
        if (PhotonNetwork.JoinRoom(roomInput.text))
        {
            joinRoom.interactable = false;
            createRoom.interactable = false;
        }
        
    }

    public override void OnJoinedRoom()
    {
        //SceneManager.LoadScene(mainSceneName);
        PhotonNetwork.LoadLevel(mainSceneName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Fail ");
        base.OnCreateRoomFailed(returnCode, message);
        joinRoom.interactable = true;
        createRoom.interactable = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Fail ");
        base.OnJoinRoomFailed(returnCode, message);
        joinRoom.interactable = true;
        createRoom.interactable = true;
    }
}
