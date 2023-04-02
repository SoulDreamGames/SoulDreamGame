using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DuplicatingEnemySwarm : EnemySwarm
{
    [SerializeField] private Mesh DuplicatingEnemyMesh;
    [SerializeField] private Material Material;
    [SerializeField] private DuplicatingEnemyEntity SwarmPrefab;
    [SerializeField] private float DuplicatingTimeDelay;
    [SerializeField] private int MaxNumMembers = 10;
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

    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        base.Initialize(enemiesManager, defaultTarget);
        forceCreateNewMember(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // RenderAll();
    }
    void FixedUpdate() {
        if (!PhotonNetwork.IsMasterClient) return;

        apply_swarm_force();
        updateTargets();

        framecout++;
    }
    public void MemberDied(DuplicatingEnemyEntity member)
    {
        swarmMembers.Remove(member);
    }
    
    private void forceCreateNewMember(Vector3 position) {
        framecout = 10000000;
        createNewMember(position);
    }
    public void createNewMember(Vector3 position) {
        /* Creates a new member if the delay has passed */
        float FixedUpdateFPS = 50.0f;
        if (framecout / FixedUpdateFPS  < DuplicatingTimeDelay) return;
        if (swarmMembers.Count == MaxNumMembers) return;
        framecout = 0;
        // DuplicatingEnemyEntity newMember = Instantiate<DuplicatingEnemyEntity>(SwarmPrefab, position, Quaternion.identity, transform);
        // newMember.Initialize(_enemy_manager, swarm_default_target, this);

        // swarmMembers.Add(newMember);
        PhotonInstantiateNewMember(position);

    }

    public void ChangeToDefaultTarget()
    {
        if (!SwarmHasActiveTarget) return;

        foreach(LevitatingEnemyBehaviour enemy in swarmMembers)
        {
            enemy._Target = null;
            enemy._DefaultTarget = SwarmDefaultTarget;
            enemy.startLookingForTargets();
            _SwarmTarget = null;
            SwarmHasActiveTarget = false;
        }
    }

    private void PhotonInstantiateNewMember(Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("Creating new duplicating member");
        GameObject memberObject = PhotonNetwork.Instantiate(SwarmPrefab.name, position, Quaternion.identity);
        if (memberObject.TryGetComponent<DuplicatingEnemyEntity>(out DuplicatingEnemyEntity NewDuplicatingEntity))
        {
            NewDuplicatingEntity.Initialize(_EnemyManager, SwarmDefaultTarget, this);
            swarmMembers.Add(NewDuplicatingEntity);
        }
        else
        {
            Debug.LogError("Duplicating Enemy Entity prefab is not correct");
        }

    }
}

