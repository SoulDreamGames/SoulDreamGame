using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.Animations;
using Random = UnityEngine.Random;
using System.Security.Cryptography;

public class NPCRandomNavMesh : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animController;


    private Vector3 previousPosition;
    public float curSpeed;
    public float life;

    public Transform NPCtarget; //centre of the area the agent wants to move around in
    //instead of centrePoint you can set it as the transform of the agent if you don't care about a specific area

    //NPCManager
    private NPCManager _npcManager;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<Animator>();
    }

    public void Initialize(NPCManager npcManager, Transform targetPoint)
    {
        _npcManager = npcManager;
        NPCtarget = targetPoint;
        agent.SetDestination(targetPoint.position);
        life = 1.0f;
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
        }

        if (agent.remainingDistance <= agent.stoppingDistance) //done with path
        {
            //    Vector3 point;
            //    if (RandomPoint(centrePoint.position, range, out point)) //pass in our centre point and radius of area
            //    {
            //        Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
            //        agent.SetDestination(point);

            //        //ToDo: call this on died
            //        //_npcManager.OnSafePoint(this);
            //    }
            //}

            _npcManager.OnSafePoint(this);


        }

    }
}