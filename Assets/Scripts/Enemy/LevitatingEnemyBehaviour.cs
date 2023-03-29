using System;
using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        Velocity = new Vector3(0,0,0);

        Drag = Mass / AccelerationTime;
        TargetForce = Drag * MaxSpeed;
        if (_Target == null) {
            startLookingForTargets();
        }
    }

    public void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        //ToDo: add other variables to initialize
        _EnemiesManager = enemiesManager;
        _DefaultTarget = defaultTarget;

        Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void FixedUpdate() {
        if (!LookingForTargets){
            FollowTarget(_Target);
        }

        else {
            FollowTarget(_DefaultTarget);
        }
    }
    public virtual void FollowTarget(GameObject current_target) {
        Vector3 target_pos = current_target.transform.position;

        Vector3 repulsion = targetRay(target_pos);
        repulsion += groundRay(HoverHeight);

        Vector3 acceleration = (repulsion + TargetForce * (target_pos - transform.position).normalized - Drag * Velocity + ExternalForces) / Mass;

        // Euler integration
        Velocity = Velocity + acceleration;

        if (RB == null) {
            Debug.LogError("Levitating Enemy Behaviour: rigid body RB is null");
        }

        Vector3 position = RB.position + Velocity;
        RB.MovePosition(position);
        transform.LookAt(target_pos, Vector3.up);

        ExternalForces = ExternalForces * 0;
    }
    protected Vector3 targetRay(Vector3 target_pos){
        RaycastHit hit;
        const float maxdist = 1000000000;
        Vector3 repulsion = new Vector3(0,0,0);

        Vector3 hover_offset = new Vector3(0, HoverHeight, 0);
        Vector3 ray_origin = transform.position - hover_offset;
        Vector3 direction = (target_pos - ray_origin).normalized;
        Debug.DrawRay(ray_origin, 100 * direction, Color.magenta);

        if (Physics.Raycast(ray_origin, direction, out hit, maxdist, WallsMask)){
            Vector3 center = hit.collider.bounds.center;
            Vector3 collision_point = hit.point;
            Vector3 offset = collision_point - center;
            offset.y = Mathf.Abs(offset.y);
            repulsion = 0.1f * offset / hit.distance;
        }
        return repulsion;
    }

    protected Vector3 groundRay(float min_dist) {
        Vector3 repulsion = new Vector3(0,0,0);
        RaycastHit hit;
        Vector3 direction = - Vector3.up;

        float maxdist = min_dist;
        float ground_repulsion_coeff = 1;

        Debug.DrawRay(transform.position, 100 * direction, Color.magenta);

        if (Physics.Raycast(transform.position, direction, out hit, maxdist, WallsMask)) {
            float y = transform.position.y - hit.point.y;
            repulsion = ground_repulsion_coeff * (2 * (y - maxdist) * Mathf.Log(y / maxdist) + (y - maxdist) * (y - maxdist) * maxdist / y) * Vector3.up;
        }

        return repulsion;
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        const float contact_repulsion_kik = 1.0f;
        if ((TargetMask.value & (1 << collision.gameObject.transform.gameObject.layer)) > 0) {
            Vector3 direction = Vector3.Normalize(transform.position - collision.gameObject.transform.position);
            ExternalForces += contact_repulsion_kik * direction;
        }
    }
    public void addExternalForce(Vector3 eforce) { ExternalForces += eforce; }
    public override void NotifyHasEatenSomeone(GameObject someone)
    {
        if (someone == _Target) {
            startLookingForTargets();
        }
    }
    public override bool RecieveDamage(int damage)
    {
        Hitpoints -= damage;
        return (Hitpoints <= 0);
    }

    public void setTarget(GameObject newTarget) {
        _Target = newTarget;
        stopLookingForTargets();
    }
    public void setDefaultTarget(GameObject newDefaultTarget) {
        _DefaultTarget = newDefaultTarget;
        startLookingForTargets();
    }
}
