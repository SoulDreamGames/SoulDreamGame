using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class PlayersManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;

    [SerializeField] private List<Vector3> respawnPoints = new List<Vector3>();
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
        Vector3 pos = Vector3.zero;
        if (respawnPoints.Count > 0)
        {
            pos = respawnPoints[rnd];
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