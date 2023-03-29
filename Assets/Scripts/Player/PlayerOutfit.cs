using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerOutfit : MonoBehaviourPunCallbacks
{
    private PhotonView _view;

    private void Awake()
    {
        _view = GetComponent<PhotonView>();
    }

    void Start()
    {
        Color skinColor = PlayerPrefsX.GetColor("SkinColor");
        Vector3 skinColorV3 = new Vector4(skinColor.r, skinColor.g, skinColor.b);
        
        object[] attributes = new object[]
        {
            skinColorV3,                                    //Skin color
            PlayerPrefs.GetInt("Trail", 0)     //Trail color
        };
        _view.RPC("UpdateOutfit", RpcTarget.All, attributes);
    }

    [PunRPC]
    void UpdateOutfit(object[] attributes)
    {
        //ToDo: Sync this by applying to skin and trail + spawn correct playerPrefab with skin
        string username = _view.Owner.NickName;
        Debug.Log("Nickname: " + username);

        //Color
        Vector3 skinColorv3 = (Vector3)attributes[0];
        Color skinColor = new Color(skinColorv3.x, skinColorv3.y, skinColorv3.z, 1.0f);
        Debug.Log("Skin color synced is: " + skinColor);
        
        //ToDo: check if -1, then dont apply anything
        
        //Trail skin
        int trailSkin = (int)attributes[1];
        Debug.Log("Trail skin is: " + trailSkin);

    }
}
