using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using GameEventType = GameManager.GameEventType;
using GameState = GameManager.GameState;


public class TimeManager : MonoBehaviour
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
    
    //PUN Properties
    private Room _room;


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
        
        //Init time and set current time state
        _room = PhotonNetwork.CurrentRoom;
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable hash = new Hashtable() { { "Time", _time } };
            _room.SetCustomProperties(hash);
        }
        else
        {
            _time = (double)_room.CustomProperties["Time"];
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
        if (PhotonNetwork.IsMasterClient)
            UpdateTimer();

        else ReadTimer();
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
        UpdateRoomTimeProperty();
        
        if (!(_time <= 0f)) return;

        //If game time ended, check current state to start next one

        _gameManager.UpdateGameState();
        _time = _gameStateTimes[_gameManager.currentGameState];
    }

    //Read timer for non-master clients / non-server
    void ReadTimer()
    {
        _time = (double)_room.CustomProperties["Time"];
    }
    
    //Update timer for all clients
    void UpdateRoomTimeProperty()
    {
        Hashtable hash = _room.CustomProperties;
        hash.Remove("Time");
        hash.Add("Time", _time);
        _room.SetCustomProperties(hash);
    }
}