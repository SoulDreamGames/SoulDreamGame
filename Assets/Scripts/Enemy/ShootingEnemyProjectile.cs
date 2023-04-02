using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEnemyProjectile : MonoBehaviour
{
    public Vector3 Velocity;
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private LayerMask WallsMask;
    [SerializeField] private float Lifetime = 30;
    private int AliveFramecount = 0;
    public void Initialize(Vector3 StartingPosition, Vector3 ProjectileVelocity)
    {
        transform.position = StartingPosition;
        Velocity = ProjectileVelocity;
    }

    void FixedUpdate()
    {
        transform.position += Velocity;
        // Check and destroy the projectile if it passed it's current lifetime
        AliveFramecount++;
        
        const float FixedUpdateFPS = 50.0f;
        if (AliveFramecount > Lifetime * FixedUpdateFPS) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the projectile has collided with a target
        if ((TargetMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            // TODO: Do damage to the target
            Debug.Log("Hit");
        }
        else if ((WallsMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            // If the projectile hits a wall its destroyed
            Destroy(gameObject);
        }
    }
}
