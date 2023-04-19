using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulLogoTimer : MonoBehaviour
{
    [SerializeField] private float logoTimer = 5.0f;
    [SerializeField] private SceneSequence sceneSequence;
    [SerializeField] private List<FadeIn> fadingObjects;
    private void Start()
    {
        Invoke(nameof(StartScene), logoTimer);
    }

    private void StartScene()
    {
        foreach (var fade in fadingObjects)
        {
            fade.InitFadeOut();
        }

        Invoke(nameof(InitScene), 1.0f);
    }

    private void InitScene()
    {
        sceneSequence.InitScene();
        gameObject.SetActive(false);
    }
}