using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Enemy swarm must controll a group of enemies 
such that they movement seems like a swarm of insects */
public class enemy_swarm : MonoBehaviour
{

    public uint num_enemies = 5;
    public float swarm_stiffness = 0.1f;
    public float swarm_distance_apart = 5.0f;
    public GameObject swarm_target;
    public enemy_controller[] swarm;

    private GameObject swarm_default_target;
    bool swarm_has_active_target = false;
    // Start is called before the first frame update
    void Start()
    {
        // swarm = new enemy_controller[num_enemies];
        if (swarm.Length != num_enemies){
            num_enemies = (uint) swarm.Length;
        }
        foreach(enemy_controller enemy in swarm){
            enemy.target = swarm_target;
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

    void apply_swarm_force(){
        /* Makes the enemies be slightly repelled from one another when close and slightly 
        attracted when close */

        for (int i = 0; i < num_enemies; i++){
            for (int j = i+1; j < num_enemies; j++){
                Vector3 delta_pos = (swarm[i].transform.position - swarm[j].transform.position);
                float distance = delta_pos.magnitude;
                Vector3 force = - swarm_stiffness * (distance - swarm_distance_apart) * delta_pos / distance;

                swarm[i].addExternalForce(force);
                swarm[j].addExternalForce(-force);
            }
        }
    }

    void updateSwarmTarget(){
        /* Makes all swarm members change target to the swarm's target */
        for (int i = 0; i < num_enemies; i++){
            swarm[i].target = swarm_target;
        }
    }
    void updateTargets(){
        /* Checks weather a member of the swarm has spotted a target
        *  and updates the swarm's target accordingly */

        bool someone_spotted_target = false;
        for (int i = 0; i < num_enemies; i++){
            if (!swarm[i].isLookingForTargets()) {
                // Someone spotted an enemy
                swarm_target = swarm[i].target;
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

}
