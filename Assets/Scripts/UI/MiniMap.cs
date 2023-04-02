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

    //[SerializeField] private Transform enemy;
    [SerializeField] private Image enemyIcon;
    [SerializeField] private float realWorldScale = 60f;
    [SerializeField] private float minimapDistance = 90f;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        if (!_gameManager)
        {
            enemyIcon.enabled = false;
            return;
        }

        if (!_gameManager.targetableEnemy)
        {
            enemyIcon.enabled = false;
        }
    }
    private void Update()
    {
        playerIcon.rectTransform.rotation = Quaternion.Euler(0f,0f,-playerPosition.eulerAngles.y);
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
        if (!_gameManager) return;

        var nearestEnemy = _gameManager.nearestEnemy;
        if (nearestEnemy == null) return;
        
        enemyIcon.enabled = true;
        Vector3 enemyMapPos = nearestEnemy.transform.position - playerPosition.position;
        
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
