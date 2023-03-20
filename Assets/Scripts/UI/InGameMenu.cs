using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameMenu : MonoBehaviour, MoveInput.IUIActions
{
    //Components
    private MoveInput _uiInput;

    [SerializeField] private List<GameObject> uiCanvas;     //0 is ingame - 1 is settings
    private int _currentUI = 0;
    
    void Awake()
    {
        _uiInput = new MoveInput();
        _uiInput.UI.OpenMenu.performed += OnOpenMenu;
        
        //Set active
        uiCanvas[0].SetActive(true);
        uiCanvas[1].SetActive(false);
    }

    public void OnOpenMenu(InputAction.CallbackContext context)
    {
        SwitchMenu();
    }

    public void SwitchMenu()
    {
        //Reverse showed ui
        uiCanvas[_currentUI].SetActive(false);
        _currentUI = _currentUI == 0 ? 1 : 0;
        uiCanvas[_currentUI].SetActive(true);

        //Check mouse lock mode
        if (_currentUI == 1)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void OnEnable()
    {
        _uiInput.Enable();
    }

    private void OnDisable()
    {
        _uiInput.Disable();
    }

}
