using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using GameEventType = GameManager.GameEventType;
using Photon.Pun;
using Debug = UnityEngine.Debug;
using UnityEngine.AI;

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


    Vector3 RandomPoint(Vector3 center, float range)
    {
        Vector3 result;
        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return result;
        }

        result = center;
        return result;
    }

    IEnumerator spawnNPCS(float waitTime)
    {
        for (int i = 0; i < npcsSpawnedPerWave; i++)
        {

            Vector3 spawnPnt = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position;
            Vector3 spawnPointWithOffset = RandomPoint(spawnPnt, 5.0f);


            SpawnNPC(npcPrefab, spawnPointWithOffset);
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
}