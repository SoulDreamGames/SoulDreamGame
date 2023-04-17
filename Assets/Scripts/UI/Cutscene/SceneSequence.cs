using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneSequence : MonoBehaviour
{
    [SerializeField] private List<GameObject> imagesScenes = new List<GameObject>();
    [SerializeField] private List<GameObject> textScenes = new List<GameObject>();

    private int _currentImage = 0;
    [SerializeField] private Button nextImage;

    void Start()
    {
        nextImage.onClick.AddListener(OnNextImage);
        
        imagesScenes[0].SetActive(true);
        textScenes[0].SetActive(true);
    }
    
    public void OnNextImage()
    {
        ShowNextImage(_currentImage);
        _currentImage++;
    }

    private void ShowNextImage(int id)
    {
        imagesScenes[id].SetActive(false);
        textScenes[id].SetActive(false);
        
        imagesScenes[id + 1].SetActive(true);
        textScenes[id + 1].SetActive(true);
    }
}
