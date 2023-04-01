using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    
    //UI Settings components
    [SerializeField] private Slider audioSlider;
    [SerializeField] private Dropdown graphicsDrop;
    [SerializeField] private Dropdown resolutionDrop;
    [SerializeField] private Toggle windowedToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Button quitGame;

    [SerializeField] private RectTransform volumeRect;
    private Resolution[] _resolutions;

    [SerializeField] private Button ourStudioButton;
    [SerializeField] private GameObject creditsObject;
    [SerializeField] private GameObject headerButtons;

    private void Awake()
    {
        //Set graphics defaults
        audioSlider.value = PlayerPrefs.GetFloat("Volume", 0.0f);
        float scale = 1.0f + (audioSlider.value - audioSlider.minValue) / (audioSlider.maxValue - audioSlider.minValue);
        volumeRect.localScale = new Vector3(scale, scale, scale);
        
        graphicsDrop.value = PlayerPrefs.GetInt("Graphics", 4);
        windowedToggle.isOn = PlayerPrefs.GetInt("Windowed", 0) == 1;
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSyncs", 0) != 0;
    }

    private void Start()
    {
        _resolutions = Screen.resolutions;
        resolutionDrop.ClearOptions();

        List<string> resOptions = new List<string>();
        int currentResolutionIndex = 0; 
        for (int i = 0; i < _resolutions.Length; i++)
        {
            resOptions.Add(_resolutions[i].width + " x " + _resolutions[i].height);

            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDrop.AddOptions(resOptions);
        resolutionDrop.value = currentResolutionIndex;
        resolutionDrop.RefreshShownValue();

        resolutionDrop.onValueChanged.AddListener(SetResolution);
        audioSlider.onValueChanged.AddListener(SetVolume);
        graphicsDrop.onValueChanged.AddListener(SetGraphicsQuality);
        windowedToggle.onValueChanged.AddListener(SetWindowed);
        vSyncToggle.onValueChanged.AddListener(SetVerticalSync);
        
        quitGame.onClick.AddListener(QuitGame);
        
        ourStudioButton.onClick.AddListener(ShowCredits);
    }

    void SetVolume(float volume)
    {
        //ToDo: Change this to logarithmic
        float scale = 1.0f + (audioSlider.value - audioSlider.minValue) / (audioSlider.maxValue - audioSlider.minValue);
        volumeRect.localScale = new Vector3(scale, scale, scale);
        masterMixer.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("Volume", volume);
    }

    void SetGraphicsQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
        PlayerPrefs.SetInt("Graphics", quality);
    }

    void SetWindowed(bool isWindowed)
    {
        Screen.fullScreen = !isWindowed;
        PlayerPrefs.SetInt("Windowed", isWindowed ? 1 : 0);
        Debug.Log("FS " +  Screen.fullScreen);
    }

    void SetResolution(int resolution)
    {
        Screen.SetResolution(_resolutions[resolution].width, _resolutions[resolution].height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolution);
        Debug.Log("Resolution " +  Screen.currentResolution);
    }

    void SetVerticalSync(bool isSynced)
    {
        QualitySettings.vSyncCount = isSynced ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isSynced ? 1 : 0);
        Debug.Log("Vsync " +  QualitySettings.vSyncCount);
    }
    
    void ShowCredits()
    {
        //Show credits
        creditsObject.SetActive(true);
        headerButtons.SetActive(false);
        gameObject.SetActive(false);
        
    }
    
    void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
