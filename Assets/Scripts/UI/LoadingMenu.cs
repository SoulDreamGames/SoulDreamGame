using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    public Image loadImage;
    private float _barSpeed;

    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private AudioMixer sfxMixer;
    private void Start()
    {
        //Set saved settings
        //Volume
        float volume = PlayerPrefs.GetFloat("Volume", -100f);
        if (volume > -100.0f)
        {
            float logVolume = 20 * Mathf.Log10(volume);
            musicMixer.SetFloat("Volume", logVolume);
        }
        
        volume = PlayerPrefs.GetFloat("VolumeSFX", -100f);
        if (volume > -100.0f)
        {
            float logVolume = 20 * Mathf.Log10(volume);
            sfxMixer.SetFloat("Volume", logVolume);
        }
        
        //Graphics level
        int graphics = PlayerPrefs.GetInt("Graphics", -1);
        if (graphics != -1)
        {
            QualitySettings.SetQualityLevel(graphics);
        }
        
        //Windowed
        int windowed = PlayerPrefs.GetInt("Windowed", 0);
        Screen.fullScreenMode = windowed == 1 ? FullScreenMode.Windowed : FullScreenMode.ExclusiveFullScreen;

        //Resolution
        int resolution = PlayerPrefs.GetInt("Resolution", -1);
        if (resolution != -1)
        {
            Resolution[] resolutions;
            resolutions = Screen.resolutions;
            resolution = Math.Min(resolution, resolutions.Length - 1);

            Screen.SetResolution(resolutions[resolution].width, resolutions[resolution].height, Screen.fullScreen);
        }
        
        //VSync
        int vsync = PlayerPrefs.GetInt("VSync", 0);
        QualitySettings.vSyncCount = vsync;
        
        
        loadImage.fillAmount = 0.0f;
    }

    public void UpdatePercentage(float percentage)
    {
        loadImage.fillAmount = Mathf.SmoothDamp(loadImage.fillAmount, percentage, ref _barSpeed, 0.01f);
    }
}
