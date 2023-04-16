using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using GameEventType = GameManager.GameEventType;
using Photon.Pun;
using Debug = UnityEngine.Debug;

public class NPCManager : MonoBehaviour, IPunObservable
{
    //GameManager
    private GameManager _gameManager;


    [SerializeField] private GameObject npcPrefab;

    //Spawn points list
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    //Safe zones points list
    [SerializeField] private List<Transform> safeZones = new List<Transform>();
    
    [SerializeField] private int npcsSpawnedPerWave = 100;
    private float _waitTimePerNpc;

    public List<NPCRandomNavMesh> _npcsSpawned;

    public int peopleEvacuated = 0;
    public int peopleDied = 0;


    public void Initialize(GameManager gameManager)
    {
        //Init gameManager
        _gameManager = gameManager;
        _npcsSpawned = new List<NPCRandomNavMesh>();
        _gameManager.SubscribeToEvent(GameEventType.onWaveStart, SpawnOnNewWave);
        
        _waitTimePerNpc = gameManager.getStateTime(GameManager.GameState.OnWave) * 0.5f / npcsSpawnedPerWave;
        Debug.Log("Wait time: " + _waitTimePerNpc);
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

        //Coroutine for spawning each time
        StartCoroutine(spawnNPCS(_waitTimePerNpc));

        //etc...
    }

    public void SpawnNPC(GameObject npcToSpawn, Vector3 spawnPoint)
    {
        //Spawn new enemy
        GameObject npc = PhotonNetwork.Instantiate(npcToSpawn.name, spawnPoint, Quaternion.identity);

        float dist = 9999999.0f;
        int index = -1;

        for (int i = 0; i < safeZones.Count; i++)
        {
            if (Vector3.Distance(spawnPoint, safeZones[i].position) < dist)
            {
                index = i;
            }
        }

        NPCRandomNavMesh npcRandomNavMesh = npc.GetComponent<NPCRandomNavMesh>();
        npcRandomNavMesh.Initialize(this, safeZones[index]);
        _npcsSpawned.Add(npcRandomNavMesh);
    }

    IEnumerator spawnNPCS(float waitTime)
    {
        for (int i = 0; i < npcsSpawnedPerWave; i++)
        {
            SpawnNPC(npcPrefab, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position);
            yield return new WaitForSeconds(waitTime);
        }
    }


    //ToDo: Call this method every time a civilian is about to die (inside OnDestroy)
    public void NPCDied(NPCRandomNavMesh npc)
    {
        //Remove npc from active npcs list
        _npcsSpawned.Remove(npc);

        //Invoke corresponding event on All clients
        _gameManager.view.RPC("NPCDiedEventRPC", RpcTarget.All);

        PhotonNetwork.Destroy(npc.view);

        peopleDied++;
    }

    [PunRPC]
    private void NPCDiedEventRPC()
    {
        _gameManager.InvokeEvent(GameEventType.onNPCDied);
    }

    public void OnSafePoint(NPCRandomNavMesh npc)
    {
        //Remove npc from active npcs list
        _npcsSpawned.Remove(npc);
        PhotonNetwork.Destroy(npc.view);

        peopleEvacuated++;
    }

    //Sync peopleDied and peopleEvacuated events
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(peopleDied);
            stream.SendNext(peopleEvacuated);
        }
        else if (stream.IsReading)
        {
            peopleDied = (int)stream.ReceiveNext();
            peopleEvacuated = (int)stream.ReceiveNext();
        }
    }

    public int GetTotalSpawnedNPCs()
    {
        return npcsSpawnedPerWave * _gameManager.totalWaves;
    }
}