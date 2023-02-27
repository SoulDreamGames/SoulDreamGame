using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField roomInput;
    public string mainSceneName = "Sandbox";

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomInput.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(mainSceneName);
    }
}
