using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatingEnemyEntity : LevitatingEnemyBehaviour 
{
    private DuplicatingEnemySwarm mySwarm;
    void Start()
    {
        // Enemy stats
        Hitpoints = 2;
    }
    public void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget, DuplicatingEnemySwarm dup_swarm) {
        base.Initialize(enemiesManager, defaultTarget);
        mySwarm = dup_swarm;
        startLookingForTargets();
    }
    public override bool ReceiveDamage(int damage) {
        Hitpoints -= damage;
        bool died = Hitpoints <= 0;
        if (died) {
            OnDeath();
        }
        return died;
    }
    public override void NotifyHasEatenSomeone(GameObject someone)
    {
        base.NotifyHasEatenSomeone(someone);
    }

    public override void OnTriggerEnter(Collider collider) {
        base.OnTriggerEnter(collider);
        if ((TargetMask.value & (1 << collider.gameObject.transform.gameObject.layer)) > 0) {

            if (collider.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            {
                Debug.Log("Player is attacking the enemy");
                //if player is attacking, then this wont duplicate
                if (player.IsAttacking || player.IsHomingAttacking)
                {
                    Debug.Log("is attacking so dont duplicate");
                    return;
                }
            }
            
            Vector3 direction = Vector3.Normalize(transform.position - collider.gameObject.transform.position);
            mySwarm.createNewMember(transform.position + direction);
            Debug.Log("Duplicating");
        }
    }

    protected override void OnDeath()
    {
        mySwarm.MemberDied(this);
        base.OnDeath();
    }
}