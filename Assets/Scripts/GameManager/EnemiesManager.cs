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
    private List<EnemySpawnable> enemiesToSpawn = new List<EnemySpawnable>();

    public List<EnemyBehaviour> _enemiesSpawned;
    
    //Spawn points list
    [SerializeField] private List<Transform> respawnPoints = new List<Transform>();
    [SerializeField] private List<GameObject> targetPoints = new List<GameObject>();
    [SerializeField] private List<int> enemiesPerWave = new List<int>();
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
        Debug.Log("Spawning enemies on new waves");
        int NumSpawnedEnemies = enemiesPerWave[_gameManager.currentWave];
        for (int i = 0; i < 10; i++)
        {
            SpawnEnemy(enemiesToSpawn[0], respawnPoints[0].position, targetPoints[0]);
        }
        // SpawnEnemy(enemiesToSpawn[0], Vector3.zero); //etc...
        
        remainingWaveEnemies = _enemiesSpawned.Count;
    }

    public void SpawnEnemy(EnemySpawnable enemyToSpawn, Vector3 spawnPoint, GameObject targetPoint)
    {
        // Spawn enemy in a selected position

        EnemySpawnable enemy = Instantiate(enemyToSpawn, spawnPoint, Quaternion.identity);
        enemy.Initialize(this, targetPoint);
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
    public void AddSpawnedEnemy(EnemyBehaviour enemy)
    {
        remainingWaveEnemies++;
        _enemiesSpawned.Add(enemy);
    }

    public void GetNewDefaultTarget(ref GameObject lastTarget)
    {
        if (targetPoints.Count == 0) return;

        bool foundNewTarget = false;
        int index = 0;

        for (int i = 0; i < 100; i++)
        {
            index = UnityEngine.Random.Range(0, targetPoints.Count);
            foundNewTarget = (lastTarget != targetPoints[index]);
            if (foundNewTarget) break;
        }
        lastTarget = targetPoints[index];
    }
}