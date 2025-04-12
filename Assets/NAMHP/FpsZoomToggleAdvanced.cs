using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FpsZoomToggleAdvanced : MonoBehaviour
{
    public Camera playerCamera;
    public Camera weaponCamera;
    [SerializeField] private GameObject crosshair; // Kéo thả crosshair từ Hierarchy vào đây
    [SerializeField] private Button zoomButton; // Kéo thả Button từ Hierarchy vào đây
    public Image scopeIcon;         // Kéo thả Image hiển thị icon zoom
    public Image zoomIcon;         // Kéo thả Image hiển thị icon zoom
    public Sprite[] zoomSprites;   // Mảng chứa 2 sprite: [0] = zoom out icon, [1] = zoom in icon
    public Slider zoomLevelSlider; // Kéo thả Slider từ Hierarchy
    public Image sliderActiveBorder; // Kéo thả Image viền từ Hierarchy vào đây
    public Text sliderValueText; // Kéo thả Text hiển thị giá trị slider từ Hierarchy vào đây
    [Header("FOV Settings")]
    public float defaultFOV = 50f;        // FOV khi không zoom
    public float weaponFOV = 45f;
    public float minZoomFOV = 20f;        // FOV nhỏ nhất (zoom tối đa, khi vừa nhấn nút hoặc slider ở mức max zoom)
    public float maxZoomedInFOV = 40f;    // FOV lớn nhất khi đang zoom (khi kéo slider xuống mức min zoom)
    public float minZoom = 0f;
    public float maxZoom = 4f;
    public float zoomSpeed = 10f;         // Tốc độ chuyển đổi FOV
    public float displayIntegerTolerance = 0.01f; // Ngưỡng để coi là số nguyên
    public float shakeAmount = 10f; // Độ lắc nhẹ
    private float targetFOV;
    private float weapontFOV;
    private bool isZoomedIn = false;
    private bool _isShooting = false;
    private Vector2 originalPosition;
    void Start()
    {
        EventManager.AddListener<bool>(EventName.OnChangeMachineGun, OnChangeWeapon);
        originalPosition = scopeIcon.rectTransform.anchoredPosition;
        zoomButton.onClick.AddListener(ToggleZoom); // Đăng ký sự kiện cho nút zoom
        if (playerCamera == null) playerCamera = Camera.main;
        // Validate FOV settings
        if (minZoomFOV >= maxZoomedInFOV)
        {
            Debug.LogWarning("minZoomFOV should be less than maxZoomedInFOV. Adjusting maxZoomedInFOV.");
            maxZoomedInFOV = minZoomFOV + 1f; // Đảm bảo có khoảng cách nhỏ
        }
        if (maxZoomedInFOV >= defaultFOV)
        {
            Debug.LogWarning("maxZoomedInFOV should ideally be less than defaultFOV. Adjusting maxZoomedInFOV.");
            maxZoomedInFOV = defaultFOV - 1f; // Đảm bảo zoom luôn hẹp hơn default
        }
        minZoomFOV = Mathf.Max(1f, minZoomFOV); // Đảm bảo FOV không quá nhỏ
        maxZoomedInFOV = Mathf.Clamp(maxZoomedInFOV, minZoomFOV, defaultFOV); // Ràng buộc hợp lý

        targetFOV = defaultFOV;
        weapontFOV = defaultFOV;
        weaponCamera.fieldOfView = defaultFOV;
        playerCamera.fieldOfView = defaultFOV;

        // Cấu hình Slider
        if (zoomLevelSlider != null)
        {
            // Giả sử slider chạy từ 0 (dưới) đến 1 (trên)
            zoomLevelSlider.minValue = 0f;
            zoomLevelSlider.maxValue = 1f;
            zoomLevelSlider.onValueChanged.AddListener(UpdateZoomLevelWhileActive);
            zoomLevelSlider.gameObject.SetActive(false);
        }

        // Đặt icon ban đầu
        if (zoomIcon != null && zoomSprites != null && zoomSprites.Length > 0)
        {
            zoomIcon.sprite = zoomSprites[0]; // Icon zoom out mặc định
        }
        sliderActiveBorder.enabled = false;
        scopeIcon.gameObject.SetActive(false); // Ẩn icon scope khi không zoom
        crosshair.SetActive(true); // Hiện crosshair mặc định
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<bool>(EventName.OnChangeMachineGun, OnChangeWeapon);
    }
    private void OnChangeWeapon(bool isChangeWeapon)
    {
            _isShooting = isChangeWeapon;
            Debug.Log("OnChangeWeapon: " + isChangeWeapon);
    }

    

    // Hàm được gọi bởi sự kiện onValueChanged của Slider *CHỈ KHI* đang zoom
    void UpdateZoomLevelWhileActive(float sliderValue)
    {
        // Chỉ cập nhật targetFOV nếu đang trong trạng thái zoom
        if (!isZoomedIn) return;
        // Dùng Lerp ngược: Lerp(max, min, value)
        targetFOV = Mathf.Lerp(maxZoomedInFOV, minZoomFOV, sliderValue);
        // Đảm bảo targetFOV luôn nằm trong khoảng cho phép khi zoom
        targetFOV = Mathf.Clamp(targetFOV, minZoomFOV, maxZoomedInFOV);

        if (sliderValueText != null)
        {
            // Tính giá trị zoom hiển thị
            float displayedZoomLevel = Mathf.Lerp(minZoom, maxZoom, sliderValue);
            // Làm tròn đến số nguyên gần nhất
            float roundedValue = Mathf.Round(displayedZoomLevel);

            // Sử dụng toán tử ba ngôi để chọn định dạng và giá trị phù hợp
            // Cấu trúc: condition ? value_if_true : value_if_false
            string formattedZoom = Mathf.Abs(displayedZoomLevel - roundedValue) < displayIntegerTolerance
                ? roundedValue.ToString("F0") // TRUE: Đủ gần số nguyên -> Dùng số đã làm tròn, định dạng F0
                : displayedZoomLevel.ToString("F1", CultureInfo.InvariantCulture); // FALSE: Không đủ gần -> Dùng số gốc, định dạng F1

            // Cập nhật Text UI
            sliderValueText.text = formattedZoom + "X";

        }
    }

    // Hàm này sẽ được gọi bởi sự kiện OnClick của UI Button
    public void ToggleZoom()
    {
        isZoomedIn = !isZoomedIn; // Đảo trạng thái

        if (isZoomedIn) // --- Vừa BẬT zoom ---
        {
            crosshair.SetActive(false); // Ẩn crosshair khi zoom
            scopeIcon.gameObject.SetActive(true); // Hiện icon scope
            // 1. Đặt mục tiêu là FOV zoom tối đa ban đầu
            targetFOV = minZoomFOV;
            weapontFOV = weaponFOV;
            Debug.Log("Zoom IN activated. Initial Target FOV: " + targetFOV);
            zoomIcon.sprite = zoomSprites[1]; // Icon zoom in
            zoomLevelSlider.gameObject.SetActive(true); // Nếu bạn ẩn/hiện slider
            // Đặt slider về vị trí tương ứng với minZoomFOV (thường là maxValue nếu kéo lên là zoom max)
            zoomLevelSlider.value = zoomLevelSlider.maxValue;
            // Gọi hàm cập nhật một lần để đảm bảo targetFOV đúng ngay cả khi slider chưa bị kéo
            UpdateZoomLevelWhileActive(zoomLevelSlider.value);
        }
        else // --- Vừa TẮT zoom ---
        {
            crosshair.SetActive(true); // Hiện lại crosshair khi tắt zoom
            scopeIcon.gameObject.SetActive(false); // Ẩn icon scope
            targetFOV = defaultFOV;
            weapontFOV = defaultFOV;
            Debug.Log("Zoom OUT activated. Target FOV: " + targetFOV);

            if (zoomIcon != null && zoomSprites != null && zoomSprites.Length > 0)
            {
                zoomIcon.sprite = zoomSprites[0]; // Icon zoom out
            }
            if (zoomLevelSlider != null)
            {
                sliderActiveBorder.enabled = false;
                zoomLevelSlider.gameObject.SetActive(false); // Nếu bạn ẩn/hiện slider
            }
        }
    }
    // Hàm này sẽ được gọi khi người dùng nhấn chuột XUỐNG trên slider
    public void OnSliderPointerDown()
    {
        // Chỉ hiện viền nếu slider đang được phép tương tác (tức là đang zoom)
        if (isZoomedIn && sliderActiveBorder != null)
        {
            sliderActiveBorder.enabled = true;
            Debug.Log("Slider Pointer Down - Showing Border");
        }
    }

    // Hàm này sẽ được gọi khi người dùng thả chuột LÊN sau khi nhấn trên slider
    public void OnSliderPointerUp()
    {
        // Luôn ẩn viền khi thả chuột
        if (sliderActiveBorder != null)
        {
            sliderActiveBorder.enabled = false;
            Debug.Log("Slider Pointer Up - Hiding Border");
        }
    }
    void Update()
    {
        // Luôn luôn làm mượt FOV camera tiến về targetFOV
        if (playerCamera != null && playerCamera.fieldOfView != targetFOV)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, weapontFOV, Time.deltaTime * zoomSpeed);
        }
        
        if (isZoomedIn && Input.GetMouseButton(0) && _isShooting)
        {
                scopeIcon.rectTransform.anchoredPosition = originalPosition + new Vector2(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount)
                ); // Đặt lại vị trí icon scope
        }
    }
}