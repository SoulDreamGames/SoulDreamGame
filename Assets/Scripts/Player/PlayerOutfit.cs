using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaterialChanger))]
public class PlayerOutfit : MonoBehaviourPunCallbacks
{
    private PhotonView _view;
    private MaterialChanger _materialChanger;
    [SerializeField] private Text usernameText;
    private Camera _mainCamera;

    private void Awake()
    {
        _view = GetComponent<PhotonView>();
        _materialChanger = GetComponent<MaterialChanger>();
        _mainCamera = Camera.main;
    }

    void Start()
    {
        if (!_view.IsMine) return;
        
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
        string username = _view.Owner.NickName;
        usernameText.text = username;
        Debug.Log("Nickname: " + username);

        //Color
        Vector3 skinColorv3 = (Vector3)attributes[0];
        Color skinColor = new Color(skinColorv3.x, skinColorv3.y, skinColorv3.z, 1.0f);
        if (skinColor.r >= 0.0f)
        {
            _materialChanger.SetMaterialColor(skinColor);
        }

        //Trail skin
        int trailSkin = (int)attributes[1];
        //ToDo: apply to skin trail
        Debug.Log("Trail skin is: " + trailSkin);
    }

    private void Update()
    {
        usernameText.gameObject.transform.forward =
            (usernameText.gameObject.transform.position - _mainCamera.transform.position).normalized;
    }
}
