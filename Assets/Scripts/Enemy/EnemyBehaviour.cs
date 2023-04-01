using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBehaviour : EnemySpawnable
{
    public GameObject _Target;
    public GameObject _DefaultTarget;
    [SerializeField] protected EnemiesManager _EnemiesManager;
    [SerializeField] protected int Hitpoints = 1;


    void Start()
    {
        _EnemiesManager._enemiesSpawned.Add(this);
    }

    protected bool LookingForTargets = true;
    public bool isLookingForTargets() { return LookingForTargets; }
    public virtual void stopLookingForTargets() { LookingForTargets = false; }
    public virtual void startLookingForTargets() { LookingForTargets = true; }
    public abstract void ChangeToDefaultTarget();

    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        Debug.Log("Assigning default target");
        _EnemiesManager = enemiesManager;
        _DefaultTarget = defaultTarget;
        enemiesManager.AddSpawnedEnemy(this);
        startLookingForTargets();
    }

    /* Returns true if the enemy died with the damage done */
    public abstract bool ReceiveDamage(int damage);

    /* Notifies to the game manager that "someone" has died -> player / NPC */
    public abstract void NotifyHasEatenSomeone(GameObject someone);

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _EnemiesManager.EnemyKilled(this);

        if (_Target == null) return;

        if (_Target.TryGetComponent(out NPCRandomNavMesh npc))
        {
            npc.isTargeted = false;
            npc._enemyFollowing = null;
        }
    }

}
