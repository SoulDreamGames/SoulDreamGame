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
    private float _charTime = 0.1f;

    private void Awake()
    {
        _text = GetComponent<Text>();
        _fullText = _text.text;
        _text.text = "";
    }

    private void OnEnable()
    {
        StartCoroutine(WriteString(_charTime));
    }

    IEnumerator WriteString(float timeSpacing)
    {
        foreach (var character in _fullText)
        {
            _text.text += character;   
            yield return new WaitForSeconds(timeSpacing);
        }
    }
}
