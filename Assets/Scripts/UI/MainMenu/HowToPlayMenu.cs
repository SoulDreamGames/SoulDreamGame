using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayMenu : MonoBehaviour
{
    [SerializeField] private GameObject headerMenu;
    [SerializeField] private GameObject gameMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private List<GameObject> tutorialPages = new List<GameObject>();
    private int _currentPage = 0;

    private void Start()
    {
        backButton.onClick.AddListener(BackToSettings);
        leftButton.onClick.AddListener(ShowLeftPage);
        rightButton.onClick.AddListener(ShowRightPage);
    }

    void ShowRightPage()
    {
        tutorialPages[_currentPage].SetActive(false);
        
        _currentPage++;
        if (_currentPage > tutorialPages.Count - 1)
        {
            _currentPage = 0;
        }
        
        tutorialPages[_currentPage].SetActive(true);

    }

    void ShowLeftPage()
    {
        tutorialPages[_currentPage].SetActive(false);
        
        _currentPage--;
        if (_currentPage < 0)
        {
            _currentPage = tutorialPages.Count - 1;
        }
        
        tutorialPages[_currentPage].SetActive(true);

    }
    
    void BackToSettings()
    {
        headerMenu.SetActive(true);
        gameMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
