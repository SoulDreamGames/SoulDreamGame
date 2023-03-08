using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public Image loadImage;
    private float _barSpeed;
    
    private void Start()
    {
        loadImage.fillAmount = 0.0f;
    }

    public void UpdatePercentage(float percentage)
    {
        loadImage.fillAmount = Mathf.SmoothDamp(loadImage.fillAmount, percentage, ref _barSpeed, 0.01f);
    }
}
