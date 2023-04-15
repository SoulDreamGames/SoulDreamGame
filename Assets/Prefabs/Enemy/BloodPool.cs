using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BloodPool : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ObjectPrefab;
    public int PoolAmount;
    public List<GameObject> PooledObjects;
    private int LastInitialized = 0;
    void Start()
    {
        for (int i = 0; i < PoolAmount; i++)
        {
            GameObject temp = PhotonNetwork.Instantiate(ObjectPrefab.name, Vector3.zero, Quaternion.identity);
            PooledObjects.Add(temp);
        }
    }

    public void Instantiate(Vector3 position)
    {
        LastInitialized++;
        if (LastInitialized >= PooledObjects.Count) LastInitialized = 0;

        PooledObjects[LastInitialized].transform.position = position;
        BloodSplashScript blood = PooledObjects[LastInitialized].GetComponent<BloodSplashScript>();
        blood.StartAnimation();
    }
}
