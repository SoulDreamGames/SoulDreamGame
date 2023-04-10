using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DuplicatingEnemyEntity : LevitatingEnemyBehaviour
{
    private DuplicatingEnemySwarm mySwarm;

    void Start()
    {
        // Enemy stats
        Hitpoints = 2;
    }

    public void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget, DuplicatingEnemySwarm dup_swarm)
    {
        base.Initialize(enemiesManager, defaultTarget);
        mySwarm = dup_swarm;
        startLookingForTargets();
    }

    public override void ReceiveDamage(int damage)
    {
        //Implemented on base class
        View.RPC("OnReceiveDamageRPC", RpcTarget.MasterClient, damage);
    }

    public override void NotifyHasEatenSomeone(GameObject someone)
    {
        base.NotifyHasEatenSomeone(someone);
    }

    public override void OnTriggerEnter(Collider collider)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        base.OnTriggerEnter(collider);
        if ((TargetMask.value & (1 << collider.gameObject.transform.gameObject.layer)) > 0)
        {
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
        if (!PhotonNetwork.IsMasterClient) return;

        mySwarm.MemberDied(this);
        base.OnDeath();
    }

    public override void ChangeToDefaultTarget()
    {
        startLookingForTargets();
        mySwarm.ChangeToDefaultTarget();
    }
}