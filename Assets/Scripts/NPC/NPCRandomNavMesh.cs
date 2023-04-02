using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Diagnostics;
using Photon.Pun;

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
        if (PhotonNetwork.IsMasterClient) return;

        _npcManager = FindObjectOfType<NPCManager>();
        agent = FindObjectOfType<NavMeshAgent>();
        _npcManager._npcsSpawned.Add(this);

        animController = GetComponent<Animator>();

        life = 1.0f;
        isTargeted = false;
        _enemyFollowing = null;

    }


    public void Initialize(NPCManager npcManager, Transform targetPoint)
    {
        agent = GetComponent<NavMeshAgent>();


        _npcManager = npcManager;
        NPCtarget = targetPoint;
        animController = GetComponent<Animator>();
        life = 1.0f;
        isTargeted = false;
        _enemyFollowing = null;

        agent.SetDestination(targetPoint.position);
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
            if (_enemyFollowing != null)
            {
                if (_enemyFollowing.TryGetComponent<EnemyBehaviour>(out EnemyBehaviour enemy))
                {
                    enemy.ChangeToDefaultTarget();
                }
            }
            _npcManager.NPCDied(this);

        }


        if (isTargeted)
        {
            Vector3 dirToPlayer = transform.position - _enemyFollowing.transform.position;

            //dirToPlayer.y = transform.position.y;

            Vector3 newTemporalPos = transform.position + 5* Vector3.Normalize(dirToPlayer);

            // agent.SetDestination(newTemporalPos);



            NavMeshPath navMeshPath = new NavMeshPath();
            //create path and check if it can be done
            // and check if navMeshAgent can reach its target
            if (agent.CalculatePath(newTemporalPos, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                //move to target
                agent.SetDestination(newTemporalPos);
            }
            else
            {
                agent.SetDestination(NPCtarget.position);
            }
            


            // UnityEngine.Debug.Log(agent.destination);
        }
        else
        {
            agent.SetDestination(NPCtarget.position);
        }

        if ((Vector3.Distance(transform.position, NPCtarget.position))< 10.0f) //done with path
        {

            if (_enemyFollowing != null)
            {
                if (_enemyFollowing.TryGetComponent<EnemyBehaviour>(out EnemyBehaviour enemy))
                {
                    enemy.ChangeToDefaultTarget();
                }
            }

            _npcManager.OnSafePoint(this);
        }
    }
}