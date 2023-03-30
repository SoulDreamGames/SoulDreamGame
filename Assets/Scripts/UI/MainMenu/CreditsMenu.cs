using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private GameObject headerMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private Button socialMediaButton;

    private void Start()
    {
        backButton.onClick.AddListener(BackToSettings);
        socialMediaButton.onClick.AddListener(ShowSocialMedia);
    }

    void ShowSocialMedia()
    {
        Application.OpenURL("https://linktr.ee/souldreamgames");
    }
    void BackToSettings()
    {
        headerMenu.SetActive(true);
        settingsMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
