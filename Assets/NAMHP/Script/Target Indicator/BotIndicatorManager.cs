// using System;
// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.UI;
//
// public class BotIndicatorManager : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] Transform botGroup; // Nhóm chứa các chỉ báo của bot
//     [SerializeField] private Camera mainCamera; // Camera chính trong trò chơi
//     [SerializeField] private RectTransform indicatorPrefab; // Prefab của chỉ báo
//     [SerializeField] private Canvas canvas; // Canvas để hiển thị các chỉ báo
//     public bool isOffScreen; // Kiểm tra bot có nằm ngoài màn hình hay không
//
//     [Header("Settings")]
//     [SerializeField] private float edgeBuffer = 15f; // Khoảng cách đệm từ mép màn hình
//     [SerializeField] private float minScale = 0.8f; // Kích thước tối thiểu của chỉ báo
//     [SerializeField] private float maxScale = 1.2f; // Kích thước tối đa của chỉ báo
//     [SerializeField] private float maxDistance = 50f; // Khoảng cách tối đa để tính toán kích thước chỉ báo
//
//     [Header("Height Settings")]
//     [SerializeField] private float defaultHeightOffset = 1.5f; // Độ cao mặc định của chỉ báo so với bot
//     [SerializeField] private float defaultRotationOffset = 90f; // Góc xoay mặc định của chỉ báo
//     private Dictionary<GameObject, float> botHeightOffsets = new Dictionary<GameObject, float>(); // Từ điển lưu độ cao tùy chỉnh của từng bot
//
//     public static BotIndicatorManager instance; // Singleton của lớp
//     private Dictionary<GameObject, RectTransform> botIndicators = new Dictionary<GameObject, RectTransform>(); // Từ điển lưu các chỉ báo tương ứng với từng bot
//
//     private void Awake()
//     {
//         // Khởi tạo singleton
//         if (instance != null) return;
//         instance = this;
//     }
//
//     private void Start()
//     {
//         // Gán camera chính nếu chưa được gán
//         if (mainCamera == null)
//             mainCamera = Camera.main;
//     }
//
//     public void RegisterBot(GameObject bot, float customHeight = -1f)
//     {
//         // Đăng ký bot mới và tạo chỉ báo tương ứng
//         if (!botIndicators.ContainsKey(bot))
//         {
//             RectTransform indicator = Instantiate(indicatorPrefab, canvas.transform); // Tạo chỉ báo
//             botIndicators.Add(bot, indicator); // Thêm vào từ điển
//             float heightOffset = customHeight >= 0 ? customHeight : defaultHeightOffset; // Sử dụng độ cao tùy chỉnh nếu có
//             botHeightOffsets.Add(bot, heightOffset); // Lưu độ cao vào từ điển
//             indicator.transform.SetParent(botGroup); // Gán chỉ báo vào nhóm
//         }
//     }
//
//     public void UpdateBotHeight(GameObject bot, float newHeight)
//     {
//         // Cập nhật độ cao của chỉ báo cho bot
//         if (botHeightOffsets.ContainsKey(bot))
//         {
//             botHeightOffsets[bot] = newHeight;
//         }
//     }
//
//     public void UnregisterBot(GameObject bot)
//     {
//         // Hủy đăng ký bot và xóa chỉ báo tương ứng
//         if (botIndicators.TryGetValue(bot, out RectTransform indicator))
//         {
//             Destroy(indicator.gameObject); // Xóa chỉ báo
//             botIndicators.Remove(bot); // Xóa khỏi từ điển
//             botHeightOffsets.Remove(bot); // Xóa độ cao khỏi từ điển
//         }
//     }
//
//     private void Update()
//     {
//         // Cập nhật vị trí, hướng, và kích thước của các chỉ báo trong mỗi khung hình
//         foreach (var kvp in new Dictionary<GameObject, RectTransform>(botIndicators))
//         {
//             GameObject bot = kvp.Key;
//             RectTransform indicator = kvp.Value;
//
//             if (bot == null)
//             {
//                 UnregisterBot(bot); // Xóa bot nếu không tồn tại
//                 continue;
//             }
//
//             UpdateIndicator(bot, indicator); // Cập nhật chỉ báo
//         }
//     }
//
//     private void UpdateIndicator(GameObject bot, RectTransform indicator)
//     {
//         // Cập nhật trạng thái của chỉ báo (vị trí, xoay, và kích thước)
//         float heightOffset = botHeightOffsets.ContainsKey(bot) ? botHeightOffsets[bot] : defaultHeightOffset;
//         Vector3 botPositionWithOffset = bot.transform.position + Vector3.up * heightOffset;
//         Vector3 screenPosition = mainCamera.WorldToScreenPoint(botPositionWithOffset);
//
//         // Kiểm tra bot có nằm ngoài màn hình không
//         isOffScreen = screenPosition.z <= 0 ||
//                           screenPosition.x <= 0 || screenPosition.x >= Screen.width ||
//                           screenPosition.y <= 0 || screenPosition.y >= Screen.height;
//
//         indicator.gameObject.SetActive(isOffScreen); // Hiển thị chỉ báo nếu bot nằm ngoài màn hình
//
//         if (isOffScreen)
//         {
//             UpdateIndicatorPosition(screenPosition, indicator); // Cập nhật vị trí
//             UpdateIndicatorRotation(bot, indicator); // Cập nhật góc xoay
//             UpdateIndicatorScale(bot, indicator); // Cập nhật kích thước
//         }
//     }
//
//     private void UpdateIndicatorPosition(Vector3 screenPosition, RectTransform indicator)
//     {
//         // Tính toán vị trí của chỉ báo khi bot nằm ngoài màn hình
//         if (screenPosition.z < 0)
//         {
//             screenPosition *= -1; // Đảo ngược vị trí nếu bot nằm phía sau camera
//         }
//
//         Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
//         Vector2 screenBounds = new Vector2(Screen.width / 2f - edgeBuffer, Screen.height / 2f - edgeBuffer);
//         Vector2 direction = (new Vector2(screenPosition.x, screenPosition.y) - screenCenter).normalized;
//
//         float angle = Mathf.Atan2(direction.y, direction.x);
//         Vector2 position;
//
//         // Tính toán vị trí chỉ báo dựa trên góc và giới hạn màn hình
//         if (Mathf.Abs(direction.x) / Mathf.Abs(direction.y) > screenBounds.x / screenBounds.y)
//         {
//             position = new Vector2(
//                 screenBounds.x * Mathf.Sign(direction.x),
//                 screenBounds.x * Mathf.Tan(angle) * Mathf.Sign(direction.x)
//             );
//         }
//         else
//         {
//             position = new Vector2(
//                 screenBounds.y * (1f / Mathf.Tan(angle)) * Mathf.Sign(direction.y),
//                 screenBounds.y * Mathf.Sign(direction.y)
//             );
//         }
//
//         indicator.position = screenCenter + position; // Gán vị trí cho chỉ báo
//     }
//
//     private void UpdateIndicatorRotation(GameObject bot, RectTransform indicator)
//     {
//         // Cập nhật góc xoay của chỉ báo dựa trên vị trí của bot
//         Vector2 indicatorScreenPos = indicator.position;
//         Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
//         Vector2 direction = (indicatorScreenPos - screenCenter).normalized;
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//         indicator.rotation = Quaternion.Euler(0, 0, angle + defaultRotationOffset); // Gán góc xoay
//     }
//
//     private void UpdateIndicatorScale(GameObject bot, RectTransform indicator)
//     {
//         // Điều chỉnh kích thước của chỉ báo dựa trên khoảng cách giữa bot và camera
//         float distance = Vector3.Distance(mainCamera.transform.position, bot.transform.position);
//         float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
//         float scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
//         indicator.localScale = Vector3.one * scale; // Gán kích thước
//     }
// }
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BotIndicatorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform botGroup;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform indicatorPrefab;
    [SerializeField] private Canvas canvas;
    public bool isOffScreen;

    [Header("Settings")]
    [SerializeField] private float edgeBuffer = 15f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Height Settings")]
    [SerializeField] private float defaultHeightOffset = 1.5f;
    [SerializeField] private float defaultRotationOffset = 90f;

    public static BotIndicatorManager instance;

    private readonly Dictionary<GameObject, RectTransform> botIndicators = new Dictionary<GameObject, RectTransform>();
    private readonly Dictionary<GameObject, float> botHeightOffsets = new Dictionary<GameObject, float>();
    private readonly List<GameObject> botsToRemove = new List<GameObject>();

    private Vector2 screenCenter;
    private Vector2 screenBounds;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        screenBounds = new Vector2(Screen.width / 2f - edgeBuffer, Screen.height / 2f - edgeBuffer);
    }

    public void RegisterBot(GameObject bot, float customHeight = -1f)
    {
        if (botIndicators.ContainsKey(bot)) return;

        RectTransform indicator = Instantiate(indicatorPrefab, botGroup);
        botIndicators[bot] = indicator;
        botHeightOffsets[bot] = customHeight >= 0 ? customHeight : defaultHeightOffset;
    }

    public void UpdateBotHeight(GameObject bot, float newHeight)
    {
        if (botHeightOffsets.ContainsKey(bot))
            botHeightOffsets[bot] = newHeight;
    }

    public void UnregisterBot(GameObject bot)
    {
        if (!botIndicators.TryGetValue(bot, out RectTransform indicator)) return;
        indicator.gameObject.SetActive(false);
        //Destroy(indicator.gameObject);
        botIndicators.Remove(bot);
        botHeightOffsets.Remove(bot);
    }

    private void Update()
    {
        botsToRemove.Clear();

        foreach (var kvp in botIndicators)
        {
            GameObject bot = kvp.Key;
            RectTransform indicator = kvp.Value;

            if (bot == null)
            {
                botsToRemove.Add(bot);
                continue;
            }

            UpdateIndicator(bot, indicator);
        }

        foreach (var bot in botsToRemove)
        {
            UnregisterBot(bot);
        }
    }

    private void UpdateIndicator(GameObject bot, RectTransform indicator)
    {
        float heightOffset = botHeightOffsets.TryGetValue(bot, out var offset) ? offset : defaultHeightOffset;
        Vector3 botPositionWithOffset = bot.transform.position + Vector3.up * heightOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(botPositionWithOffset);

        isOffScreen = screenPosition.z <= 0 ||
                      screenPosition.x <= 0 || screenPosition.x >= Screen.width ||
                      screenPosition.y <= 0 || screenPosition.y >= Screen.height;

        indicator.gameObject.SetActive(isOffScreen);

        if (!isOffScreen) return;

        UpdateIndicatorPosition(screenPosition, indicator);
        UpdateIndicatorRotation(indicator);
        UpdateIndicatorScale(bot.transform.position, indicator);
    }

    private void UpdateIndicatorPosition(Vector3 screenPosition, RectTransform indicator)
    {
        if (screenPosition.z < 0)
            screenPosition *= -1;

        Vector2 direction = ((Vector2)screenPosition - screenCenter).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x);
        Vector2 position;

        bool isWider = Mathf.Abs(direction.x) / Mathf.Abs(direction.y) > screenBounds.x / screenBounds.y;
        if (isWider)
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

    private void UpdateIndicatorRotation(RectTransform indicator)
    {
        Vector2 direction = ((Vector2)indicator.position - screenCenter).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, angle + defaultRotationOffset);
    }

    private void UpdateIndicatorScale(Vector3 botPosition, RectTransform indicator)
    {
        float distance = Vector3.Distance(mainCamera.transform.position, botPosition);
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        float scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
        indicator.localScale = Vector3.one * scale;
    }

}
