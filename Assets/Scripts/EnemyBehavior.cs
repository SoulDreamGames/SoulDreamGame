using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPositions;

    private void Start()
    {
        GetRandomPosition();
    }

    public void OnRespawn()
    {
        GetRandomPosition();
    }

    private void GetRandomPosition()
    {
        transform.position = spawnPositions[Random.Range(0, spawnPositions.Count)].position;
    }
}