using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;

public class OutfitMenu : MonoBehaviour
{

    [SerializeField] private List<GameObject> skinObjects = new List<GameObject>();
    [SerializeField] private List<Button> skinButtons = new List<Button>();
    [SerializeField] private List<Button> trailButtons = new List<Button>();
    [SerializeField] private GameObject colorsParent;

    private List<Color> _colors;
    private GameObject selectedSkin;

    public void InitSkin()
    {
        //Select current skin
        if (skinObjects.Count != 0)
        {
            int id = Math.Min(PlayerPrefs.GetInt("Skin", 0), skinObjects.Count - 1);
            selectedSkin = skinObjects[id];
            Debug.Log("skin loaded:  "+ id);
            
            Color colorSkin = PlayerPrefsX.GetColor("SkinColor");
            
            if (!(colorSkin.r >= 0.0f)) return;
            
            selectedSkin.transform.GetChild(1).
                GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_EmissionColor", colorSkin);
            Debug.Log("color loaded:  "+ colorSkin);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set colors list based on UI
        _colors = new List<Color>();
        for (int i = 0; i < colorsParent.transform.childCount; i++)
        {
            //Add each image color to list
            Button b = colorsParent.transform.GetChild(i).GetComponent<Button>();
            Color c = colorsParent.transform.GetChild(i).GetComponent<Image>().color;
            b.onClick.AddListener(() => OnClickColor(c));

            _colors.Add(c);
        }

        //Skins
        foreach (Button b in skinButtons)
        {
            int i = skinButtons.IndexOf(b);
            b.onClick.AddListener(()=>OnClickSkin(i));

            //if list not set correctly, make buttons non interactable
            if (skinObjects.Count == 0) b.interactable = false;
        }

        //Trails
        foreach (Button b in trailButtons)
        {
            int i = trailButtons.IndexOf(b);
            b.onClick.AddListener(()=>OnClickTrail(i));
        }

    }

    void OnClickColor(Color color)
    {
        selectedSkin.transform.GetChild(1).
            GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_EmissionColor", color);
        //Set to player prefs with PlayerPrefsX
        PlayerPrefsX.SetColor("SkinColor", color);
        Debug.Log("Color saved " + color);
    }

    void OnClickSkin(int selected)
    {
        int id_selected = Math.Min(selected, skinObjects.Count - 1);
        
        if (selectedSkin == skinObjects[id_selected]) return;
        
        selectedSkin.SetActive(false);
        selectedSkin = skinObjects[id_selected];
        selectedSkin.SetActive(true);
        
        PlayerPrefs.SetInt("Skin", id_selected);
        Debug.Log("Selected trail: " + id_selected);
    }

    void OnClickTrail(int selected)
    {
        PlayerPrefs.SetInt("Trail", selected);
        Debug.Log("Selected trail: " + selected);
    }
}
