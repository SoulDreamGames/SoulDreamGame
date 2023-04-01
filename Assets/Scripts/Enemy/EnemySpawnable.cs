using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemySpawnable : MonoBehaviour
{
    public abstract void Initialize(EnemiesManager enemiesManager, GameObject defaultTarget);
}