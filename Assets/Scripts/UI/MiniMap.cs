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

    [SerializeField] private Transform enemy;
    [SerializeField] private Image enemyIcon;
    [SerializeField] private float realWorldScale = 60f;
    [SerializeField] private float minimapDistance = 90f;
    
    private void Update()
    {
        //align the player icon to the player rotation; i do - and +90 because my game north is not on 0 degrees
        playerIcon.rectTransform.rotation = Quaternion.Euler(0f,0f,-playerPosition.eulerAngles.y);
        //size is the value of the orthographic size of the camera you toke the screenshot with. *2 because the orthograpic size represents half the height of the screen.
        Vector3 mapPos = playerPosition.position - mapCenter.position;
        mapPos = new Vector3(-mapPos.z, mapPos.y, mapPos.x);

        float _aspectRatio = Screen.width / Screen.height;
        minimapMaterial.SetTextureScale("_MainTex", new Vector2(1/_aspectRatio, 1f));
        minimapMaterial.SetTextureOffset("_MainTex", new Vector2(0.5f * (1 - (1/_aspectRatio)), 0f));
        minimapMaterial.SetVector("_PlayerPos", mapPos / (size * 2f));

        UpdateNearestEnemy();
    }

    void UpdateNearestEnemy()
    {
        //ToDo: ask for nearest enemy on enemymanager list
        //ToDo: reset this with the real correspondence
        
        Vector3 enemyMapPos = enemy.position - playerPosition.position;
        
        //if enemy outside map area
        if (enemyMapPos.magnitude > realWorldScale)
        {
            Vector3 enemyPos = enemyMapPos.normalized;
            enemyPos = new Vector3(enemyPos.x, enemyPos.z, enemyPos.y) * minimapDistance;
            enemyIcon.rectTransform.localPosition = playerIcon.rectTransform.localPosition + enemyPos;
            return;
            
        }

        //If enemy inside map area
        enemyMapPos = new Vector3(enemyMapPos.x, enemyMapPos.z, enemyMapPos.y);
        enemyIcon.rectTransform.localPosition = enemyMapPos / realWorldScale * minimapDistance;
    }
}
