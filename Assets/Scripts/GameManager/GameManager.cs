using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(PlayersManager))]
[RequireComponent(typeof(EnemiesManager))] 
[RequireComponent(typeof(NPCManager))]
public class GameManager : MonoBehaviour
{
    //ToDo: save statistics here - Eg. n_enemies killed, enemies remaining, civilians killed, etc.
    //ToDo: add get nearest enemy, get nearest civilian and get nearest player for targets
    
    public enum GameEventType
    {
        onGameStart = 0,
        onWaveStart = 1,
        onWaveEnd = 2,
        onPlayerDied = 3,
        onTimerUpdated = 4,
        onGameEnd = 5,
        onEnemySpawned = 6,
        onEnemyDied = 7,
        onNPCDied = 8
    }

    [Serializable]
    public enum GameState
    {
        StartingGame = 0,
        WaittingWave = 1,
        OnWave = 2,
        EndingGame = 3,
    }

    //Events list
    private List<UnityEvent> onGameEvents;

    //Manager components
    private TimeManager _timeManager;
    private PlayersManager _playersManager;
    private EnemiesManager _enemiesManager;
    private NPCManager _npcManager;

    //Game properties
    [HideInInspector] public int currentWave = 0;
    public int totalWaves = 5;

    //City energy, game ends when decreased to 0
    public float cityEnergy = 100.0f;
    public float energyLostOnPlayer = 5.0f;
    public float energyLostOnCivilian = 0.5f;

    //Game state 
    [HideInInspector] public GameState currentGameState;
    [HideInInspector] public bool gameStarted = false;

    private void Start()
    {
        //Init game Events based on GameEventType definition
        onGameEvents = new List<UnityEvent>(Enum.GetNames(typeof(GameEventType)).Length);

        //Init all callbacks needed inside the game manager
        InitGameManagerEvents();
        
        //Invoke start event
        InvokeEvent(GameEventType.onGameStart);
    }

    private void Update()
    {
        if (!gameStarted) return;

        _timeManager.OnUpdate();
        _playersManager.OnUpdate();
        _enemiesManager.OnUpdate();
        _npcManager.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (!gameStarted) return;

        _timeManager.OnFixedUpdate();
        _playersManager.OnFixedUpdate();
        _enemiesManager.OnFixedUpdate();
        _npcManager.OnFixedUpdate();
    }

    private void InitGame()
    {
        //ToDo: add behaviour here
        gameStarted = true;
    }

    private void EndGame()
    {
        //ToDo: add behaviour here - Eg. call cinematic, show statistics, etc.
        //Check game results and show ending depending on that
        
        currentGameState = GameState.EndingGame;
        gameStarted = false;
    }

    public void DecreaseCityEnergy(float energy)
    {
        cityEnergy -= energy;

        if (cityEnergy <= 0.0f)
        {
            InvokeEvent(GameEventType.onGameEnd);
        }
    }

    public void SubscribeToEvent(GameEventType eventType, UnityAction action)
    {
        onGameEvents[(int)eventType].AddListener(action);
    }

    public void InvokeEvent(GameEventType eventType)
    {
        onGameEvents[(int)eventType].Invoke();
    }

    public void UpdateGameState()
    {
        switch (currentGameState)
        {
            case GameState.StartingGame:

                //Set time to waitting wave countdown
                currentGameState = GameState.WaittingWave;
                break;
            case GameState.WaittingWave:

                //Set time to starting wave countdown
                InvokeEvent(GameEventType.onWaveStart);
                break;
            case GameState.OnWave:

                //Invoke GameEnd if completed waves or waveEnd if middle wave
                InvokeEvent(GameEventType.onWaveEnd);
                break;
        }
    }

    public void InitGameManagerEvents()
    {
        //Get components
        _timeManager = GetComponent<TimeManager>();
        _playersManager = GetComponent<PlayersManager>();
        _enemiesManager = GetComponent<EnemiesManager>();
        _npcManager = GetComponent<NPCManager>();
        
        //Initialize components
        _timeManager.Initialize(this);
        _playersManager.Initialize(this);
        _enemiesManager.Initialize(this);
        _npcManager.Initialize(this);

        //Subscribe to events
        SubscribeToEvent(GameEventType.onGameStart, InitGame);
        SubscribeToEvent(GameEventType.onGameEnd, EndGame);
        SubscribeToEvent(GameEventType.onPlayerDied, () =>
        {
            DecreaseCityEnergy(energyLostOnPlayer);
        });
        SubscribeToEvent(GameEventType.onWaveStart, () => { 
            currentGameState = GameState.OnWave;
            currentWave++;
        });
        SubscribeToEvent(GameEventType.onWaveEnd, () =>
        {
            currentGameState = GameState.WaittingWave;

            //If currentWave is last one, then call gameEnd
            if (currentWave >= totalWaves)
                InvokeEvent(GameEventType.onGameEnd);
        });
        SubscribeToEvent(GameEventType.onNPCDied, () =>
        {
            DecreaseCityEnergy(energyLostOnCivilian);
        });
    }
}