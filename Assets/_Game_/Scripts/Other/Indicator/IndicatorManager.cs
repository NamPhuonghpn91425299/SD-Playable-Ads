using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera playerCamera;
    public Canvas uiCanvas;
    
    [Header("Prefabs")]
    public GameObject indicatorPrefab; // Prefab chứa Image mũi tên
    
    [Header("Settings")]
    public float checkInterval = 0.1f; // Kiểm tra mỗi 0.1 giây
    
    private Dictionary<Transform, GameObject> activeIndicators = new Dictionary<Transform, GameObject>();
    [SerializeField]private Transform[] enemies;
    private float lastCheckTime;
    
    void Start()
    {
        // Tìm tất cả enemies trong scene
        RefreshEnemyList();
    }
    
    void Update()
    {
        // Kiểm tra với interval để tối ưu performance
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckEnemiesVisibility();
            lastCheckTime = Time.time;
        }
    }
    
    void RefreshEnemyList()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemies = new Transform[enemyObjects.Length];
        
        for (int i = 0; i < enemyObjects.Length; i++)
        {
            enemies[i] = enemyObjects[i].transform;
        }
    }
    
    void CheckEnemiesVisibility()
    {
        foreach (Transform enemy in enemies)
        {
            if (enemy == null)
            {
                // Enemy đã bị destroy, remove indicator nếu có
                if (activeIndicators.ContainsKey(enemy))
                {
                    Destroy(activeIndicators[enemy]);
                    activeIndicators.Remove(enemy);
                }
                continue;
            }
            
            bool isVisible = IsEnemyVisible(enemy);
            bool hasIndicator = activeIndicators.ContainsKey(enemy);
            
            if (!isVisible && !hasIndicator)
            {
                // Enemy không nhìn thấy và chưa có indicator -> Spawn indicator
                SpawnIndicator(enemy);
            }
            else if (isVisible && hasIndicator)
            {
                // Enemy nhìn thấy và có indicator -> Destroy indicator
                DestroyIndicator(enemy);
            }
        }
    }
    
    bool IsEnemyVisible(Transform enemy)
    {
        Vector3 screenPoint = playerCamera.WorldToViewportPoint(enemy.position);
        
        bool onScreen = screenPoint.z > 0 && 
                       screenPoint.x > 0 && screenPoint.x < 1 && 
                       screenPoint.y > 0 && screenPoint.y < 1;
        
        return onScreen;
    }
    
    void SpawnIndicator(Transform enemy)
    {
        GameObject indicatorObj = Instantiate(indicatorPrefab, uiCanvas.transform);
        
        // Setup indicator component
        Indicator indicator = indicatorObj.GetComponent<Indicator>();
        if (indicator == null)
        {
            indicator = indicatorObj.AddComponent<Indicator>();
        }
        
        // Gán references
        indicator.targetEnemy = enemy;
        indicator.player = player;
        indicator.playerCamera = playerCamera;
        indicator.arrowUI = indicatorObj.GetComponent<RectTransform>();
        indicator.uiCanvas = uiCanvas;
        
        // Thêm vào dictionary
        activeIndicators[enemy] = indicatorObj;
        
        Debug.Log($"Spawned indicator for {enemy.name}");
    }
    
    void DestroyIndicator(Transform enemy)
    {
        if (activeIndicators.ContainsKey(enemy))
        {
            Destroy(activeIndicators[enemy]);
            activeIndicators.Remove(enemy);
            
            Debug.Log($"Destroyed indicator for {enemy.name}");
        }
    }
    
    // Gọi hàm này khi có enemy mới spawn vào game
    public void AddNewEnemy(Transform newEnemy)
    {
        RefreshEnemyList();
    }
    
    // Gọi khi enemy bị destroy
    public void RemoveEnemy(Transform enemy)
    {
        if (activeIndicators.ContainsKey(enemy))
        {
            DestroyIndicator(enemy);
        }
        RefreshEnemyList();
    }
    
    void OnDestroy()
    {
        // Clean up tất cả indicators
        foreach (var indicator in activeIndicators.Values)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        activeIndicators.Clear();
    }
}
