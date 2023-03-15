using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class PlayersManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;

    [SerializeField] private List<Vector3> respawnPoints = new List<Vector3>();
    
    //ToDo: add players here - Call it on playerStart or from Photon room
    public List<PlayerController> players;

    [SerializeField] private float _respawnTime = 5.0f;
    
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
    
    //ToDo: call this method from playerController on dead
    public void PlayerDied(PlayerController pc)
    {
        _gameManager.InvokeEvent(GameEventType.onPlayerDied);
        //Call respawn coroutine
        StartCoroutine(Respawn(_respawnTime, pc));
    }

    IEnumerator Respawn(float time, PlayerController pc)
    {
        yield return new WaitForSeconds(time);
        SpawnPlayer(pc);
    }
    public void SpawnPlayer(PlayerController pc)
    {
        //Respawn player into a random position in the map
        int rnd = Random.Range(0, respawnPoints.Count);
        //pc.Spawn(respawnPoints[rnd]);
        //ToDo: uncomment this and add this method to playerController
    }
}