using System;
using System.Collections.Generic;
using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class EnemiesManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;

    private int remainingWaveEnemies = 10;

    [SerializeField]
    private List<GameObject> enemiesToSpawn = new List<GameObject>();

    private List<EnemyBehaviour> _enemiesSpawned;
    
    //Spawn points list
    [SerializeField] private List<Transform> respawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> targetPoints = new List<Transform>();
    
    public void Initialize(GameManager gameManager)
    {
        //Init gameManager
        _gameManager = gameManager;
        _enemiesSpawned = new List<EnemyBehaviour>();
        _gameManager.SubscribeToEvent(GameEventType.onWaveStart, SpawnOnNewWave);
    }

    public void OnUpdate()
    {
        ////
    }

    public void OnFixedUpdate()
    {
        ////
    }

    void SpawnOnNewWave()
    {
        SpawnEnemy(enemiesToSpawn[0], Vector3.zero); //etc...

        remainingWaveEnemies = _enemiesSpawned.Count;
    }

    public void SpawnEnemy(GameObject enemyToSpawn, Vector3 spawnPoint)
    {
        //ToDo: spawn enemy in a selected position

        //Invoke enemySpawn event 
        GameObject enemy = Instantiate(enemyToSpawn, spawnPoint, Quaternion.identity);
        _enemiesSpawned.Add(enemy.GetComponent<EnemyBehaviour>());
        _gameManager.InvokeEvent(GameEventType.onEnemySpawned);
    }

    //ToDo: both onEnemySpawned and onEnemyDied are yet subscribed by other components - Eg. use it on UI
    
    //ToDo: call this method when enemy killed/destroyed - Call inside the OnDestroy method from the enemy
    public void EnemyKilled(EnemyBehaviour enemy)
    {
        //ToDo: delete this enemy
        //_enemiesSpawned.Delete(enemy);
        
        //Invoke Enemy Died event
        _gameManager.InvokeEvent(GameEventType.onEnemyDied);
        
        //Decrease enemyCount and check if all enemies are cleared on this wave
        remainingWaveEnemies--;
        if (remainingWaveEnemies <= 0)
        {
            _gameManager.InvokeEvent(GameEventType.onWaveEnd);
        } 
    }
    public void IncreaseWaveRemainingEnemies(int number) {
        remainingWaveEnemies += number;
    }

    // public void RemoveEnemyTarget(GameObject gameobject) {
    //  /* If NPC died, the enemies must find another target */
    //  foreach(EnemyBehaviour enemy in _enemiesSpawned) {
    //      if (enemy.target == gameobject) {
    //          enemy.target = null;
    //          enemy.startLookingForTargets();
    //      }
    //  }
    // }
}