using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Enemy swarm must controll a group of enemies 
such that they movement seems like a swarm of insects */
public abstract class EnemySwarm : MonoBehaviour
{

    public uint num_enemies = 5;
    public float swarm_stiffness = 0.001f;
    public float swarm_distance_apart = 5.0f;
    public GameObject swarm_target;
    public List<LevitatingEnemyBehaviour> swarmMembers;

    [SerializeField] protected GameObject swarm_default_target;
    [SerializeField] protected EnemiesManager _enemy_manager;
    bool swarm_has_active_target = false;
    // Start is called before the first frame update
    void Start()
    {
        if (swarmMembers.Count != num_enemies){
            num_enemies = (uint) swarmMembers.Count;
        }
        foreach(LevitatingEnemyBehaviour enemy in swarmMembers){
            enemy._Target = swarm_target;
        }
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

        for (int i = 0; i < num_enemies; i++){
            for (int j = i+1; j < num_enemies; j++){
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
        for (int i = 0; i < num_enemies; i++){
            swarmMembers[i]._DefaultTarget = swarm_default_target;
        }
    }
    public void updateSwarmTarget()
    {
        /* Makes all swarm members change target to the swarm's target */
        for (int i = 0; i < num_enemies; i++){
            swarmMembers[i].setTarget(swarm_target);
        }
    }
    public void updateTargets()
    {
        /* Checks weather a member of the swarm has spotted a target
        *  and updates the swarm's target accordingly */
        bool someone_spotted_target = false;
        for (int i = 0; i < num_enemies; i++){
            if (!swarmMembers[i].isLookingForTargets()) {
                // Someone spotted an enemy
                swarm_target = swarmMembers[i]._Target;
                someone_spotted_target = true;
                swarm_has_active_target = true;
                break;
            }
        }
        if (!someone_spotted_target && swarm_has_active_target) {
            swarm_target = swarm_default_target;
            updateSwarmTarget();
        }

        if (!swarm_has_active_target) return;

        updateSwarmTarget();
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
