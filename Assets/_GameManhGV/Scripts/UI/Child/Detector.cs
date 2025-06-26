using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static NUtiliti; // Sử dụng các phương thức tĩnh từ lớp NUtiliti mà không cần ghi tên lớp.

/// <summary>
/// Quản lý một "Detector" hoặc "Điểm yếu" trên một đối tượng (ví dụ: Bot).
/// Lớp này xử lý vòng đời, máu, hoạt ảnh và tương tác với các hệ thống khác.
/// </summary>
public class Detector : BaseDetector
{
    // === CÁC TRƯỜNG DỮ LIỆU ===

    [Header("Cấu hình vòng đời")]
    [Tooltip("Nếu được chọn, đối tượng sẽ không tự động bị vô hiệu hóa sau khi hết thời gian sống.")]
    [SerializeField] private bool DonActiveFalse;
    [Tooltip("Dải màu sắc của detector. Màu sẽ thay đổi dựa trên thời gian sống còn lại.")]
    [SerializeField] private Gradient detectorGradient;
    [Tooltip("Tham chiếu đến hệ thống mạng của Bot, dùng để lắng nghe sự kiện sát thương.")]
    [SerializeField] private BotNetwork botNetwork;
    [Tooltip("Tỷ lệ kích thước mặc định của detector.")]
    [SerializeField] private float _scaleDefault = 1f;

    [Header("Look Camera")]
    [Tooltip("Transform của camera chính để detector luôn hướng về phía người chơi.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Phóng to sau đó thu nhỏ Detector")]
    [Tooltip("Tốc độ thu nhỏ của detector về kích thước ban đầu.")]
    [SerializeField] private float _scaleSpeed = 60f;
    [Tooltip("Hệ số nhân kích thước ban đầu khi detector mới xuất hiện (tạo hiệu ứng phóng to).")]
    [SerializeField] private float _scaleMultiplier = 10f;

    // Biến nội bộ để lưu trữ kích thước gốc
    private Vector3 _originalScale;
    // Coroutine đang chạy cho hoạt ảnh thu phóng
    private Coroutine _currentCoroutine;
    // Coroutine đang chạy cho việc tự động vô hiệu hóa
    private Coroutine _ActiveFalseCoroutine;

    [Tooltip("Thời gian tồn tại (tính bằng giây) của detector trước khi tự động biến mất.")]
    [SerializeField] private float lifeTime = 2f;
    // Thời gian sống còn lại hiện tại
    private float _currentLifeTime;

    [Tooltip("Transform của đối tượng hình ảnh detector chính, được dùng để thay đổi kích thước.")]
    [SerializeField] private Transform _detector;

    [Header("Health")]
    [Tooltip("Chỉ số của kỹ năng (chưa rõ mục đích sử dụng trong code này).")]
    [SerializeField] private int _skillIndex;
    [Tooltip("Lượng máu tối đa của detector.")]
    [SerializeField] private int _maxHealth = 100;
    // Lượng máu hiện tại
    private int _currentHealth;
    [Tooltip("Hình ảnh dùng để hiển thị thanh máu.")]
    public Image healthImage;
    [Tooltip("Hình ảnh phụ bên trong, cũng thay đổi màu theo thời gian sống.")]
    public Image inSide1Image;
    [Tooltip("Hình ảnh phụ bên trong, cũng thay đổi màu theo thời gian sống.")]
    public Image inSide2Image;

    // Cờ đánh dấu detector đã bị phá hủy hay chưa
    private bool _deadDetector;
    [Tooltip("Nếu true, hoạt ảnh sẽ được thực hiện khi đối tượng được kích hoạt.")]
    public bool isPlaying = true;

    /// <summary>
    /// Sự kiện được kích hoạt khi detector bị phá hủy (hết máu).
    /// Các đối tượng khác có thể lắng nghe sự kiện này để thực hiện hành động tương ứng.
    /// </summary>
    public Action<Detector> OnDetectorDestroyed;

    // === CÁC PHƯƠNG THỨC UNITY ===

    protected override void Awake()
    {
        base.Awake();
        // Tự động lấy transform của camera chính khi khởi tạo
        cameraTransform = Camera.main.transform;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Đăng ký lắng nghe sự kiện nhận sát thương từ BotNetwork
        botNetwork.OnWeaknessTakeDamage += HandleWeaknessDamage;
        botNetwork.OnBotDead += HandleBossDead;
        // Thiết lập lại trạng thái ban đầu mỗi khi detector được kích hoạt
        healthImage.fillAmount = 1f;
        _deadDetector = false;
        _currentHealth = _maxHealth;
        _currentLifeTime = lifeTime;
        _detector.localScale = Vector3.one * _scaleDefault;
        _originalScale = Vector3.one * _scaleDefault;

        // Nếu isPlaying là true, bắt đầu hoạt ảnh
        if (isPlaying)
            Play();

        // Nếu DonActiveFalse là false, bắt đầu coroutine để tự động vô hiệu hóa sau một khoảng thời gian
        if (!DonActiveFalse)
            _ActiveFalseCoroutine = StartCoroutine(DisableThis());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Hủy đăng ký sự kiện để tránh lỗi và rò rỉ bộ nhớ khi đối tượng bị vô hiệu hóa
        botNetwork.OnWeaknessTakeDamage -= HandleWeaknessDamage;
        botNetwork.OnBotDead -= HandleBossDead;
        // Dừng coroutine tự hủy nếu nó đang chạy
        if (_ActiveFalseCoroutine != null)
            StopCoroutine(_ActiveFalseCoroutine);
    }

    private void HandleBossDead()
    {
        gameObject.SetActive(false); // Vô hiệu hóa đối tượng khi Bot chết
    }

    private void Update()
    {
        // Nếu detector không phải là loại "bất tử" và vẫn còn thời gian sống
        if (!DonActiveFalse && _currentLifeTime > 0)
        {
            _currentLifeTime -= Time.deltaTime; // Giảm thời gian sống
            UpdateLifetimeColor(); // Cập nhật màu sắc dựa trên thời gian còn lại
        }
        
        // Luôn xoay detector để nó hướng về phía camera
        if (cameraTransform != null)
        {
            // Gọi phương thức tĩnh từ lớp NUtiliti để thực hiện việc xoay
            AlignCamera(_detector, cameraTransform);
        }
    }

    // === XỬ LÝ SÁT THƯƠNG VÀ MÁU ===

    /// <summary>
    /// Xử lý sự kiện nhận sát thương từ BotNetwork.
    /// </summary>
    private void HandleWeaknessDamage(string targetName, int damage)
    {
        // Chuyển tiếp thông tin sát thương đến một phương thức tĩnh trong lớp NUtiliti.
        // Lưu ý: Logic này không trực tiếp gây sát thương lên detector này.
        // Sát thương thực tế được áp dụng thông qua phương thức `ApplyDamage`.
        HandleWeaknessDamageStatic(targetName, damage);
    }

    /// <summary>
    /// Ghi đè phương thức từ lớp cha để áp dụng sát thương lên detector này.
    /// </summary>
    protected override void ApplyDamage(int damage)
    {
        // Gọi phương thức để giảm máu và cập nhật UI
        SetHealthImage(damage);
        Debug.Log($"[Detector] {gameObject.name} took {damage} dmg");
    }

    /// <summary>
    /// Xử lý việc nhận sát thương, cập nhật máu và UI.
    /// Lưu ý: Tên hàm `SetHealthImage` có thể gây nhầm lẫn, chức năng chính là xử lý sát thương (TakeDamage).
    /// </summary>
    public void SetHealthImage(int damage)
    {
        if (_deadDetector) return; // Nếu đã chết, không xử lý nữa

        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Debug.Log("Detector is dead");
            _deadDetector = true;
            _currentHealth = 0;
            healthImage.fillAmount = 0;
            
            // Kích hoạt sự kiện, thông báo cho các hệ thống khác rằng detector này đã bị phá hủy
            OnDetectorDestroyed?.Invoke(this);

            // Dừng coroutine tự hủy (nếu có) vì detector đã bị phá hủy do hết máu
            if (_ActiveFalseCoroutine != null)
                StopCoroutine(_ActiveFalseCoroutine);
            
            // Vô hiệu hóa đối tượng
            gameObject.SetActive(false);
        }
        else
        {
            // Cập nhật lại thanh máu trên UI
            healthImage.fillAmount = (float)_currentHealth / _maxHealth;
        }
    }

    // === HOẠT ẢNH VÀ HIỆU ỨNG HÌNH ẢNH ===

    /// <summary>
    /// Bắt đầu hoạt ảnh phóng to rồi thu nhỏ.
    /// </summary>
    public void Play()
    {
        // Dừng coroutine cũ nếu nó đang chạy để bắt đầu một cái mới
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _currentCoroutine = StartCoroutine(AnimateScale());
    }

    /// <summary>
    /// Coroutine thực hiện hoạt ảnh: đặt kích thước lớn ban đầu và thu nhỏ dần về kích thước mặc định.
    /// </summary>
    private IEnumerator AnimateScale()
    {
        Vector3 targetScale = _originalScale; // Kích thước đích
        Vector3 startScale = _originalScale * _scaleMultiplier; // Kích thước bắt đầu (phóng to)
        _detector.localScale = startScale;

        // Vòng lặp chạy cho đến khi kích thước hiện tại gần bằng kích thước đích
        while (Vector3.Distance(_detector.localScale, targetScale) > 0.001f)
        {
            // Di chuyển dần kích thước hiện tại về kích thước đích với tốc độ _scaleSpeed
            _detector.localScale = Vector3.MoveTowards(_detector.localScale, targetScale, _scaleSpeed * Time.deltaTime);
            yield return null; // Chờ đến khung hình tiếp theo
        }

        _currentCoroutine = null; // Đánh dấu coroutine đã hoàn thành
    }

    /// <summary>
    /// Coroutine tự động vô hiệu hóa đối tượng sau khi hết `lifeTime`.
    /// </summary>
    private IEnumerator DisableThis()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Cập nhật màu sắc của các thành phần UI dựa trên thời gian sống còn lại.
    /// </summary>
    private void UpdateLifetimeColor()
    {
        if (detectorGradient != null && healthImage != null)
        {
            // Tính toán tỷ lệ phần trăm thời gian sống còn lại (từ 0 đến 1)
            float lifetimePercentage = Mathf.Clamp01(_currentLifeTime / lifeTime);
            // Lấy màu tương ứng từ Gradient
            Color gradientColor = detectorGradient.Evaluate(lifetimePercentage);
            // Áp dụng màu cho các hình ảnh
            healthImage.color = gradientColor;
            inSide1Image.color = gradientColor;
            inSide2Image.color = gradientColor;
        }
    }

    /// <summary>
    /// Kiểm tra xem detector đã bị phá hủy hay chưa.
    /// </summary>
    /// <returns>True nếu đã bị phá hủy, ngược lại là false.</returns>
    public bool IsDestroyed() => _deadDetector;
}