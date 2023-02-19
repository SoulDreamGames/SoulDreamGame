using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedBar : MonoBehaviour
{
    public Image SpeedImage;
    public PlayerController player;

    private float _barSpeed;

    private void Start()
    {
        SpeedImage.fillAmount = 0.0f;
    }

    public void UpdateHealthBar()
    {
        float newSpeed = Mathf.Clamp(player.moveSpeed / player.MaxMoveSpeed, 0, 1f);
        SpeedImage.fillAmount = Mathf.SmoothDamp(SpeedImage.fillAmount, newSpeed, ref _barSpeed, 0.1f);
    }
}