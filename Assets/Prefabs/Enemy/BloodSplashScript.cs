using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BloodSplashScript : MonoBehaviour
{
    // Start is called before the first frame update
    PhotonView View;
    private int counter = 0;
    private ParticleSystem ps;
    void Start()
    {
        View = GetComponent<PhotonView>();
        ps = GetComponent<ParticleSystem>();
    }

    void FixedUpdate()
    {
        // counter++;
        // const float FixedUpdateFPS = 50.0f;

        // if (counter / FixedUpdateFPS > 2.0f) 
        // {
            // PhotonNetwork.Destroy(View);
        // }
    }
    
    public void StartAnimation()
    {
        ps.Clear();
        ps.Play();
    }
}
