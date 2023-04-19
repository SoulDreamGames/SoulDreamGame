using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BloodSplashScript : MonoBehaviour
{
    public bool IsFromPool = false;
    private PhotonView View;
    private int counter = 0;
    private ParticleSystem ps;
    void Start()
    {
        View = GetComponent<PhotonView>();
        ps = GetComponent<ParticleSystem>();
    }

    void FixedUpdate()
    {
        if(!View.IsMine) return;
        if (IsFromPool) return;

        counter++;
        const float FixedUpdateFPS = 49.0f;

        if (counter / FixedUpdateFPS > 1.0f) 
        {
            PhotonNetwork.Destroy(View);
        }
    }
    
    public void StartAnimation()
    {
        ps.Clear();
        ps.Play();
    }
}
