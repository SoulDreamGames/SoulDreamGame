using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    private Image _image;
    private Text _text;

    private Color _fadeColor;
    private float alpha = 0.0f;
    void Start(){
        _image = GetComponent<Image>();
        _text = GetComponent<Text>();

        if (_image)
        {
            _fadeColor = new Color(_image.color.r, _image.color.g, _image.color.b);
            _fadeColor.a = alpha;
            _image.color = _fadeColor;
        }

        if (_text)
        {
            _fadeColor = new Color(_text.color.r, _text.color.g, _text.color.b);
            _fadeColor.a = alpha;
            _text.color = _fadeColor;
        }
        
        StartCoroutine(FadeInImage(0.05f));
    }

    public void InitFadeOut()
    {
        StartCoroutine(FadeOutImage(0.05f));
    }
    
    IEnumerator FadeInImage(float time)
    {
        while (alpha < 1.0f)
        {
            yield return new WaitForSeconds(time);
            alpha += time;
            _fadeColor.a = alpha;
            
            if(_image)
                _image.color = _fadeColor;
            
            if(_text)
                _text.color = _fadeColor;
        }
    }
    
    IEnumerator FadeOutImage(float time)
    {
        while (alpha > 0.0f)
        {
            yield return new WaitForSeconds(time);
            
            alpha -= time;
            _fadeColor.a = alpha;
            
            if(_image)
                _image.color = _fadeColor;
            
            if(_text)
                _text.color = _fadeColor;
        }
    }
}
