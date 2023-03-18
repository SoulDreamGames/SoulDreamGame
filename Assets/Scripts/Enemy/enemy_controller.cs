using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_controller : MonoBehaviour
{
    public GameObject target;
    public float mass = 1;
    public float acceleration_time = 1;
    public float max_vel = 0.1f;
    public float hover_height;
    public LayerMask walls_mask;
    public LayerMask target_mask;

    private Vector3 velocity;

    private Rigidbody RB;

    private float target_force;
    private float drag;
    private bool lookingForTargets = true;
    private Vector3 external_forces;
    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        velocity = new Vector3(0,0,0);
        drag = mass / acceleration_time;
        target_force = drag * max_vel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        if (!lookingForTargets){
            followTarget();
        }

        else {
            lookForTargets();
            followTarget();
        }
    }
    void lookForTargets() {
    }
    void followTarget() {
        Vector3 target_pos = target.transform.position;

        Vector3 repulsion = targetRay(target_pos);
        repulsion += groundRay(hover_height);

        Vector3 acceleration = 1 / mass *(repulsion + target_force * (target_pos - transform.position).normalized - drag * velocity + external_forces);

        // Euler integration
        velocity += acceleration;
        Vector3 position = RB.position + velocity;
        RB.MovePosition(position);
        transform.LookAt(target_pos, Vector3.up);

        external_forces = external_forces * 0;
    }
    Vector3 targetRay(Vector3 target_pos){
        RaycastHit hit;
        Vector3 direction = (target_pos - transform.position).normalized;
        const float maxdist = 1000000000;
        Vector3 repulsion = new Vector3(0,0,0);

        Debug.DrawRay(transform.position, 100*direction, Color.magenta);

        if (Physics.Raycast(transform.position, direction, out hit, maxdist, walls_mask)){
            Vector3 center = hit.collider.bounds.center;
            Vector3 collision_point = hit.point;
            Vector3 offset = collision_point - center;
            offset.y = Mathf.Abs(offset.y);
            repulsion = 0.1f * offset / hit.distance;
        }
        return repulsion;
    }

    Vector3 groundRay(float min_dist) {
        Vector3 repulsion = new Vector3(0,0,0);
        RaycastHit hit;
        Vector3 direction = - Vector3.up;

        float maxdist = min_dist;
        float ground_repulsion_coeff = 1;

        Debug.DrawRay(transform.position, 100 * direction, Color.magenta);

        if (Physics.Raycast(transform.position, direction, out hit, maxdist, walls_mask)) {
            float y = transform.position.y - hit.point.y;
            repulsion = ground_repulsion_coeff * (2 * (y - maxdist) * Mathf.Log(y / maxdist) + (y - maxdist) * (y - maxdist) * maxdist / y) * Vector3.up;
        }

        return repulsion;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("heloooooooooooooooooooooooooo");
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
    }
    public bool isLookingForTargets() { return lookingForTargets; }
    public void stopLookingForTargets() { lookingForTargets = false; }
    public void startLookingForTargets() { lookingForTargets = true; }

    public void addExternalForce(Vector3 eforce) { external_forces += eforce; }

}
