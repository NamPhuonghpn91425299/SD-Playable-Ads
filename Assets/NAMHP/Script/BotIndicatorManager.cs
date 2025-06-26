using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BotIndicatorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform botGroup;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform indicatorPrefab;
    [SerializeField] private Canvas canvas;
    public bool isOffScreen;
    [Header("Settings")]
    [SerializeField] private float edgeBuffer = 50f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float maxDistance = 50f;
    
    [Header("Height Settings")]
    [SerializeField] private float defaultHeightOffset = 1.5f;
    [SerializeField] private float defaultRotationOffset = 90f;
    private Dictionary<GameObject, float> botHeightOffsets = new Dictionary<GameObject, float>();
    
    public static BotIndicatorManager instance;
    private Dictionary<GameObject, RectTransform> botIndicators = new Dictionary<GameObject, RectTransform>();

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void RegisterBot(GameObject bot, float customHeight = -1f)
    {
        if (!botIndicators.ContainsKey(bot))
        {
            RectTransform indicator = Instantiate(indicatorPrefab, canvas.transform);
            botIndicators.Add(bot, indicator);
            float heightOffset = customHeight >= 0 ? customHeight : defaultHeightOffset;
            botHeightOffsets.Add(bot, heightOffset);
            indicator.transform.SetParent(botGroup);
        }
    }

    public void UpdateBotHeight(GameObject bot, float newHeight)
    {
        if (botHeightOffsets.ContainsKey(bot))
        {
            botHeightOffsets[bot] = newHeight;
        }
    }

    public void UnregisterBot(GameObject bot)
    {
        if (botIndicators.TryGetValue(bot, out RectTransform indicator))
        {
            Destroy(indicator.gameObject);
            botIndicators.Remove(bot);
            botHeightOffsets.Remove(bot);
        }
    }

    private void Update()
    {
        foreach (var kvp in new Dictionary<GameObject, RectTransform>(botIndicators))
        {
            GameObject bot = kvp.Key;
            RectTransform indicator = kvp.Value;

            if (bot == null)
            {
                UnregisterBot(bot);
                continue;
            }

            UpdateIndicator(bot, indicator);
        }
    }

    private void UpdateIndicator(GameObject bot, RectTransform indicator)
    {
        float heightOffset = botHeightOffsets.ContainsKey(bot) ? botHeightOffsets[bot] : defaultHeightOffset;
        Vector3 botPositionWithOffset = bot.transform.position + Vector3.up * heightOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(botPositionWithOffset);
        
        // Kiểm tra bot có nằm ngoài màn hình không
        isOffScreen = screenPosition.z <= 0 || 
                          screenPosition.x <= 0 || screenPosition.x >= Screen.width ||
                          screenPosition.y <= 0 || screenPosition.y >= Screen.height;

        indicator.gameObject.SetActive(isOffScreen);

        if (isOffScreen)
        {
            UpdateIndicatorPosition(screenPosition, indicator);
            UpdateIndicatorRotation(bot, indicator);
            UpdateIndicatorScale(bot, indicator);
        }

    }

    private void UpdateIndicatorPosition(Vector3 screenPosition, RectTransform indicator)
    {
        if (screenPosition.z < 0)
        {
            screenPosition *= -1;
        }
    
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 screenBounds = new Vector2(Screen.width / 2f - edgeBuffer, Screen.height / 2f - edgeBuffer);
        Vector2 direction = (new Vector2(screenPosition.x, screenPosition.y) - screenCenter).normalized;
        
        float angle = Mathf.Atan2(direction.y, direction.x);
        Vector2 position;
    
        if (Mathf.Abs(direction.x) / Mathf.Abs(direction.y) > screenBounds.x / screenBounds.y)
        {
            position = new Vector2(
                screenBounds.x * Mathf.Sign(direction.x),
                screenBounds.x * Mathf.Tan(angle) * Mathf.Sign(direction.x)
            );
        }
        else
        {
            position = new Vector2(
                screenBounds.y * (1f / Mathf.Tan(angle)) * Mathf.Sign(direction.y),
                screenBounds.y * Mathf.Sign(direction.y)
            );
        }
    
        indicator.position = screenCenter + position;
    }

    private void UpdateIndicatorRotation(GameObject bot, RectTransform indicator)
    {
        Vector2 indicatorScreenPos = indicator.position;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 direction = (indicatorScreenPos - screenCenter).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, angle + defaultRotationOffset);
    }

    private void UpdateIndicatorScale(GameObject bot, RectTransform indicator)
    {
        float distance = Vector3.Distance(mainCamera.transform.position, bot.transform.position);
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        float scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
        indicator.localScale = Vector3.one * scale;
    }
}