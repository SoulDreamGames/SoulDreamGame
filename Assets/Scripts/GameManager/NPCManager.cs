using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    public List<NPCRandomNavMesh> _npcsSpawned;

    public int peopleEvacuated = 0;
    public int peopleDied = 0;
    
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

        UnityEngine.Debug.Log("Waveeee");

        for (int i = 0; i < 100; i++)
        {
            SpawnNPC(npcPrefab, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position);
        }

         //etc...
    }
    
    public void SpawnNPC(GameObject npcToSpawn, Vector3 spawnPoint)
    {
        //Spawn new enemy
        GameObject npc = Instantiate(npcToSpawn, spawnPoint, Quaternion.identity);

        float dist = 9999999.0f;
        int index = -1;

        for (int i = 0;i < safeZones.Count;i++)
        {
            if (Vector3.Distance(spawnPoint, safeZones[i].position)< dist)
            {
                index = i;
            }
        }

        npc.GetComponent<NPCRandomNavMesh>().Initialize(this, safeZones[index]);
        _npcsSpawned.Add(npc.GetComponent<NPCRandomNavMesh>());
    }
    
    
    
    //ToDo: Call this method every time a civilian is about to die (inside OnDestroy)
    public void NPCDied(NPCRandomNavMesh npc)
    {
        //Remove npc from active npcs list
        _npcsSpawned.Remove(npc);
        
        //Invoke corresponding event
        _gameManager.InvokeEvent(GameEventType.onNPCDied);

        Destroy(npc.gameObject);
        peopleDied++;
    }
    
    public void OnSafePoint(NPCRandomNavMesh npc)
    {
        //Remove npc from active npcs list
        _npcsSpawned.Remove(npc);
        Destroy(npc.gameObject);
        peopleEvacuated++;
    }
}