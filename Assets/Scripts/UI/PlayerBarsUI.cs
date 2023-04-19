using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBarsUI : MonoBehaviour
{
    public PlayerController Player;
    private GameManager _gameManager;
    
    //Speed bar
    public Image SpeedImage;
    private float _barSpeed;
    
    //Energy bar
    public Image EnergyImage;
    private float _barEnergy;
    
    //Dome Energy bar
    public Image DomeImage;
    private float _barDome;
    
    //City energy bar
    public Image CityEnergyImage;
    private float _barCity;
    
    //Round Text
    [SerializeField] private Text currentWaveText;
    [SerializeField] private Text shadowWaveText;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        SpeedImage.fillAmount = 0.0f;
        EnergyImage.fillAmount = 0.0f;
        DomeImage.fillAmount = 0.0f;
    }

    public void UpdateUIBars()
    {
        UpdateHealthBar();
        UpdateEnergyBar();
        
        if (!_gameManager) return;
        UpdateDomeBar();
        UpdateCityEnergyBar();
        UpdateCurrentWave();
    }

    private void UpdateHealthBar()
    {
        //Debug.Log("Update with value: " + Player.MoveSpeed / Player.MaxMoveSpeed);
        float newSpeed = Mathf.Clamp(Player.MoveSpeed / Player.MaxMoveSpeed, 0, 1f);
        SpeedImage.fillAmount = Mathf.SmoothDamp(SpeedImage.fillAmount, newSpeed, ref _barSpeed, 0.1f);
    }

    private void UpdateEnergyBar()
    {
        float newEnergy = Mathf.Clamp(Player.PlayerEnergy / Player.MaxEnergy, 0, 1f);
        EnergyImage.fillAmount = Mathf.SmoothDamp(EnergyImage.fillAmount, newEnergy, ref _barEnergy, 0.1f);
    }
    
    private void UpdateDomeBar()
    {
        if (!_gameManager) return;
        
        float newEnergy = Mathf.Clamp(_gameManager.domeEnergy / _gameManager.maxDomeEnergy, 0, 1f);
        DomeImage.fillAmount = Mathf.SmoothDamp(DomeImage.fillAmount, newEnergy, ref _barDome, 0.1f);
    }
    
    private void UpdateCityEnergyBar()
    {
        float newEnergy = Mathf.Clamp(_gameManager.cityEnergy / 100.0f, 0, 1f);
        CityEnergyImage.fillAmount = Mathf.SmoothDamp(CityEnergyImage.fillAmount, newEnergy, ref _barCity, 0.1f);
    }

    private void UpdateCurrentWave()
    {
        currentWaveText.text = _gameManager.currentWave + "/" +  _gameManager.totalWaves;
        shadowWaveText.text = _gameManager.currentWave + "/" +  _gameManager.totalWaves;
    }
}