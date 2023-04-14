using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class EnemyBehaviour : EnemySpawnable, IPunObservable
{
    public GameObject _Target;
    public GameObject _DefaultTarget;
    public float ContactDamage = 10.0f;
    [SerializeField] protected EnemiesManager _EnemiesManager;
    [SerializeField] protected int Hitpoints = 1;
    protected PhotonView View;
    protected bool LookingForTargets = true;
    protected Animator _animator;


    public bool isLookingForTargets()
    {
        return LookingForTargets;
    }

    public virtual void stopLookingForTargets()
    {
        LookingForTargets = false;
    }

    public virtual void startLookingForTargets()
    {
        LookingForTargets = true;
    }

    public abstract void ChangeToDefaultTarget();

    //Awake
    void Awake()
    {
        View = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
    }

    //Initialize - called on master client
    public override void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget)
    {
        Debug.Log("Assigning default target");

        _DefaultTarget = defaultTarget;
        _EnemiesManager = enemiesManager;
        _EnemiesManager.AddSpawnedEnemy(this);

        View.RPC("InitializeOnClientRPC", RpcTarget.AllBuffered);

        if (PhotonNetwork.IsMasterClient)
        {
            View.RPC("SetScale", RpcTarget.All, UnityEngine.Random.Range(1.0f, 2.0f));
        }

        startLookingForTargets();
    }

    [PunRPC]
    public void InitializeOnClientRPC()
    {
        if (PhotonNetwork.IsMasterClient) return;

        _EnemiesManager = FindObjectOfType<EnemiesManager>();
        _EnemiesManager.AddSpawnedEnemy(this);

        if (_animator != null)
        {
            _animator.SetBool("isMoving", true);
        }
    }

    [PunRPC]
    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }


    /* Returns true if the enemy died with the damage done */
    public virtual void ReceiveDamage(int damage)
    {
        if (_animator)
            _animator.SetTrigger("Injured");
    }

    /* Notifies to the game manager that "someone" has died -> player / NPC */
    public abstract void NotifyHasEatenSomeone(GameObject someone);

    protected virtual void OnDeath()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        InstantiateBloodHeare(transform.position);
        PhotonNetwork.Destroy(View);
    }

    private void OnDestroy()
    {
        Debug.Log("On destroy enemy");
        if (_EnemiesManager)
            _EnemiesManager.EnemyKilled(this);

        if (!_Target ) return;
        if (!_Target.TryGetComponent(out NPCRandomNavMesh npc)) return;
        npc.isTargeted = false;
        npc._enemyFollowing = null;
        Debug.Log("Removed NPC target");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Hitpoints);
        }
        else if (stream.IsReading)
        {
            Hitpoints = (int)stream.ReceiveNext();
        }
    }

    public void InstantiateBloodHeare(Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log("Instancing bloood!");
        PhotonNetwork.Instantiate("BloodPS", position, Quaternion.identity);
    }
}