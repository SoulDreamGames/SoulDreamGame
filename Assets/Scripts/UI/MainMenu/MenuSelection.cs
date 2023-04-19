using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuSelection : MonoBehaviour
{
    [Serializable]
    public enum MenuType
    {
        StartGame = 0,
        OutfitMenu = 1,
        BattlePassMenu = 2,
        ShopMenu = 3,
        SettingsMenu = 4
    }
    [SerializeField] private List<Button> menuButtons;
    [SerializeField] private List<GameObject> menuCanvas;

    private MenuType _currentMenu = MenuType.StartGame;
    [SerializeField] private OutfitMenu outfitMenu;
    [SerializeField] private SettingsMenu settingsMenu;

    private void Start()
    {
        //Init visual player
        outfitMenu.InitSkin();
        settingsMenu.OnStartMenu();
        
        //Assign callbacks to menus
        for (int i = 0; i < Enum.GetNames(typeof(MenuType)).Length; i++)
        {
            var currentType = i;
            menuButtons[i].onClick.AddListener(() =>
            {
                OnButtonPressed((MenuType)currentType);
            });
        }
        
        //Init with default canvas
        OnButtonPressed(MenuType.StartGame);
        menuButtons[(int)_currentMenu].Select();
    }

    public void OnButtonPressed(MenuType menuType)
    {
        if (_currentMenu == MenuType.SettingsMenu)
        {
            PlayerPrefs.Save();
        }
        menuCanvas[(int)_currentMenu].SetActive(false);
        menuCanvas[(int)menuType].SetActive(true);
        _currentMenu = menuType;
    }
}
