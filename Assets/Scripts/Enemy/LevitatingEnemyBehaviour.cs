using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class LevitatingEnemyBehaviour : EnemyBehaviour
{
    public float Mass = 1;
    public float AccelerationTime = 1; // Proportional on how long it takes to get to maximum speed
    public float MaxSpeed = 0.1f; // Maximum speed towards a target
    public float HoverHeight = 1.5f; // Minimum height over the ground
    [SerializeField] protected LayerMask WallsMask;
    [SerializeField] protected LayerMask TargetMask;

    protected Vector3 Velocity;
    protected Rigidbody RB;

    protected float TargetForce; // Constant which regulates the atractive force between the target and the enemy
    protected float Drag; // Drag coefficient
    protected Vector3 ExternalForces; // A sum of external forces

    protected float DefaultTargetChangeDistance = 20.0f; // Change default target when near the current one
    protected float TooFarAwayDistance = 100.0f;
    protected int TooFarAwayCounter = 0;

    private PhotonView _view;

    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        base.Initialize(enemiesManager, defaultTarget);
        
        RB = GetComponent<Rigidbody>();
        _view = GetComponent<PhotonView>();
        Velocity = new Vector3(0,0,0);

        Drag = Mass / AccelerationTime;
        TargetForce = Drag * MaxSpeed;
        if (_Target == null) {
            startLookingForTargets();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (_Target == null)  ChangeToDefaultTarget();

        if (!LookingForTargets && _Target == null)
            Debug.LogError("ChangeToDefault target does not work on this enemy: "+this.name);
        
        if (!LookingForTargets){
            FollowTarget(_Target);
            // Leave chase if the target is too far away for too long
            if ((_Target.transform.position - transform.position).magnitude >= TooFarAwayDistance) 
            {
                TooFarAwayCounter++;
                const float FixedUpdateFPS = 50.0f;
                const float GiveUpTime = 5.0f; // seconds
                if (TooFarAwayCounter >= FixedUpdateFPS * GiveUpTime) 
                {
                    if (_Target.TryGetComponent<NPCRandomNavMesh>(out NPCRandomNavMesh npc))
                    {
                        npc._enemyFollowing = null;
                        npc.isTargeted = false;
                    }
                    _Target = null;
                    ChangeToDefaultTarget();
                }
            }
            else TooFarAwayCounter = 0;
        }
        else {
            FollowTarget(_DefaultTarget);
            // Change default target when close (keep the enemies moving)
            if ((_DefaultTarget.transform.position - transform.position).magnitude <= DefaultTargetChangeDistance)
            {
                // Enemy has reached  the default destination
                _EnemiesManager.GetNewDefaultTarget(ref _DefaultTarget);
            }
        }
    }
    public virtual void FollowTarget(GameObject current_target) {
        
        Vector3 target_pos = current_target.transform.position;

        Vector3 repulsion = targetRay(target_pos);
        repulsion += groundRay(HoverHeight);

        Vector3 AtraccionForce = TargetForce * (target_pos - transform.position).normalized;
        Vector3 acceleration = (repulsion + AtraccionForce - Drag * Velocity + ExternalForces) / Mass;

        // Euler integration
        Velocity = Velocity + acceleration;

        if (RB == null) {
            Debug.LogError("Levitating Enemy Behaviour: rigid body RB is null");
        }

        Vector3 position = RB.position + Velocity;
        transform.position = position;
        transform.LookAt(target_pos, Vector3.up);

        ExternalForces = ExternalForces * 0;
    }
    protected Vector3 targetRay(Vector3 target_pos){
        RaycastHit hit;
        const float maxdist = 1000000000;
        Vector3 repulsion = new Vector3(0,0,0);

        Vector3 HoverOffset = new Vector3(0, HoverHeight * 0.6f, 0);
        Vector3 RayOrigin = transform.position - HoverOffset;
        Vector3 RayDirection = (target_pos - RayOrigin).normalized;
        Debug.DrawRay(RayOrigin, 100 * RayDirection, Color.magenta);

        if (Physics.Raycast(RayOrigin, RayDirection, out hit, maxdist, WallsMask))
        {
            const float ThresholdAngle = 0.1f;
            if (1.0f - Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < ThresholdAngle)
            { 
                // Debug.Log("Pointing to the ground");
                return new Vector3(0.0f, 0.0f, 0.0f);
            }
            Vector3 center = hit.collider.bounds.center;
            Vector3 ColliderSize = hit.collider.bounds.extents;
            Vector3 collision_point = hit.point;
            Vector3 offset = collision_point - center;
            offset.y = Mathf.Abs(offset.y) * 4.0f;
            const float DodgeForce = 20.0f;

            repulsion =  DodgeForce * offset.normalized / (hit.distance * hit.distance);
        }
        return repulsion;
    }

    protected Vector3 groundRay(float MinDist) {
        Vector3 repulsion = new Vector3(0,0,0);
        RaycastHit hit;
        Vector3 direction = - Vector3.up;

        float MaxDist = MinDist;
        float GroundRepulsionCoeff = 2.0f;

        Debug.DrawRay(transform.position, MaxDist * direction, Color.magenta);

        if (Physics.Raycast(transform.position, direction, out hit, MaxDist, WallsMask)) {
            float y = transform.position.y - hit.point.y;
            repulsion = GroundRepulsionCoeff * (2 * (y - MaxDist) * Mathf.Log(y / MaxDist) + (y - MaxDist) * (y - MaxDist) * MaxDist / y) * Vector3.up;
        }

        return repulsion;
    }

    public virtual void OnTriggerEnter(Collider collider)
    {
        const float contact_repulsion_kik = 10.0f;
        if ((TargetMask.value & (1 << collider.gameObject.layer)) > 0) {
            Vector3 direction = Vector3.Normalize(transform.position - collider.gameObject.transform.position);
            ExternalForces += contact_repulsion_kik * direction;


            if (collider.gameObject.TryGetComponent<NPCRandomNavMesh>(out NPCRandomNavMesh npc))
            {
                npc.life -= 0.1f;
                if (npc.life <= 0.0f) { NotifyHasEatenSomeone(collider.gameObject);  }
            }
        }
    }

    public void addExternalForce(Vector3 eforce) { ExternalForces += eforce; }
    public override void NotifyHasEatenSomeone(GameObject someone)
    {
        if (someone == _Target) {
            ChangeToDefaultTarget();
        }
    }

    public override bool ReceiveDamage(int damage)
    {
        Hitpoints -= damage;
        bool died = (Hitpoints <= 0);
        if (died) OnDeath();
        return died;
    }

    public void setTarget(GameObject newTarget) {
        _Target = newTarget;
        stopLookingForTargets();
    }
    public void setDefaultTarget(GameObject newDefaultTarget) {
        _DefaultTarget = newDefaultTarget;
        startLookingForTargets();
    }

    public override void ChangeToDefaultTarget()
    {
        startLookingForTargets();
    }

}
