using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text domeEnergy;
    [SerializeField] private Text nDeaths;
    [SerializeField] private Text nEnemies;
    [SerializeField] private Text nEvacuees;

    [SerializeField] private Text mainMessage;

    [SerializeField] private Button continueButton;

    public ResultsData results;

    private string victoryMsg = "City Saved!";
    private string defeatMsg = "City Doomed...";

    [SerializeField] private List<GameObject> playerSkins = new List<GameObject>();
    private Animator _animator;
    
    //Database etc
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        InitSkin();
        
        _animator.SetBool("Victory", results.victory);
        
        SetText(mainMessage, results.victory ? victoryMsg : defeatMsg);

        SetText(domeEnergy, results.domeEnergy.ToString() +" %");
        SetText(nDeaths, results.nDeaths.ToString());
        SetText(nEnemies, results.enemiesKilled.ToString());
        SetText(nEvacuees, results.evacuees.ToString() + " %");
        
        continueButton.onClick.AddListener(BackToMainMenu);
    }

    private void SetText(Text msg, string s)
    {
        msg.text = s;
        msg.gameObject.transform.GetChild(0).GetComponent<Text>().text = s;
    }

    public void BackToMainMenu()
    {
        //Disconnect and go back to menu scene
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            SceneManager.LoadScene("Menu");
    }
    
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
        base.OnLeftRoom();
    }
    
    public void InitSkin()
    {
        //Select current skin
        if (playerSkins.Count != 0)
        {
            int id = Math.Min(PlayerPrefs.GetInt("Skin", 0), playerSkins.Count - 1);
            var selectedSkin = playerSkins[id];
            playerSkins[id].SetActive(true);
            Debug.Log("skin loaded:  "+ id);
            
            _animator = selectedSkin.GetComponent<Animator>();
            
            Color colorSkin = PlayerPrefsX.GetColor("SkinColor");

            if (!(colorSkin.r >= 0.0f)) return;
            
            selectedSkin.GetComponent<MaterialChanger>().SetMaterialColor(colorSkin);
            Debug.Log("color loaded:  "+ colorSkin);
        }
    }
}
