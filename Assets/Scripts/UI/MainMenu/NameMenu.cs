using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NameMenu : MonoBehaviour
{
    [SerializeField] private InputField nameInput;
    [SerializeField] private Button confirmName;

    [SerializeField] private GameObject mainMenu;
    void Start()
    {
        confirmName.onClick.AddListener(ConfirmName);
        string username = PlayerPrefs.GetString("username");
        Debug.Log(username);
        
        if (username.Equals("")) return;

        SetPhotonNickname(username);
        
        //Ignore this menu if name is already set
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    public void ConfirmName()
    {
        confirmName.interactable = false;
        string username = nameInput.text;
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
        SetPhotonNickname(username);
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    private void SetPhotonNickname(string username)
    {
        if(PhotonNetwork.IsConnected)
            PhotonNetwork.NickName = username;
        else
            Debug.Log("Unconnected");
        
        Debug.Log("Pun nick: " + PhotonNetwork.NickName);
    }
}
