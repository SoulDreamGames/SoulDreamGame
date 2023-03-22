using System.Collections;
using System.Collections.Generic;
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
        
        //Ignore this menu if name is already set
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    public void ConfirmName()
    {
        confirmName.interactable = false;
        PlayerPrefs.SetString("username", nameInput.text);
        PlayerPrefs.Save();
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
