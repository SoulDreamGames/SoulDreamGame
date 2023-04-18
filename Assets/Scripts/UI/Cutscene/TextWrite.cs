using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TextWrite : MonoBehaviour
{
    private Text _text;
    private string _fullText;
    private float _charTime = 0.075f;

    [SerializeField] private AudioManager audioManager;
    private Coroutine _writeCoroutine;

    private bool _isWritting = false;
    private void Awake()
    {
        _text = GetComponent<Text>();
        _fullText = _text.text;
        _text.text = "";
    }

    private void OnEnable()
    {
        _isWritting = true;
        _writeCoroutine = StartCoroutine(WriteString(_charTime));
    }

    IEnumerator WriteString(float timeSpacing)
    {
        foreach (var character in _fullText)
        {
            if(audioManager)
                audioManager.PlayAudioButtonWithVolume("Typewritter", 0.5f);
            
            _text.text += character;   
            yield return new WaitForSeconds(timeSpacing);
        }

        _isWritting = false;
    }

    public void StopWritting()
    {
        if (!_isWritting) return;
        
        StopCoroutine(_writeCoroutine);
        _text.text = _fullText;

        _isWritting = false;
    }

    public bool IsWritting()
    {
        return _isWritting;
    }
}
