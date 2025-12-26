using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    [Header("References")]
    public Transform targetEnemy;
    public Transform player;
    public Camera playerCamera;
    public RectTransform arrowUI;
    public Canvas uiCanvas;
    
    [Header("Settings")]
    public float borderOffset = 50f;
    public float rotationSpeed = 10f;
    public float fadeSpeed = 5f;
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        canvasGroup = arrowUI.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = arrowUI.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Bắt đầu với alpha = 0
        canvasGroup.alpha = 0f;
    }
    
    void Update()
    {
        if (targetEnemy == null)
        {
            DestroyIndicator();
            return;
        }
        
        bool isVisible = IsEnemyVisible();
        
        if (!isVisible)
        {
            ShowIndicator();
            UpdateIndicatorPosition();
        }
        else
        {
            HideIndicator();
        }
    }
    
    bool IsEnemyVisible()
    {
        Vector3 screenPoint = playerCamera.WorldToViewportPoint(targetEnemy.position);
        
        // Kiểm tra xem enemy có trong viewport không
        bool onScreen = screenPoint.z > 0 && 
                       screenPoint.x > 0 && screenPoint.x < 1 && 
                       screenPoint.y > 0 && screenPoint.y < 1;
        
        return onScreen;
    }
    
    void ShowIndicator()
    {
        // Fade in
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
    }
    
    void HideIndicator()
    {
        // Fade out
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
        
        // Destroy khi đã fade out hoàn toàn
        if (canvasGroup.alpha < 0.01f)
        {
            DestroyIndicator();
        }
    }
    
    void UpdateIndicatorPosition()
    {
        // Tính toán hướng từ player đến enemy
        Vector3 direction = (targetEnemy.position - player.position).normalized;
        
        // Chuyển đổi sang screen space
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 enemyScreenPos = playerCamera.WorldToScreenPoint(targetEnemy.position);
        Vector3 screenDirection = (enemyScreenPos - screenCenter).normalized;
        
        // Tính toán vị trí mũi tên ở mép màn hình
        Vector2 arrowPosition = CalculateEdgePosition(screenDirection);
        arrowUI.anchoredPosition = arrowPosition;
        
        // Quay mũi tên theo hướng
        float angle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);
        arrowUI.rotation = Quaternion.Lerp(arrowUI.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    Vector2 CalculateEdgePosition(Vector3 direction)
    {
        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        Vector2 position = Vector2.zero;
        
        // Tính toán giao điểm với mép màn hình
        float slope = direction.y / direction.x;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Giao với mép trái hoặc phải
            position.x = (direction.x > 0 ? 1 : -1) * (canvasWidth / 2f - borderOffset);
            position.y = position.x * slope;
            
            // Giới hạn y
            position.y = Mathf.Clamp(position.y, -canvasHeight/2f + borderOffset, canvasHeight/2f - borderOffset);
        }
        else
        {
            // Giao với mép trên hoặc dưới
            position.y = (direction.y > 0 ? 1 : -1) * (canvasHeight / 2f - borderOffset);
            position.x = position.y / slope;
            
            // Giới hạn x
            position.x = Mathf.Clamp(position.x, -canvasWidth/2f + borderOffset, canvasWidth/2f - borderOffset);
        }
        
        return position;
    }
    
    public void DestroyIndicator()
    {
        Destroy(gameObject);
    }
    
}
