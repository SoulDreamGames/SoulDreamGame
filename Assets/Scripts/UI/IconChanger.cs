using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconChanger : MonoBehaviour
{
    [SerializeField] private List<GameObject> skinIcons = new List<GameObject>();
    void Awake()
    {
        foreach (var go in skinIcons)
        {
            go.SetActive(false);
        }
        
        int selectedSkin = Math.Min(PlayerPrefs.GetInt("Skin", 0), skinIcons.Count -1);
        skinIcons[selectedSkin].SetActive(true);
    }
}
