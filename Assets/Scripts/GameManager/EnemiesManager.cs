using System;
using System.Collections.Generic;
using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class EnemiesManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;

    private int remainingWaveEnemies = 10;
    
    //Spawn points list
    [SerializeField] private List<Vector3> respawnPoints = new List<Vector3>();
    
    public void Initialize(GameManager gameManager)
    {
        //Init gameManager
        _gameManager = gameManager;
    }

    public void OnUpdate()
    {
        ////
    }

    public void OnFixedUpdate()
    {
        ////
    }

    public void SpawnEnemy(Vector3 spawnPoint)
    {
        //ToDo: spawn enemy in a selected position

        //Invoke enemySpawn event 
        _gameManager.InvokeEvent(GameEventType.onEnemySpawned);
    }

    //ToDo: both onEnemySpawned and onEnemyDied are yet subscribed by other components - Eg. use it on UI
    
    //ToDo: call this method when enemy killed/destroyed - Call inside the OnDestroy method from the enemy
    public void EnemyKilled()
    {
        //Invoke Enemy Died event
        _gameManager.InvokeEvent(GameEventType.onEnemyDied);
        
        //Decrease enemyCount and check if all enemies are cleared on this wave
        remainingWaveEnemies--;
        if (remainingWaveEnemies <= 0)
        {
            _gameManager.InvokeEvent(GameEventType.onWaveEnd);
        } 
    }
}