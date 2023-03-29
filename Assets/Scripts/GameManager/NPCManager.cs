using System;
using System.Collections.Generic;
using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class NPCManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;


    [SerializeField] private GameObject npcPrefab;
    //Spawn points list
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    
    //Safe zones points list
    [SerializeField] private List<Transform> safeZones = new List<Transform>();
    
    private List<NPCRandomNavMesh> _npcsSpawned;
    
    public void Initialize(GameManager gameManager)
    {
        //Init gameManager
        _gameManager = gameManager;
        _npcsSpawned = new List<NPCRandomNavMesh>();
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
        //for

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            SpawnNPC(npcPrefab, spawnPoints[i].position);
        }

         //etc...
    }
    
    public void SpawnNPC(GameObject npcToSpawn, Vector3 spawnPoint)
    {
        //Spawn new enemy
        GameObject npc = Instantiate(npcToSpawn, spawnPoint, Quaternion.identity);
        npc.GetComponent<NPCRandomNavMesh>().Initialize(this, safeZones[UnityEngine.Random.Range(0, safeZones.Count)]);
        _npcsSpawned.Add(npc.GetComponent<NPCRandomNavMesh>());
    }
    
    
    
    //ToDo: Call this method every time a civilian is about to die (inside OnDestroy)
    public void NPCDied(NPCRandomNavMesh npc)
    {
        //Remove npc from active npcs list
        _npcsSpawned.Remove(npc);
        
        //Invoke corresponding event
        _gameManager.InvokeEvent(GameEventType.onNPCDied);
    }
    
    public void OnSafePoint(NPCRandomNavMesh npc)
    {
        //Remove npc from active npcs list
        _npcsSpawned.Remove(npc);
    }
}