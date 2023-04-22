using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class PlayersManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;

    [SerializeField] private List<Transform> respawnPoints = new List<Transform>();
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
        //Get nearest enemy
        var enemies = _gameManager.GetEnemiesSpawnedList();
        GetPositionToNearestEnemy(enemies, _gameManager.localPlayer.homingRadius);
    }
    
    public void PlayerDied(PlayerController pc)
    {
        //Called only by isMine (from player controller)
        _gameManager.view.RPC("PlayerDiedEventRPC", RpcTarget.All);
        
        //Call respawn coroutine only on local
        StartCoroutine(Respawn(_respawnTime, pc));
    }

    [PunRPC]
    private void PlayerDiedEventRPC()
    {
        _gameManager.InvokeEvent(GameEventType.onPlayerDied);
        Debug.Log("------------ Player DIED --------");
    }

    IEnumerator Respawn(float time, PlayerController pc)
    {
        yield return new WaitForSeconds(time);
        
        if (_gameManager.gameStarted)
            SpawnPlayer(pc);
    }
    public void SpawnPlayer(PlayerController pc)
    {
        //Respawn player into a random position in the map
        int rnd = Random.Range(0, respawnPoints.Count);
        Vector3 pos = Vector3.zero;
        if (respawnPoints.Count > 0)
        {
            pos = respawnPoints[rnd].transform.position;
        }
        pc.Respawn(pos);
    }

    public Vector3 GetLocalNearestEnemy()
    {
        Debug.Log("Nearest enemy to target");
        return _gameManager.targetableEnemy ? 
            _gameManager.targetableEnemy.transform.position : 
            Vector3.positiveInfinity;
    }
    
    public void GetPositionToNearestEnemy(List<EnemyBehaviour> enemies, float attackRadius)
    {
        float minDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        //Get nearest by checking all distances
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(
                _gameManager.localPlayer.transform.position, enemy.transform.position);

            if (minDistance > distance)
            {
                nearestEnemy = enemy.gameObject;
                minDistance = distance;
            }
        }

        //Return nearest
        if (nearestEnemy != null)
        {
            _gameManager.nearestEnemy = nearestEnemy;
            _gameManager.targetableEnemy = minDistance <= attackRadius ? nearestEnemy : null;
        }
    }
}