using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyView : MonoBehaviour
{
    public LevitatingEnemyBehaviour parent_enemy;
    public LayerMask target_mask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        if (!parent_enemy.isLookingForTargets()) return;
        if ((target_mask.value & (1 << other.transform.gameObject.layer)) > 0) {
            parent_enemy._Target = other.gameObject;
            parent_enemy.stopLookingForTargets();

            if (other.gameObject.TryGetComponent<NPCRandomNavMesh>(out NPCRandomNavMesh npc))
            {
                if (!npc.isTargeted)
                {
                    npc.isTargeted = true;
                    npc._enemyFollowing = parent_enemy.gameObject;
                }
            }
        }
    }
}
