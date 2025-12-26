using UnityEngine;
using Random = UnityEngine.Random;
using Assets._Develop_.ThanhNT.Scripts.Observer;
/// <summary>
/// Điều khiển một GameObject chuyển động theo kiểu lượn sóng như con rắn.
/// Component này độc lập với logic đạn/pháo và có thể gắn vào bất kỳ đối tượng nào
/// cần có đường bay lượn sóng, uyển chuyển.
/// </summary>
public class SnakeMovementController : GameUnit<GameConstants.ProjectileEnemy>
{
    [Header("SÁT THƯƠNG & HIỆU ỨNG")]
    [Tooltip("Sát thương gây ra bởi mỗi tên lửa")]
    [SerializeField] private int m_rocketDamage = 50;
    [Tooltip("Cường độ rung camera khi tên lửa chạm mục tiêu")]
    public float StrengSnakeCam = .025f;
    [Header("Cài Đặt Chuyển Động Kiểu Rắn")]
    [Tooltip("Phần hình ảnh của đối tượng sẽ thực hiện chuyển động lượn sóng. Nếu để trống, transform gốc sẽ được dùng.")]
    [SerializeField] private Transform visualBodyTransform;

    [Tooltip("Mảng các AnimationCurve định nghĩa hình dạng của chuyển động lượn sóng cho mỗi trục. " +
             "Curve[0] cho Ngẩng/Cúi (trục X), Curve[1] cho Quay ngang (trục Y), Curve[2] cho Nghiêng (trục Z). " +
             "Nếu một curve không được gán, trục tương ứng sẽ không bị ảnh hưởng.")]
    [SerializeField] private AnimationCurve[] snakeMoveCurves = new AnimationCurve[3];

    [Tooltip("Mảng các Vector2 dùng để ngẫu nhiên hóa độ lệch thời gian khi tính toán giá trị của mỗi curve. " +
             "Điều này đảm bảo mỗi đối tượng sẽ có một nhịp điệu chuyển động độc đáo. " +
             "Phần tử [0] cho trục X, [1] cho trục Y, [2] cho trục Z.")]
    [SerializeField]
    private Vector2[] curveRandomRange = new Vector2[3]
    {
        new Vector2(0f, 3f),
        new Vector2(0f, 3f),
        new Vector2(0f, 3f)
    };

    [Tooltip("Thời gian mà hiệu ứng chuyển động kiểu rắn sẽ dần yếu đi. " +
             "Chuyển động sẽ mạnh nhất ở lúc đầu và giảm dần theo thời gian này.")]
    [SerializeField] private float snakeMoveFadeDuration = 2f;

    [Header("Cài Đặt Chuyển Động Chung")]
    [Tooltip("Transform của mục tiêu cần đuổi theo. Có thể để trống nếu di chuyển đến một vị trí cố định.")]
    public Transform target;

    [Tooltip("Một vị trí cố định trong thế giới để di chuyển đến nếu 'target' là null.")]
    public Vector3 destination;

    [Tooltip("Tốc độ di chuyển về phía trước của đối tượng.")]
    [SerializeField] private float moveSpeed = 60f;

    [Tooltip("Tốc độ xoay về hướng mục tiêu/vị trí đích.")]
    [SerializeField] private float rotationSpeed = 70f;

    [Header("Cài Đặt Bao Trùm")]
    [Tooltip("Bán kính để random vị trí lệch xung quanh target. Càng lớn thì càng bao trùm.")]
    [SerializeField] private float surroundRadius = 10f;

    [Tooltip("Chiều cao tối đa để random vị trí lệch so với target.")]
    [SerializeField] private float maxHeightOffset = 5f;

    [Tooltip("Thời gian trễ trước khi đối tượng bắt đầu chủ động xoay về phía mục tiêu.")]
    [SerializeField] private float timeToStartChasing = 0.3f;

    [Tooltip("Khoảng cách đến target để tự động set active false.")]
    [SerializeField] private float deactivateDistance = 1f;

    // --- Các biến trạng thái riêng ---
    private float _snakeMoveTimeMultiplier = 1f;
    private float _snakeTimeOffsetX, _snakeTimeOffsetY, _snakeTimeOffsetZ;
    private float _elapsedTime = 0f;
    private Vector3 _randomOffset;

    // Performance optimization caches
    private bool[] _hasValidCurve = new bool[3];
    private float _sqrDeactivateDistance;
    private Transform _cachedVisualBody;
    private Transform _cachedTransform;
    private Vector3 _forwardDirection;

    // Static cache để giảm GC allocation
    private static readonly Vector3[] _offsetCache = new Vector3[20];
    private static int _cacheIndex = 0;

    /// <summary>
    /// Khởi tạo các giá trị ngẫu nhiên cho chuyển động lượn sóng khi đối tượng được tạo.
    /// </summary>
    private void Awake()
    {
        // Cache transforms để tối ưu performance
        _cachedTransform = transform;
        _cachedVisualBody = visualBodyTransform != null ? visualBodyTransform : transform;
        _sqrDeactivateDistance = deactivateDistance * deactivateDistance;

        // Cache curve validation để tránh null check lặp lại
        for (int i = 0; i < 3; i++)
        {
            _hasValidCurve[i] = snakeMoveCurves[i] != null;
        }

        _snakeTimeOffsetX = Random.Range(curveRandomRange[0].x, curveRandomRange[0].y);
        _snakeTimeOffsetY = Random.Range(curveRandomRange[1].x, curveRandomRange[1].y);
        _snakeTimeOffsetZ = Random.Range(curveRandomRange[2].x, curveRandomRange[2].y);
    }

    /// <summary>
    /// Thiết lập mục tiêu mặc định là người chơi và random vị trí lệch khi đối tượng được kích hoạt.
    /// </summary>
    private void OnEnable()
    {
        target = PlayerInstant.Instance?.ExplosionPos.transform;
        _elapsedTime = 0f;
        _snakeMoveTimeMultiplier = 1f;
        RandomizeOffset();
    }

    /// <summary>
    /// Random vị trí lệch xung quanh target để tạo cảm giác bao trùm.
    /// </summary>
    private void RandomizeOffset()
    {
        if (target != null)
        {
            // Dùng cache để giảm GC allocation
            _cacheIndex = (_cacheIndex + 1) % _offsetCache.Length;
            Vector2 randomCircle = Random.insideUnitCircle * surroundRadius;
            _offsetCache[_cacheIndex] = new Vector3(randomCircle.x, Random.Range(0f, maxHeightOffset), randomCircle.y);
            _randomOffset = _offsetCache[_cacheIndex];
        }
        else
        {
            _randomOffset = Vector3.zero;
        }
    }

    /// <summary>
    /// Cập nhật trạng thái chuyển động mỗi frame.
    /// </summary>
    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        // Cache target position để tránh truy cập lặp lại
        Vector3 targetPosition = target ? target.position + _randomOffset : destination;
        Vector3 directionToTarget = targetPosition - _cachedTransform.position;
        float sqrDistanceToDestination = directionToTarget.sqrMagnitude;

        // Sử dụng squared distance để tối ưu performance
        if (sqrDistanceToDestination <= _sqrDeactivateDistance)
        {
            OnDespawn();
            return;
        }

        if (_elapsedTime > timeToStartChasing)
        {
            RotateTowardsTargetOptimized(directionToTarget);
        }

        UpdateSnakeMovementOptimized();
        MoveForwardOptimized();
    }

    /// <summary>
    /// Xoay transform về phía mục tiêu với tốc độ được chỉ định (version tối ưu).
    /// </summary>
    private void RotateTowardsTargetOptimized(Vector3 directionToTarget)
    {
        if (rotationSpeed <= 0) return;

        directionToTarget.Normalize(); // Reuse direction đã tính ở Update
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            _cachedTransform.rotation = Quaternion.Slerp(_cachedTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Cập nhật hiệu ứng chuyển động lượn sóng kiểu rắn (version tối ưu).
    /// Áp dụng các góc xoay local dựa trên AnimationCurve và thời gian.
    /// </summary>
    private void UpdateSnakeMovementOptimized()
    {
        if (snakeMoveFadeDuration > 0)
        {
            _snakeMoveTimeMultiplier -= Time.deltaTime / snakeMoveFadeDuration;
            if (_snakeMoveTimeMultiplier < 0) _snakeMoveTimeMultiplier = 0;
        }

        // Sử dụng cache để tránh null check lặp lại
        float pitch = _hasValidCurve[0] ? snakeMoveCurves[0].Evaluate(_elapsedTime + _snakeTimeOffsetX) * _snakeMoveTimeMultiplier : 0f;
        float yaw = _hasValidCurve[1] ? snakeMoveCurves[1].Evaluate(_elapsedTime + _snakeTimeOffsetY) * _snakeMoveTimeMultiplier : 0f;
        float roll = _hasValidCurve[2] ? snakeMoveCurves[2].Evaluate(_elapsedTime + _snakeTimeOffsetZ) * _snakeMoveTimeMultiplier : 0f;

        _cachedVisualBody.localEulerAngles = new Vector3(pitch, yaw, roll);
    }

    private void OnDespawn()
    {
        SimplePool<GameConstants.ProjectileEnemy>.Spawn<ExplosionPanzerwerfer>(GameConstants.ProjectileEnemy.Explsion, this.transform.position, Quaternion.identity);
        EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: m_rocketDamage, state: "OnlyDamage"));
        EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData { duration = .3f, strength = StrengSnakeCam, vibrato = 15, randomness = 45 }));
        SimplePool<GameConstants.ProjectileEnemy>.Despawn(this);

    }
    /// <summary>
    /// Di chuyển đối tượng về phía trước theo hướng của visual body (version tối ưu).
    /// </summary>
    private void MoveForwardOptimized()
    {
        // Cache forward direction 1 lần mỗi frame
        _forwardDirection = _cachedVisualBody.forward;
        _cachedTransform.position += moveSpeed * Time.deltaTime * _forwardDirection;
    }

    /// <summary>
    /// Thiết lập mục tiêu cần đuổi theo.
    /// </summary>
    /// <param name="newTarget">Transform của mục tiêu mới.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        RandomizeOffset();
    }

    /// <summary>
    /// Thiết lập vị trí đích cố định để di chuyển đến.
    /// </summary>
    /// <param name="newDestination">Vị trí đích mới trong không gian world.</param>
    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        target = null;
    }

    /// <summary>
    /// Thiết lập tốc độ di chuyển của đối tượng.
    /// </summary>
    /// <param name="newSpeed">Tốc độ di chuyển mới.</param>
    public void SetMoveSpeed(float newSpeed) => moveSpeed = newSpeed;

    /// <summary>
    /// Thiết lập tốc độ xoay về hướng mục tiêu.
    /// </summary>
    /// <param name="newRotSpeed">Tốc độ xoay mới (độ/giây).</param>
    public void SetRotationSpeed(float newRotSpeed) => rotationSpeed = newRotSpeed;

    /// <summary>
    /// Thiết lập bán kính bao trùm.
    /// </summary>
    /// <param name="radius">Bán kính mới.</param>
    public void SetSurroundRadius(float radius)
    {
        surroundRadius = radius;
        RandomizeOffset();
    }

    /// <summary>
    /// Thiết lập chiều cao tối đa.
    /// </summary>
    /// <param name="height">Chiều cao mới.</param>
    public void SetMaxHeightOffset(float height)
    {
        maxHeightOffset = height;
        RandomizeOffset();
    }

    /// <summary>
    /// Thiết lập khoảng cách để tự động deactivate.
    /// </summary>
    /// <param name="distance">Khoảng cách mới.</param>
    public void SetDeactivateDistance(float distance)
    {
        deactivateDistance = distance;
        _sqrDeactivateDistance = distance * distance;
    }
}
