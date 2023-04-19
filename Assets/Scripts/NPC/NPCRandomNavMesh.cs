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
    public bool hasPath;
    public GameObject _enemyFollowing;
    
    private float distanceToTarget = 5.0f;

    //NPCManager
    [SerializeField] private NPCManager _npcManager;
    
    //PhotonView
    [HideInInspector] public PhotonView view;

    private bool _isInitialized = false;


    private void Awake()
    {
        view = GetComponent<PhotonView>();
        animController = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }
    
    public void Initialize(NPCManager npcManager, Transform targetPoint)
    {
        _npcManager = npcManager;
        NPCtarget = targetPoint;
        
        life = 1.0f;
        isTargeted = false;
        _enemyFollowing = null;

        try
        {
            agent.SetDestination(targetPoint.position);
        }
        catch (Exception)
        {
            UnityEngine.Debug.Log("initialize", gameObject);
            throw;
        }
        
        

        _isInitialized = true;
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

        result = Vector3.zero;
        return result;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!_isInitialized) return;
        hasPath = agent.hasPath;
        
        Vector3 curMove = transform.position - previousPosition;
        curSpeed = curMove.magnitude / Time.deltaTime;

        previousPosition = transform.position;

        float velocity = agent.velocity.magnitude / agent.speed;

        animController.SetFloat("MoveSpeed", curSpeed);

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
            return;

        }


        if (isTargeted)
        {
            if (_enemyFollowing == null) 
            {
                isTargeted = false;
                return;
            }
            Vector3 dirToPlayer = transform.position - _enemyFollowing.transform.position;

            //dirToPlayer.y = transform.position.y;

            Vector3 newTemporalPos = transform.position + 5* Vector3.Normalize(dirToPlayer);

            // agent.SetDestination(newTemporalPos);



            NavMeshPath navMeshPath = new NavMeshPath();
            //create path and check if it can be done
            // and check if navMeshAgent can reach its target
            if ((life > 0.0f) && agent.CalculatePath(newTemporalPos, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                //move to target
                if (PhotonNetwork.IsMasterClient)
                {

                    try
                    {
                        agent.SetDestination(newTemporalPos);
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.Log("TemporalPos", gameObject);
                        throw;
                    }
                }
            }
            else
            {
                if (PhotonNetwork.IsMasterClient && (life > 0.0f))
                {
                    

                    try
                    {
                        agent.SetDestination(NPCtarget.position);
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.Log("Targeted but no path", gameObject);
                        throw;
                    }
                    
                }
            }
            


            // UnityEngine.Debug.Log(agent.destination);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient && (life > 0.0f))
            {
                

                try
                {
                    agent.SetDestination(NPCtarget.position);
                }
                catch (Exception)
                {
                    UnityEngine.Debug.Log("Normal", gameObject);
                    throw;
                }
                
            }
        }

        if ((life > 0.0f) && NPCtarget && ((Vector3.Distance(transform.position, NPCtarget.position)) < distanceToTarget)) //done with path
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