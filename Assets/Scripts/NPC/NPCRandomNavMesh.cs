using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Diagnostics;

public class NPCRandomNavMesh : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animController;


    private Vector3 previousPosition;
    public float curSpeed;
    public float life;

    public Transform NPCtarget;
    public Transform runfrom;

    public bool isTargeted;
    public GameObject _enemyFollowing;

    //NPCManager
    [SerializeField] private NPCManager _npcManager;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<Animator>();
        life = 1.0f;
        agent.SetDestination(NPCtarget.position);


        isTargeted = false;
        _enemyFollowing = null;

        _npcManager._npcsSpawned.Add(this);
    }

    public void Initialize(NPCManager npcManager, Transform targetPoint)
    {
        //_npcManager = npcManager;
        //NPCtarget = targetPoint;
        //agent.SetDestination(targetPoint.position);
        //ToDo: call this on died
        //_npcManager.NPCDied(this);
    }

    void Update()
    {

        Vector3 curMove = transform.position - previousPosition;
        curSpeed = curMove.magnitude / Time.deltaTime;

        previousPosition = transform.position;

        float velocity = agent.velocity.magnitude / agent.speed;

        animController.SetFloat("Blend", curSpeed);

        if (life <= 0.0f)
        {
            _npcManager.NPCDied(this);
            Destroy(this.gameObject);
        }


        if (isTargeted)
        {
            Vector3 dirToPlayer = transform.position - _enemyFollowing.transform.position;

            //dirToPlayer.y = transform.position.y;

            Vector3 newTemporalPos = transform.position + 5* Vector3.Normalize(dirToPlayer);

            agent.SetDestination(newTemporalPos);


            UnityEngine.Debug.Log(agent.destination);
        }
        else
        {
            agent.SetDestination(NPCtarget.position);
        }

        if (agent.remainingDistance <= agent.stoppingDistance) //done with path
        {
            _npcManager.OnSafePoint(this);

            if (isTargeted)
            {
                if (_enemyFollowing.TryGetComponent<EnemyBehaviour>(out EnemyBehaviour enemy))
                {
                    enemy.startLookingForTargets();
                }
            }
        }
    }
}