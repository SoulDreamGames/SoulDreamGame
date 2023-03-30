using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedBar : MonoBehaviour
{
    public Image SpeedImage;
    public PlayerController Player;

    private float _barSpeed;

    private void Start()
    {
        SpeedImage.fillAmount = 0.0f;
    }

    public void UpdateHealthBar()
    {
        Debug.Log("Update with value: " + player.moveSpeed / player.MaxMoveSpeed);
        float newSpeed = Mathf.Clamp(Player.MoveSpeed / Player.MaxMoveSpeed, 0, 1f);
        SpeedImage.fillAmount = Mathf.SmoothDamp(SpeedImage.fillAmount, newSpeed, ref _barSpeed, 0.1f);
    }
}