using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(PlayersManager))]
[RequireComponent(typeof(EnemiesManager))] 
[RequireComponent(typeof(NPCManager))]
public class GameManager : MonoBehaviour
{
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
        onNPCDied = 8,
        onSceneQuit = 9
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
    [HideInInspector] public EnemiesManager _enemiesManager;
    [HideInInspector] public NPCManager _npcManager;

    //Game properties
    [HideInInspector] public int currentWave = 0;
    public int totalWaves = 5;

    //City energy, game ends when decreased to 0
    public float cityEnergy = 100.0f;
    public float energyLostOnPlayer = 5.0f;
    public float energyLostOnCivilian = 0.5f;

    public float domeEnergy = 0.0f;
    public float maxDomeEnergy = 100.0f;
    public float energyGainedOnKill = 1.0f;
    public float energyLostOnDeath = 10.0f;
    
    //Game state 
    [HideInInspector] public GameState currentGameState;
    [HideInInspector] public bool gameStarted = false;
    
    //Player variables
    [HideInInspector] public PlayerController localPlayer;
    [SerializeField] private Image enemyMarker;
    public GameObject nearestEnemy = null;
    public GameObject targetableEnemy = null;
    
    //Results scene
    [SerializeField] private string resultsScene = "Menu";
    [SerializeField] private ResultsData _resultsData;
    
    //Results variables
    private bool _gameVictory = false;
    private int enemyKills = 0;
    private int nDeaths = 0;

    private void Start()
    {
        //Init game Events based on GameEventType definition
        onGameEvents = new List<UnityEvent>(Enum.GetNames(typeof(GameEventType)).Length);
        for (int i = 0; i < onGameEvents.Capacity; i++)
        {
            onGameEvents.Add(new UnityEvent());
        }

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
        
        //UI
        UpdateNearestEnemyOnScreen();
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

    private void EndingGame()
    {
        currentGameState = GameState.EndingGame;
    }
    private void EndGame()
    {
        //ToDo: add behaviour here - Eg. call cinematic, show statistics, etc.
        //Check game results and show ending depending on that
        gameStarted = false;

        SaveDataToResults();
        
        ShowGameResults();
    }

    void SaveDataToResults()
    {
        //Save data to scriptable object
        _resultsData.victory = _gameVictory;
        _resultsData.evacuees = _npcManager.peopleEvacuated;    //ToDo: divide with total spawned
        _resultsData.domeEnergy = (int)domeEnergy;
        _resultsData.enemiesKilled =  enemyKills;
        _resultsData.nDeaths = nDeaths;
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ShowGameResults()
    {
        PhotonNetwork.LoadLevel(resultsScene);
    }

    public void DecreaseCityEnergy(float energy)
    {
        cityEnergy -= energy;

        if (cityEnergy <= 0.0f)
        {
            InvokeEvent(GameEventType.onGameEnd);
            _gameVictory = false;
        }
    }

    public void SubscribeToEvent(GameEventType eventType, UnityAction action)
    {
        onGameEvents[(int)eventType].AddListener(action);
    }

    public void InvokeEvent(GameEventType eventType)
    {
        //Debug.Log("Called event + " + eventType.ToString());
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
            case GameState.EndingGame:
                InvokeEvent(GameEventType.onSceneQuit);
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
        SubscribeToEvent(GameEventType.onGameEnd, EndingGame);
        SubscribeToEvent(GameEventType.onSceneQuit, EndGame);
        SubscribeToEvent(GameEventType.onPlayerDied, () =>
        {
            DecreaseCityEnergy(energyLostOnPlayer);
        });
        SubscribeToEvent(GameEventType.onPlayerDied, () =>
        {
            nDeaths++;
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
            {
                currentGameState = GameState.EndingGame;
                _gameVictory = true;
                InvokeEvent(GameEventType.onGameEnd);
            }
        });
        SubscribeToEvent(GameEventType.onNPCDied, () =>
        {
            DecreaseCityEnergy(energyLostOnCivilian);
        });
        
        //Dome energy events - Gain on enemy kill + loss on player respawn
        SubscribeToEvent(GameEventType.onEnemyDied, () =>
        {
            UpdateDomeEnergy(energyGainedOnKill);
        });
        SubscribeToEvent(GameEventType.onEnemyDied, () =>
        {
            enemyKills++;
        });
        
        SubscribeToEvent(GameEventType.onPlayerDied, () =>
        {
            UpdateDomeEnergy(-energyLostOnDeath);
        });
    }

    //ToDo: change this to UIManager
    private void UpdateNearestEnemyOnScreen()
    {
        if (targetableEnemy == null)
        {
            enemyMarker.enabled = false;
            return;
        }

        Vector3 point = Camera.main.WorldToScreenPoint(targetableEnemy.transform.position);
        //Only print this if inside screen
        if (point.z > 0 && point.x > 0 && point.y > 0 && point.x < Screen.width && point.y < Screen.width)
        {
            float scale = Mathf.Clamp((localPlayer.homingRadius - point.z), 0f, localPlayer.homingRadius) / localPlayer.homingRadius;
            enemyMarker.rectTransform.localScale = new Vector3(scale, scale, scale) * 1.5f;
            
            point.z = 0.0f;
            enemyMarker.rectTransform.position = point;
            
            if (!enemyMarker.enabled) enemyMarker.enabled = true;
            return;
        }
        
        enemyMarker.enabled = false;
    }

    private void UpdateDomeEnergy(float energy)
    {
        domeEnergy += energy;
        domeEnergy = Math.Clamp(domeEnergy, 0f, maxDomeEnergy);

        if (domeEnergy >= maxDomeEnergy)
        {
            //Call gameEnd as shield has been fully charged
            InvokeEvent(GameEventType.onGameEnd);
            _gameVictory = true;
            //ToDo: this one is the real victory (charge the shield)
        }
    }
    
    public List<EnemyBehaviour> GetEnemiesSpawnedList()
    {
        return _enemiesManager.GetEnemiesSpawned();
    }
}