using System;
using System.Collections.Generic;
using UnityEngine;
using GameEventType = GameManager.GameEventType;
using Photon.Pun;

public class EnemiesManager : MonoBehaviour
{
    //ToDo: Sync enemies HP (for receive damage + die)
    //ToDo: call KillEnemy with Master and then Pun.Destroy
    //ToDo: delete from list in every client
    //ToDo: sync dome energy 
    
    //GameManager
    private GameManager _gameManager;

    private int remainingWaveEnemies = 10;

    [SerializeField] public BloodPool _BloodPool;
    [SerializeField]
    private List<EnemySpawnable> enemiesToSpawn = new List<EnemySpawnable>();
    [SerializeField] private List<EnemyBehaviour> _enemiesSpawned;
    
    //Spawn points list
    [SerializeField] private List<Transform> respawnPoints = new List<Transform>();
    [SerializeField] private List<GameObject> targetPoints = new List<GameObject>();
    [SerializeField] private List<int> enemiesPerWave = new List<int>();
    [SerializeField] private int defaultNEnemiesPerWave = 10;
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
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log("Spawning enemies on new wave");
        
        int numSpawnedEnemies = enemiesPerWave.Count > _gameManager.currentWave ? 
            enemiesPerWave[_gameManager.currentWave] : 
            defaultNEnemiesPerWave;

        numSpawnedEnemies *= _gameManager.GetNumPlayers();

        for (int i = 0; i < numSpawnedEnemies; i++)
        {
            /* Choose random spawn and attack points for each enemy */
            int enemyIndex = UnityEngine.Random.Range(0, enemiesToSpawn.Count);
            int spawnIndex = UnityEngine.Random.Range(0, respawnPoints.Count);
            int targetIndex = UnityEngine.Random.Range(0, targetPoints.Count);
        
            SpawnEnemySynced(enemiesToSpawn[enemyIndex], respawnPoints[spawnIndex].position, targetPoints[targetIndex]);
        }
        
        remainingWaveEnemies = _enemiesSpawned.Count;
    }

    public void SpawnEnemySynced(EnemySpawnable enemyToSpawn, Vector3 spawnPoint, GameObject targetPoint)
    {
        // Spawn enemy in a selected position
        GameObject enemy = PhotonNetwork.Instantiate(enemyToSpawn.name, spawnPoint, Quaternion.identity);
        if (enemy.TryGetComponent<EnemySpawnable>(out EnemySpawnable spawnable))
        {
            spawnable.Initialize(this, targetPoint);
        }
        
        _gameManager.view.RPC("SpawnedEnemyRPC", RpcTarget.All);
    }

    [PunRPC]
    private void SpawnedEnemyRPC()
    {
        _gameManager.InvokeEvent(GameEventType.onEnemySpawned);
    }
    
    //ToDo: both onEnemySpawned and onEnemyDied are yet subscribed by other components - Eg. use it on UI
    
    //ToDo: call this method when enemy killed/destroyed - Call inside the OnDestroy method from the enemy
    public void EnemyKilled(EnemyBehaviour enemy)
    {
        Debug.Log("------Enemy killed-----");
        if(!_gameManager) return;

        Debug.Log("------Remove and invoke-----");
        _enemiesSpawned.Remove(enemy);
        
        //Invoke Enemy Died event
        _gameManager.view.RPC("EnemyDiedRPC", RpcTarget.All);
        
        if (_gameManager.nearestEnemy != null)
        {
            if (_gameManager.nearestEnemy.Equals(enemy.gameObject))
            {
                _gameManager.nearestEnemy = null;
            }
        }

        if (_gameManager.targetableEnemy != null)
        {
            if (_gameManager.targetableEnemy.Equals(enemy.gameObject))
            {
                _gameManager.targetableEnemy = null;
            }
        }
    }
    
    [PunRPC]
    private void EnemyDiedRPC()
    {
        if(PhotonNetwork.IsMasterClient)
            _gameManager.InvokeEvent(GameEventType.onEnemyDied);
        
        //Decrease enemyCount and check if all enemies are cleared on this wave with master client
        remainingWaveEnemies--;

        if (!PhotonNetwork.IsMasterClient) return;
        if (remainingWaveEnemies <= 0)
        {
            _gameManager.view.RPC("WaveEndRPC", RpcTarget.All);
        } 
    }

    [PunRPC]
    public void WaveEndRPC()
    {
        _gameManager.InvokeEvent(GameEventType.onWaveEnd);
    }

    public void AddSpawnedEnemy(EnemyBehaviour enemy)
    {
        _enemiesSpawned.Add(enemy);
        remainingWaveEnemies = _enemiesSpawned.Count;
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
    public List<EnemyBehaviour> GetEnemiesSpawned()
    {
        return _enemiesSpawned;
    }
}
