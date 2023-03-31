using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatingEnemySwarm : EnemySwarm
{
    [SerializeField] private Mesh DuplicatingEnemyMesh;
    [SerializeField] private Material Material;
    [SerializeField] private DuplicatingEnemyEntity SwarmPrefab;
    [SerializeField] private float DuplicatingTimeDelay;
    private Vector3 SwarmPosition;
    private int framecout = 0;
    private void RenderAll()
    {
        /* Instancing of the enemies */
        List<Matrix4x4> matrices = new List<Matrix4x4>();
        foreach(LevitatingEnemyBehaviour enemy in swarmMembers) {
            matrices.Add(enemy.transform.localToWorldMatrix);
        }
        Graphics.DrawMeshInstanced(DuplicatingEnemyMesh, 0, Material, matrices);
    }

    // Start is called before the first frame update
    void Start()
    {
        forceCreateNewMember(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // RenderAll();
    }
    void FixedUpdate() {
        apply_swarm_force();
        updateTargets();

        framecout++;
    }
    public void MemberDied(DuplicatingEnemyEntity member)
    {
        swarmMembers.Remove(member);
        Destroy(member);
    }
    
    private void forceCreateNewMember(Vector3 position) {
        framecout = 10000000;
        createNewMember(position);
    }
    public void createNewMember(Vector3 position) {
        /* Creates a new member if the delay has passed */
        float FixedUpdateFPS = 50.0f;
        if (framecout / FixedUpdateFPS  < DuplicatingTimeDelay) return;
        framecout = 0;
        DuplicatingEnemyEntity newMember = Instantiate<DuplicatingEnemyEntity>(SwarmPrefab, position, Quaternion.identity, transform);
        newMember.Initialize(_enemy_manager, swarm_default_target, this);

        swarmMembers.Add(newMember);

        num_enemies = (uint) swarmMembers.Count;
    }
}

