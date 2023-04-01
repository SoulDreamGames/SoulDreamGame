using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatingEnemyEntity : LevitatingEnemyBehaviour 
{
    private DuplicatingEnemySwarm mySwarm;
    void Start()
    {
        // Enemy stats
        Hitpoints = 10;
    }
    public void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget, DuplicatingEnemySwarm dup_swarm) {
        base.Initialize(enemiesManager, defaultTarget);
        mySwarm = dup_swarm;
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

    public override void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);
        if ((TargetMask.value & (1 << collision.gameObject.transform.gameObject.layer)) > 0) {
            Vector3 direction = Vector3.Normalize(transform.position - collision.gameObject.transform.position);
            mySwarm.createNewMember(transform.position + direction);
        }
    }

    protected override void OnDeath()
    {
        mySwarm.MemberDied(this);
        base.OnDeath();
    }
}