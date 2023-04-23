using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitConfig : MonoBehaviour
{
    void Start()
    {
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
    }
}
