using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class InGameSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    
    //UI Settings components
    [SerializeField] private Slider audioSlider;
    [SerializeField] private Dropdown graphicsDrop;
    [SerializeField] private Dropdown resolutionDrop;
    [SerializeField] private Toggle windowedToggle;
    [SerializeField] private Toggle vSyncToggle;
    
    //ToDo: Leave PUN and apply changes
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button applySettings;
    
    [SerializeField] private RectTransform volumeRect;
    private Resolution[] _resolutions;
    
    //Values for graphics settings
    private int _qualityLevel;
    private int _resolutionLevel;
    private bool _isWindowed;
    private bool _isSynced;
    
    //Main menu component
    [SerializeField] private InGameMenu inGameMenu;

    private void Awake()
    {
        //Set graphics defaults
        audioSlider.value = PlayerPrefs.GetFloat("Volume", 0.0f);
        float scale = 1.0f + (audioSlider.value - audioSlider.minValue) / (audioSlider.maxValue - audioSlider.minValue);
        volumeRect.localScale = new Vector3(scale, scale, scale);

        _qualityLevel = PlayerPrefs.GetInt("Graphics", 4);
        graphicsDrop.value = _qualityLevel;

        _isWindowed = PlayerPrefs.GetInt("Windowed", 0) == 1;
        windowedToggle.isOn = _isWindowed;

        _isSynced = PlayerPrefs.GetInt("VSyncs", 0) != 0;
        vSyncToggle.isOn = _isSynced;
    }

    private void Start()
    {
        _resolutions = Screen.resolutions;
        resolutionDrop.ClearOptions();

        List<string> resOptions = new List<string>();
        _resolutionLevel = 0; 
        for (int i = 0; i < _resolutions.Length; i++)
        {
            resOptions.Add(_resolutions[i].width + " x " + _resolutions[i].height);

            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
                _resolutionLevel = i;
        }
        resolutionDrop.AddOptions(resOptions);
        resolutionDrop.value = _resolutionLevel;
        resolutionDrop.RefreshShownValue();

        resolutionDrop.onValueChanged.AddListener(SetResolution);
        audioSlider.onValueChanged.AddListener(SetVolume);
        graphicsDrop.onValueChanged.AddListener(SetGraphicsQuality);
        windowedToggle.onValueChanged.AddListener(SetWindowed);
        vSyncToggle.onValueChanged.AddListener(SetVerticalSync);
        
        //Buttons onClick event
        applySettings.onClick.AddListener(ApplySettings);
        leaveButton.onClick.AddListener(LeaveGame);
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
        _qualityLevel = quality;
    }

    void SetWindowed(bool isWindowed)
    {
        _isWindowed = isWindowed;
    }

    void SetResolution(int resolution)
    {
        _resolutionLevel = resolution;
    }

    void SetVerticalSync(bool isSynced)
    {
        _isSynced = isSynced;
    }

    void ApplySettings()
    {
        //Set quality
        QualitySettings.SetQualityLevel(_qualityLevel);
        PlayerPrefs.SetInt("Graphics", _qualityLevel);
        
        //Windowed
        Screen.fullScreen = !_isWindowed;
        PlayerPrefs.SetInt("Windowed", _isWindowed ? 1 : 0);
        
        //Resolution
        Screen.SetResolution(_resolutions[_resolutionLevel].width, _resolutions[_resolutionLevel].height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", _resolutionLevel);
        
        //Sync
        QualitySettings.vSyncCount = _isSynced ? 1 : 0;
        PlayerPrefs.SetInt("VSync", _isSynced ? 1 : 0);
        
        inGameMenu.SwitchMenu();
    }

    void LeaveGame()
    {
        //Disconnect and go back to menu scene
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }
}
