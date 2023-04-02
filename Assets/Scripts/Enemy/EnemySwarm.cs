using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Enemy swarm must controll a group of enemies 
such that they movement seems like a swarm of insects */
public abstract class EnemySwarm : EnemySpawnable
{

    public uint NumEnemies = 5;
    public float swarm_stiffness = 0.001f;
    public float swarm_distance_apart = 5.0f;
    public GameObject _SwarmTarget;
    public List<LevitatingEnemyBehaviour> swarmMembers;

    [SerializeField] protected GameObject SwarmDefaultTarget;
    [SerializeField] protected EnemiesManager _EnemyManager;
    public bool SwarmHasActiveTarget = false;
    // Start is called before the first frame update

    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget){
        _EnemyManager = enemiesManager;
        SwarmDefaultTarget = defaultTarget;
    }

    void FixedUpdate() {
        apply_swarm_force();
        updateTargets();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    protected void apply_swarm_force(){
        /* Makes the enemies be slightly repelled from one another when close and slightly 
        attracted when close */

        for (int i = 0; i < NumEnemies; i++){
            for (int j = i+1; j < NumEnemies; j++){
                Vector3 delta_pos = (swarmMembers[i].transform.position - swarmMembers[j].transform.position);
                float distance = delta_pos.magnitude;
                Vector3 force = - swarm_stiffness * (distance - swarm_distance_apart) * delta_pos / distance;

                swarmMembers[i].addExternalForce(force);
                swarmMembers[j].addExternalForce(-force);
            }
        }
    }
    public void updateSwarmDefaultTarget()
    {
        /* Makes all swarm members change default target to the swarm's default target */
        Debug.Log("updating swarm default target");
        for (int i = 0; i < NumEnemies; i++){
            swarmMembers[i]._DefaultTarget = SwarmDefaultTarget;
        }
    }
    public void updateSwarmTarget()
    {
        /* Makes all swarm members change target to the swarm's target */
        for (int i = 0; i < NumEnemies; i++){
            swarmMembers[i].setTarget(_SwarmTarget);
        }
    }
    public void updateTargets()
    {
        /* Checks weather a member of the swarm has spotted a target
        *  and updates the swarm's target accordingly */
        if (!SwarmHasActiveTarget)
        {
            foreach (EnemyBehaviour Enemy in swarmMembers)
            {
                if (!Enemy.isLookingForTargets())
                {
                    _SwarmTarget = Enemy._Target;
                    SwarmHasActiveTarget = true;
                    updateSwarmTarget();
                    return;
                }
            }
        } 
        else
        {
            updateSwarmDefaultTarget();
        }
    }

    public Vector3 calculateAveragePosition() {
        Vector3 average = new Vector3(0,0,0);
        foreach(LevitatingEnemyBehaviour enemy in swarmMembers) 
        {
            average += enemy.transform.position;
        }
        return average / swarmMembers.Count;
    }

}
