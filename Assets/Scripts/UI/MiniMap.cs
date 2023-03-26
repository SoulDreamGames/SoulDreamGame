using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    public float size;
    public Material minimapMaterial;
    public Transform playerPosition;
    [SerializeField] private Transform mapCenter;

    //ToDo: pass player object at spawn instead of serialized field
    private void Update()
    {
        //align the player icon to the player rotation; i do - and +90 because my game north is not on 0 degrees
        playerIcon.rectTransform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        //size is the value of the orthographic size of the camera you toke the screenshot with. *2 because the orthograpic size represents half the height of the screen.
        Vector3 mapPos = playerPosition.position - mapCenter.position;
        mapPos = new Vector3(-mapPos.z, mapPos.y, mapPos.x);

        float _aspectRatio = Screen.width / Screen.height;
        minimapMaterial.SetTextureScale("_MainTex", new Vector2(1/_aspectRatio, 1f));
        minimapMaterial.SetTextureOffset("_MainTex", new Vector2(0.5f * (1 - (1/_aspectRatio)), 0f));
        minimapMaterial.SetVector("_PlayerPos", mapPos / (size * 2f));
    }
}
