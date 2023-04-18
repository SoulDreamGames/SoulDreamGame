using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class InGameSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private AudioMixer sfxMixer;
    
    //UI Settings components
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Dropdown graphicsDrop;
    [SerializeField] private Dropdown resolutionDrop;
    [SerializeField] private Toggle windowedToggle;
    [SerializeField] private Toggle vSyncToggle;
    
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button applySettings;
    
    [SerializeField] private RectTransform musicRect;
    [SerializeField] private RectTransform sfxRect;
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
        musicSlider.value = PlayerPrefs.GetFloat("Volume", 0.0f);
        float scale = 1.0f + (musicSlider.value - musicSlider.minValue) / (musicSlider.maxValue - musicSlider.minValue);
        musicRect.localScale = new Vector3(scale, scale, scale);
        
        //SFX audio
        sfxSlider.value = PlayerPrefs.GetFloat("VolumeSFX", 0.0f);
        scale = 1.0f + (sfxSlider.value - sfxSlider.minValue) / (sfxSlider.maxValue - sfxSlider.minValue);
        sfxRect.localScale = new Vector3(scale, scale, scale);

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
            resOptions.Add(_resolutions[i].width + " x " + _resolutions[i].height + " " 
                           + _resolutions[i].refreshRate+"Hz");

            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
                _resolutionLevel = i;
        }
        resolutionDrop.AddOptions(resOptions);
        resolutionDrop.value = _resolutionLevel;
        resolutionDrop.RefreshShownValue();

        resolutionDrop.onValueChanged.AddListener(SetResolution);
        musicSlider.onValueChanged.AddListener(SetVolumeMusic);
        sfxSlider.onValueChanged.AddListener(SetVolumeSFX);

        graphicsDrop.onValueChanged.AddListener(SetGraphicsQuality);
        windowedToggle.onValueChanged.AddListener(SetWindowed);
        vSyncToggle.onValueChanged.AddListener(SetVerticalSync);
        
        //Buttons onClick event
        applySettings.onClick.AddListener(ApplySettings);
        leaveButton.onClick.AddListener(LeaveGame);
    }

    void SetVolumeMusic(float volume)
    {
        float scale = 1.0f + (musicSlider.value - musicSlider.minValue) / (musicSlider.maxValue - musicSlider.minValue);
        musicRect.localScale = new Vector3(scale, scale, scale);
        
        float logVolume = 20 * Mathf.Log10(volume);
        musicMixer.SetFloat("Volume", logVolume);
        PlayerPrefs.SetFloat("Volume", volume);
    }
    
    void SetVolumeSFX(float volume)
    {
        float scale = 1.0f + (sfxSlider.value - sfxSlider.minValue) / (sfxSlider.maxValue - sfxSlider.minValue);
        sfxRect.localScale = new Vector3(scale, scale, scale);
        
        float logVolume = 20 * Mathf.Log10(volume);
        sfxMixer.SetFloat("Volume", logVolume);
        PlayerPrefs.SetFloat("VolumeSFX", volume);
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
        
        //Resolution
        Screen.SetResolution(_resolutions[_resolutionLevel].width, _resolutions[_resolutionLevel].height, !_isWindowed);
        PlayerPrefs.SetInt("Resolution", _resolutionLevel);

        //Windowed
        Screen.fullScreenMode = _isWindowed ? FullScreenMode.Windowed : FullScreenMode.ExclusiveFullScreen;
        PlayerPrefs.SetInt("Windowed", _isWindowed ? 1 : 0);

        //Sync
        QualitySettings.vSyncCount = _isSynced ? 1 : 0;
        PlayerPrefs.SetInt("VSync", _isSynced ? 1 : 0);

        inGameMenu.SwitchMenu();
    }

    void LeaveGame()
    {
        //Disconnect and go back to menu scene
        PhotonNetwork.LeaveRoom();
    }
}
