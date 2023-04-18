using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSequence : MonoBehaviour, MoveInput.IInitSceneActions
{
    [SerializeField] private List<GameObject> imagesScenes = new List<GameObject>();
    [SerializeField] private List<GameObject> textScenes = new List<GameObject>();

    private int _currentImage = 0;
    [SerializeField] private Button nextImage;
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject keyboardButtons;

    private bool _sceneInit = false;

    private MoveInput _uiInput;
    private AudioManager _audioManager;

    void Awake()
    {
        _uiInput = new MoveInput();
        _audioManager = GetComponent<AudioManager>();
        nextImage.onClick.AddListener(OnNextImage);

        _uiInput.InitScene.SkipScene.performed += OnSkipScene;
    }

    public void InitScene()
    {
        imagesScenes[0].SetActive(true);
        textScenes[0].SetActive(true);
        keyboardButtons.SetActive(true);
        
        _audioManager.PlayAudioLoop("Intro");
        _sceneInit = true;
    }
    
    public void OnNextImage()
    {
        if (!_sceneInit) return;
        ShowNextImage(_currentImage);
        _currentImage++;
    }

    private void ShowNextImage(int id)
    {
        imagesScenes[id].SetActive(false);
        textScenes[id].SetActive(false);

        if (id + 1 >= imagesScenes.Count)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        
        imagesScenes[id + 1].SetActive(true);
        textScenes[id + 1].SetActive(true);
    }
    
    public void OnSkipScene(InputAction.CallbackContext context)
    {
        Debug.Log("On intro");
        if (!_sceneInit) return;
        
        SkipSequence();
    }
    
    private void SkipSequence()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    
    private void OnEnable() => _uiInput.Enable();
    private void OnDisable() => _uiInput.Disable();
}
