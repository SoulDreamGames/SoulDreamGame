using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] private LoadingMenu loadingMenu;

    void Start()
    {
        //Tiny wait time
        StartCoroutine(Wait(2.0f));
        
        loadingMenu.UpdatePercentage(0.25f);
        PhotonNetwork.ConnectUsingSettings();
        loadingMenu.UpdatePercentage(0.5f);
    }

    public override void OnConnectedToMaster()
    {
        loadingMenu.UpdatePercentage(0.75f);
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        loadingMenu.UpdatePercentage(1.0f);
        SceneManager.LoadScene("Menu");
    }

    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}