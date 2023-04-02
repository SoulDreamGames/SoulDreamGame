using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class EnemyBehaviour : EnemySpawnable
{
    public GameObject _Target;
    public GameObject _DefaultTarget;
    public float ContactDamage = 10.0f;
    [SerializeField] protected EnemiesManager _EnemiesManager;
    [SerializeField] protected int Hitpoints = 1;
    private PhotonView _view;
    protected bool LookingForTargets = true;
    public bool isLookingForTargets() { return LookingForTargets; }
    public virtual void stopLookingForTargets() { LookingForTargets = false; }
    public virtual void startLookingForTargets() { LookingForTargets = true; }
    public abstract void ChangeToDefaultTarget();
    private Animator animator;
    
    void Start()
    {
        if (PhotonNetwork.IsMasterClient) return;

        _EnemiesManager = FindObjectOfType<EnemiesManager>();
        _EnemiesManager.AddSpawnedEnemy(this);

        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }
    
    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        Debug.Log("Assigning default target");
        
        _view = GetComponent<PhotonView>();
        _DefaultTarget = defaultTarget;
        _EnemiesManager = enemiesManager;
        _EnemiesManager.AddSpawnedEnemy(this);

        if (PhotonNetwork.IsMasterClient) 
        {
            _view.RPC("SetScale", RpcTarget.All, UnityEngine.Random.Range(1.0f, 2.0f));
        }
        // SetScale(UnityEngine.Random.Range(1.0f, 2.0f));
        startLookingForTargets();
    }
    
    [PunRPC]
    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }
    /* Returns true if the enemy died with the damage done */
    public virtual bool ReceiveDamage(int damage)
    {
        animator.SetTrigger("Injured");
        return false;
    }

    /* Notifies to the game manager that "someone" has died -> player / NPC */
    public abstract void NotifyHasEatenSomeone(GameObject someone);

    protected virtual void OnDeath()
    {
        // Destroy(gameObject);

        if (PhotonNetwork.IsMasterClient) 
        {
            PhotonView view = GetComponent<PhotonView>();
            PhotonNetwork.Destroy(view);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("On destroy enemy");

        if (_EnemiesManager == null)
            Debug.LogError("This enemy does not have enemy manager reference " + this.name);
        _EnemiesManager.EnemyKilled(this);
        
        if (_Target == null) return;

        if (_Target.TryGetComponent(out NPCRandomNavMesh npc))
        {
            npc.isTargeted = false;
            npc._enemyFollowing = null;
        }
    }

}
