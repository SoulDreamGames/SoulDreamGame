using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] private LoadingBar loadingBar;

    void Start()
    {
        loadingBar.UpdatePercentage(0.25f);
        PhotonNetwork.ConnectUsingSettings();
        loadingBar.UpdatePercentage(0.5f);
    }

    public override void OnConnectedToMaster()
    {
        loadingBar.UpdatePercentage(0.75f);
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        loadingBar.UpdatePercentage(1.0f);
        SceneManager.LoadScene("Menu");
    }
}