using UnityEngine;

public class WeaponViewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _mainRoot;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform WeaponTrans;
    [SerializeField] private RectTransform CrosshairTransform;

    [Header("View Settings")]
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private float _slerpFactor = 10f;
    [SerializeField] private Vector2 _viewHorizontalThreshold = new Vector2(-80f, 80f);
    [SerializeField] private Vector2 _viewVerticalThreshold = new Vector2(-60f, 60f);
    [SerializeField] private Vector2 _weaponMovementLimit = new Vector2(30f, 30f);

    [Header("Input Smoothing")]
    [SerializeField] private bool _enableSmoothing = true;
    [SerializeField] private float _smoothingSpeed = 10f;
    private Vector2 _currentInputVelocity;
    private Vector2 _smoothedInput;

    [Header("Recoil Settings")]
    [SerializeField] private float _recoilAmount = 2f;
    [SerializeField] private float _recoilRecoverySpeed = 5f;
    [SerializeField] private float _maxRecoil = 10f;
    private float _currentRecoil;
    private float _recoilRecovery;

    // [Header("Weapon Sway")]
    // [SerializeField] private float _swayAmount = 0.02f;
    // [SerializeField] private float _swaySpeed = 1f;
    // [SerializeField] private float _returnSpeed = 5f;
    // [SerializeField] private float _breathingAmount = 0.5f;
    // [SerializeField] private float _breathingSpeed = 1f;
    // private Vector3 _initialWeaponPosition;
    // private Vector3 _targetWeaponPosition;

    public bool WeaponView { get; set; } = true;
    private Vector2 _totalRotate;
    private Vector2 _previousRotate;

    private void Start()
    {
        CrosshairTransform.anchoredPosition = Vector2.zero;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMouseInput();
        if (WeaponView && WeaponTrans != null)
        {
            //ApplyWeaponSway();
            UpdateRecoil();
        }
    }

     private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            // Lấy input từ chuột
            var input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            
            // Kiểm tra giá trị hợp lệ
            if (Mathf.Abs(input.x) > 1000) input.x = 0;
            if (Mathf.Abs(input.y) > 1000) input.y = 0;

            // Áp dụng làm mượt input nếu được bật
            if (_enableSmoothing)
            {
                _smoothedInput = Vector2.SmoothDamp(_smoothedInput, input, ref _currentInputVelocity, 
                    1f / _smoothingSpeed);
                input = _smoothedInput;
            }

            var totalRotate = _totalRotate;
            var rotate = input * (_sensitivity * Time.timeScale);
            var slerpParam = _slerpFactor * Time.deltaTime;
            
            // Thêm recoil vào rotation theo trục Y
            if (WeaponView)
            {
                rotate.y += _currentRecoil * Time.deltaTime;
            }

            totalRotate += rotate;
            totalRotate.x = Mathf.Clamp(totalRotate.x, _viewHorizontalThreshold.x, _viewHorizontalThreshold.y);
            totalRotate.y = Mathf.Clamp(totalRotate.y, _viewVerticalThreshold.x, _viewVerticalThreshold.y);

            if (WeaponView && WeaponTrans != null)
            {
                // Giới hạn phạm vi di chuyển của súng
                var weaponRotate = totalRotate;
                weaponRotate.x = Mathf.Clamp(weaponRotate.x, -_weaponMovementLimit.x, _weaponMovementLimit.x);
                weaponRotate.y = Mathf.Clamp(weaponRotate.y, -_weaponMovementLimit.y, _weaponMovementLimit.y);

                // Xoay WeaponTrans theo di chuyển của chuột
                WeaponTrans.localRotation = Quaternion.Slerp(WeaponTrans.localRotation,
                    Quaternion.Euler(-weaponRotate.y, weaponRotate.x, 0), slerpParam);
            }
            else
            {
                // Xoay _mainRoot và _head
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
    // private void ApplyWeaponSway()
    // {
    //     // Tính toán sway dựa trên chuyển động chuột
    //     float movementX = -Input.GetAxis("Mouse X") * _swayAmount;
    //     float movementY = -Input.GetAxis("Mouse Y") * _swayAmount;
    //
    //     // Thêm hiệu ứng thở (breathing effect)
    //     float breathingOffset = Mathf.Sin(Time.time * _breathingSpeed) * _breathingAmount;
    //
    //     // Tính vị trí đích
    //     _targetWeaponPosition = _initialWeaponPosition + new Vector3(
    //         movementX,
    //         movementY + breathingOffset,
    //         0
    //     );
    //
    //     // Áp dụng smooth movement
    //     WeaponTrans.localPosition = Vector3.Lerp(
    //         WeaponTrans.localPosition,
    //         _targetWeaponPosition,
    //         Time.deltaTime * _returnSpeed
    //     );
    // }

    private void UpdateRecoil()
    {
        // Xử lý recoil khi bắn
        if (Input.GetMouseButtonDown(0)) // Thay bằng event bắn thật
        {
            _currentRecoil = Mathf.Min(_currentRecoil + _recoilAmount, _maxRecoil);
        }

        // Hồi phục recoil
        if (_currentRecoil > 0)
        {
            _currentRecoil = Mathf.Max(0, _currentRecoil - _recoilRecoverySpeed * Time.deltaTime);
        }
    }

    private void UpdateCrossHair(Vector2 totalRotate, float slerpParam)
    {
        if (CrosshairTransform == null) return;

        // Cập nhật vị trí crosshair để theo dõi hướng súng một cách mượt mà
        Vector2 targetPosition = new Vector2(totalRotate.x, totalRotate.y) * 1f;

        // Lerp vị trí của crosshair để theo hướng của súng
        CrosshairTransform.localPosition = Vector3.Lerp(
            CrosshairTransform.localPosition,
            new Vector3(targetPosition.x, targetPosition.y, CrosshairTransform.localPosition.z),
            Mathf.Clamp01(slerpParam*2f)  // Đảm bảo slerpParam không vượt quá 1
        );

        // Kiểm tra chuyển động của chuột và hiện tượng recoil
        float spread = (new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).magnitude + _currentRecoil) * 0.1f;

        // Lerp kích thước của crosshair để tạo hiệu ứng mở rộng khi di chuyển
        CrosshairTransform.localScale = Vector3.Lerp(
            CrosshairTransform.localScale,
            Vector3.one * Mathf.Clamp(1f + spread, 1f, 2f), // Giới hạn scale để tránh crosshair quá lớn
            slerpParam * 0.7f
        );
    }


    // Phương thức public để kích hoạt recoil từ bên ngoài
    public void AddRecoil()
    {
        _currentRecoil = Mathf.Min(_currentRecoil + _recoilAmount, _maxRecoil);
    }
}
