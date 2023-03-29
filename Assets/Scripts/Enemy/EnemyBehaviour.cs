using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBehaviour : MonoBehaviour
{
    public GameObject _Target;
    public GameObject _DefaultTarget;
    protected EnemiesManager _EnemiesManager;
    [SerializeField] protected int Hitpoints = 1;
    protected bool LookingForTargets = true;
    public bool isLookingForTargets() { return LookingForTargets; }
    public void stopLookingForTargets() { LookingForTargets = false; }
    public void startLookingForTargets() { LookingForTargets = true; }

    /* Returns true if the enemy died with the damage done */
    public abstract bool RecieveDamage(int damage);

    /* Notifies to the game manager that "someone" has died -> player / NPC */
    public abstract void NotifyHasEatenSomeone(GameObject someone);

    private void OnDestroy()
    {
        _EnemiesManager.EnemyKilled(this);
    }

}
