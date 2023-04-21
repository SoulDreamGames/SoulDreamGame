using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using GameEventType = GameManager.GameEventType;
using GameState = GameManager.GameState;


public class TimeManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // this time only matters when staying at OnWave state
    //ToDo: if every enemy on this wave has dead, change this state to WaittingWave and reset time to that

    [Serializable]
    private struct InitStruct
    {
        public GameState gameState;
        public float time;
    }
    
    //Game Time
    private double _time;
    private Dictionary<GameState, float> _gameStateTimes;

    [SerializeField]
    private List<InitStruct> gameStateTimes;
    
    [SerializeField]
    private float _defaultStateTime = 120.0f;
    
    //UI
    [SerializeField] private Text timerText;
    [SerializeField] private Text bgText;
    
    //GameManager
    private GameManager _gameManager;


    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
        _gameStateTimes = new Dictionary<GameState, float>();

        //Init states with default
        for (int i = 0; i < Enum.GetNames(typeof(GameState)).Length; i++)
        {
            _gameStateTimes.Add((GameState)i, _defaultStateTime);
        }
        
        //If added on editor component, use that values
        foreach (var init in gameStateTimes)
        {
            _gameStateTimes[init.gameState] = init.time;
        }
        
        _gameManager.SubscribeToEvent(GameEventType.onTimerUpdated, SetUITimer);
    }

    public void OnUpdate()
    {
        //Update Timer
        UpdateTime();
        
        //Invoke on timer updated
        _gameManager.InvokeEvent(GameEventType.onTimerUpdated);
    }

    public void OnFixedUpdate()
    {
        ////
    }

    void UpdateTime()
    {
        if (!_gameManager.gameStarted) return;
        
        if (PhotonNetwork.IsMasterClient)
            UpdateTimer();
    }
    
    void SetUITimer()
    {
        timerText.text = ((int)_time).ToString();
        bgText.text = ((int)_time).ToString();
    }
    
    
    void UpdateTimer()
    {
        //Update Room Time
        _time -= Time.deltaTime;

        if (!(_time <= 0f)) return;

        //If game time ended, check current state to start next one
        _gameManager.UpdateGameState();
        _time = _gameStateTimes[_gameManager.currentGameState];
    }

    public float GetStateTime(GameState state)
    {
        return _gameStateTimes[_gameManager.currentGameState];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_time);
        }
        else if (stream.IsReading)
        {
            _time = (double)stream.ReceiveNext();
        }
    }
}