using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.Animations;

public class NPCRandomNavMesh : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animController;
    public float range; //radius of sphere

    private Vector3 previousPosition;
    public float curSpeed;

    public Transform centrePoint; //centre of the area the agent wants to move around in
    //instead of centrePoint you can set it as the transform of the agent if you don't care about a specific area

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<Animator>();
    }


    void Update()
    {

        Vector3 curMove = transform.position - previousPosition;
        curSpeed = curMove.magnitude / Time.deltaTime;

        previousPosition = transform.position;

        float velocity = agent.velocity.magnitude / agent.speed;

        animController.SetFloat("Blend", curSpeed);

        if (agent.remainingDistance <= agent.stoppingDistance) //done with path
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point)) //pass in our centre point and radius of area
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                agent.SetDestination(point);

            }
        }


    }
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}
