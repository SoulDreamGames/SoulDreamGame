using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShootingEnemyProjectile : MonoBehaviour
{
    public Vector3 Velocity;
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask WallsMask;
    [SerializeField] private float Lifetime = 30;
    [SerializeField] private float damage = 10.0f;
    private int AliveFramecount = 0;
    private ShootingEnemyBehaviour ParentEnemy;
    public void Initialize(Vector3 StartingPosition, Vector3 ProjectileVelocity, ShootingEnemyBehaviour parent)
    {
        transform.position = StartingPosition;
        Velocity = ProjectileVelocity;
        ParentEnemy = parent;
    }

    void FixedUpdate()
    {
        transform.position += Velocity;
        // Check and destroy the projectile if it passed it's current lifetime
        AliveFramecount++;
        
        const float FixedUpdateFPS = 50.0f;
        // if (AliveFramecount > Lifetime * FixedUpdateFPS) Destroy(gameObject);
        if (AliveFramecount > Lifetime * FixedUpdateFPS) PhotonDestroy();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the projectile has collided with a target
        if ((TargetMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            // TODO: Do damage to the target
            if (other.gameObject.TryGetComponent<NPCRandomNavMesh>(out NPCRandomNavMesh npc))
            {
                npc.life -= 0.1f;
                if (npc.life <= 0.0f) { ParentEnemy.NotifyHasEatenSomeone(other.gameObject);  }
                // Destroy(gameObject);
                PhotonDestroy();
            }
            else if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            {
                // Deal damage to the player
                player.ReceiveDamage(damage);
            }
        }
        else if ((WallsMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            // If the projectile hits a wall its destroyed
            // Destroy(gameObject);
            PhotonDestroy();
        }
    }

    void PhotonDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView view = GetComponent<PhotonView>();
            PhotonNetwork.Destroy(view);
        }
    }
}
