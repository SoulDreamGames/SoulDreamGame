using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShootingEnemyBehaviour : LevitatingEnemyBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float ShootingDistance = 10.0f; // Safe distance between the enemy and the target when the shooting can start
    [SerializeField] private float ShootingDelay = 10.0f; // The time delay between shots
    [SerializeField] private float ShootingRange = 10.0f; // At which distance from the target the enemy will try to shoot
    [SerializeField] private ShootingEnemyProjectile ProjectilePrefab;
    [SerializeField] private float ProjectileSpeed = 1.0f;
    private int ShootFramecount;
    private float TargetDistance = 100000000.0f;
    private Vector3 TargetPos;

    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        base.Initialize(enemiesManager, defaultTarget);
        DefaultTargetChangeDistance = ShootingDistance += 10.0f;
    }

    protected override void FixedUpdate()
    {
        ShootFramecount++;
        Shoot();
        base.FixedUpdate();
    }
    public override void FollowTarget(GameObject CurrentTarget)
    {
        TargetPos = CurrentTarget.transform.position;

        Vector3 WallRepulsion = targetRay(TargetPos);
        WallRepulsion += groundRay(HoverHeight);

        Vector3 TargetForceVec = new Vector3(0,0,0);
        TargetDistance = (TargetPos - transform.position).magnitude;
        if (TargetDistance > ShootingDistance)
        {
            TargetForceVec =  TargetForce * (TargetPos - transform.position).normalized;
        }
        Vector3 Acceleration = (WallRepulsion + TargetForceVec - Drag * Velocity + ExternalForces) / Mass;

        // Euler integration
        Velocity = Velocity + Acceleration;

        if (RB == null) {
            Debug.LogError("Shooting Enemy Behaviour: rigid body RB is null");
        }

        Vector3 Position = RB.position + Velocity;
        transform.position = Position;
        transform.LookAt(TargetPos, Vector3.up);

        ExternalForces = ExternalForces * 0;
    }

    public void Shoot()
    {
        // Check if the target is out of range
        if (TargetDistance > ShootingRange) return;
        const float FixedUpdateFPS = 50.0f;
        // Check if has passed enough time since the last shot
        if (ShootFramecount / FixedUpdateFPS < ShootingDelay) return;
        ShootFramecount = 0;

        // Actual shoot
        // ShootingEnemyProjectile current_projectile = Instantiate<ShootingEnemyProjectile>(ProjectilePrefab, transform.position, Quaternion.identity);
        // Vector3 ProjectileVelocity = ProjectileSpeed * (TargetPos - transform.position).normalized;
        // current_projectile.Initialize(transform.position, ProjectileVelocity, this);
        PhotonInstantiateShoot();
    }

    private void PhotonInstantiateShoot()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView view = GetComponent<PhotonView>();
        GameObject projectileObject = PhotonNetwork.Instantiate(ProjectilePrefab.name, transform.position, Quaternion.identity);
        Vector3 ProjectileVelocity = ProjectileSpeed * (TargetPos - transform.position).normalized;
        if (projectileObject.TryGetComponent<ShootingEnemyProjectile>(out ShootingEnemyProjectile CurrentProjectile))
        {
            CurrentProjectile.Initialize(transform.position, ProjectileVelocity, this);
        }
    }

}
