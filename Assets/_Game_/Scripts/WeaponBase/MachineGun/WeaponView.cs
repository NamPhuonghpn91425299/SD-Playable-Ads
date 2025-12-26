using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;

public class WeaponView : MonoBehaviour, IObserver<CamShakeEvent>
{
    [Header("Basic control")] [SerializeField]
    private bool _canRotateCamera = true; // Biến để kiểm tra có thể xoay camera hay không

    [SerializeField] private Transform _mainRoot;
    [SerializeField] private Transform _head;
    [SerializeField] private float _sensitivity = 15f;
    [SerializeField] private float _slerpFactor = 12.5f;
    [SerializeField] private Vector2 _viewHorizontalThreshold = new Vector2(-60f, 60f);
    [SerializeField] private Vector2 _viewVerticalThreshold = new Vector2(-89f, 89f);
    [SerializeField] private Vector2 _initRotate;
    [SerializeField] private Vector2 _totalRotate;
    [SerializeField] private bool WeaponViews = false; // Biến Bool để chọn logic
    [SerializeField] private Transform WeaponTrans; // Biến Transform cho vũ khí
    [SerializeField] private RectTransform CrossHair; // Biến RectTransform cho CrossHair

    [SerializeField]
    private Vector2 _crossHairMovementLimit = new Vector2(100f, 100f); // Giới hạn phạm vi di chuyển của CrossHair

    [SerializeField]
    private Vector2 _weaponMovementLimit = new Vector2(30f, 30f); // Giới hạn phạm vi di chuyển của súng

    [SerializeField] private Vector2 screenPosValue; // Thay đổi từ float thành Vector2
    [SerializeField] private Transform CameraTrans; // Thêm biến Transform cho Camera
    [SerializeField] private float CrossHairPos; // Thêm biến Transform cho Camera
    [SerializeField] private bool IsLimitRotateX = true; // Biến để kiểm tra có giới hạn phạm vi di chuyển hay không
    [SerializeField] private bool IsLimitRotateY = true; // Biến để kiểm tra có giới hạn phạm vi di chuyển hay không
    private Tween _currentShake;


    private Quaternion originalCameraRotation;
    private Vector3 vectorCam;
    private Vector2 _previousRotate;


    private void Awake()
    {
        SetDefaultView();
        originalCameraRotation = CameraTrans.localRotation;
    }

    public void SetDefaultView()
    {
        _totalRotate = _initRotate;
        _previousRotate = _totalRotate;
        _mainRoot.localRotation = Quaternion.Euler(0, _previousRotate.x, 0);
        _head.localRotation = Quaternion.Euler(-_previousRotate.y, 0, 0);
    }

    private void Start()
    {
        EventManager.Instance?.Subscribe<CamShakeEvent>(this);
        // if (WeaponViews)
        // {
        //     CrossHair.anchoredPosition = new Vector2(5.66243e-05f, 43.61921f);
        // }
        // else
        // {
        //     CrossHair.anchoredPosition = new Vector2(0, CrossHairPos);
        // }
    }

    public Vector2 GetTouchInputLikeMouse()
    {
        Touch touch = Input.GetTouch(0);

        // Chỉ tính khi di chuyển
        if (touch.phase == TouchPhase.Moved)
        {
            // deltaPosition là pixel movement
            Vector2 delta = touch.deltaPosition;

            // Convert sang normalized (-1 to 1) như Mouse
            delta.x = delta.x / Screen.width * 170f; // 20f là sensitivity
            delta.y = delta.y / Screen.height * 170f;

            // Smooth nếu cần
            delta = Vector2.Lerp(Vector2.zero, delta, 0.5f);

            return delta;
        }

        return Vector2.zero;
    }

    public CamShakeData CamShakeData;

    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        //     EventManager.Instance?.Publish(new CamShakeEvent(CamShakeData));

        if (GameController.Instance.CurrentGameState != GameConstants.GameState.InGame)
            return;
#if UNITY_EDITOR
        if (Input.GetMouseButton(0) && _canRotateCamera)
        {
            var input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
        if (Input.touchCount > 0 && _canRotateCamera)
        {
            var input = GetTouchInputLikeMouse();
#endif
            if (Mathf.Abs(input.x) > 1000)
                input.x = 0;
            if (Mathf.Abs(input.y) > 1000)
                input.y = 0;

            var totalRotate = _totalRotate;
            var rotate = input * (_sensitivity * Time.timeScale);
            var slerpParam = _slerpFactor * Time.deltaTime;
            totalRotate += rotate;

            if (IsLimitRotateX)
                totalRotate.x = Mathf.Clamp(totalRotate.x, _viewHorizontalThreshold.x, _viewHorizontalThreshold.y);
            if (IsLimitRotateY)
                totalRotate.y = Mathf.Clamp(totalRotate.y, _viewVerticalThreshold.x, _viewVerticalThreshold.y);

            if (WeaponViews && WeaponTrans != null)
            {
                if (IsLimitRotateX)
                    totalRotate.x = Mathf.Clamp(totalRotate.x, -_weaponMovementLimit.x, _weaponMovementLimit.x);
                if (IsLimitRotateY)
                    totalRotate.y = Mathf.Clamp(totalRotate.y, -_weaponMovementLimit.y, _weaponMovementLimit.y);

                WeaponTrans.localRotation = Quaternion.Slerp(WeaponTrans.localRotation,
                    Quaternion.Euler(-totalRotate.y, totalRotate.x, 0), slerpParam);
            }
            else
            {
                _mainRoot.localRotation = Quaternion.Slerp(_mainRoot.localRotation,
                    Quaternion.Euler(0, totalRotate.x, 0), slerpParam);
                _head.localRotation = Quaternion.Slerp(_head.localRotation,
                    Quaternion.Euler(-totalRotate.y, 0, 0), slerpParam);
            }

            UpdateCrossHair(totalRotate, slerpParam);

            _totalRotate = totalRotate;
            _previousRotate = totalRotate;
        }
    }

    private void UpdateCrossHair(Vector2 totalRotate, float slerpParam)
    {
        if (CrossHair != null && CameraTrans != null && WeaponViews)
        {
            Vector2 screenPos = new Vector2(
                totalRotate.x / _viewHorizontalThreshold.y,
                totalRotate.y / _viewVerticalThreshold.y
            );

            Vector2 adjustedScreenPos = new Vector2(
                screenPos.x * screenPosValue.x,
                screenPos.y * screenPosValue.y
            );

            if (IsLimitRotateX)
                adjustedScreenPos.x = Mathf.Clamp(adjustedScreenPos.x, -_crossHairMovementLimit.x,
                    _crossHairMovementLimit.x);
            if (IsLimitRotateY)
                adjustedScreenPos.y = Mathf.Clamp(adjustedScreenPos.y, -_crossHairMovementLimit.y,
                    _crossHairMovementLimit.y);

            Vector3 cameraRotation = originalCameraRotation.eulerAngles;
            float cameraTiltX = Mathf.Sin(cameraRotation.x * Mathf.Deg2Rad);
            float cameraTiltY = Mathf.Sin(cameraRotation.y * Mathf.Deg2Rad);

            adjustedScreenPos.x += cameraTiltY * screenPosValue.x;
            adjustedScreenPos.y += cameraTiltX * screenPosValue.y;

            CrossHair.anchoredPosition = Vector2.Lerp(CrossHair.anchoredPosition, adjustedScreenPos, slerpParam);
        }
    }

    public void OnNotify(CamShakeEvent data)
    {
        _currentShake?.Kill(); // huỷ rung cũ nếu có
        CameraTrans.localPosition = new Vector3(0, 0.2f, 0.464f);
        _currentShake = CameraTrans.DOShakePosition(data._camShakeData.duration, data._camShakeData.strength,
                data._camShakeData.vibrato, data._camShakeData.randomness)
            .SetEase(Ease.OutQuad)
            .OnKill(() => _currentShake = null)
            .OnComplete(()=>{CameraTrans.localPosition = new Vector3(0, 0.2f, 0.464f);});
    }

    public void OnButtonSelect()
    {
        _canRotateCamera = false;
    }

    public void OnButtonUnselect()
    {
        _canRotateCamera = true;
    }
}