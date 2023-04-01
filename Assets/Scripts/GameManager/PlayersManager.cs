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

    //Add players to player list on spawn and set local player
    public void AddPlayer(PlayerController pc)
    {
        players.Add(pc);

        if (!pc.view.IsMine) return;
        _gameManager.localPlayer = pc;
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

    public Vector3 GetLocalNearestEnemy(float radius)
    {
        var enemies = _gameManager.GetEnemiesSpawnedList();
        return GetPositionToNearestEnemy(enemies, radius);
    }
    
    public Vector3 GetPositionToNearestEnemy(List<EnemyBehaviour> enemies, float attackRadius)
    {
        float minDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        //Get nearest by checking all distances
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance > attackRadius) continue;
            
            if (minDistance > distance)
            {
                nearestEnemy = enemy.gameObject;
                minDistance = distance;
            }
        }

        //Return nearest
        if (nearestEnemy != null)
        {
            Debug.Log("Nearest enemy is: " + _gameManager.nearestEnemy.name);
            _gameManager.nearestEnemy = nearestEnemy;
            return nearestEnemy.transform.position;
        }

        Debug.Log("Enemy not found");
        _gameManager.nearestEnemy = null;
        return Vector3.positiveInfinity;
    }
}