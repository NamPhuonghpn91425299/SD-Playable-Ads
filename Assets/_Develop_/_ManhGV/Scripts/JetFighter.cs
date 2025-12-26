using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class JetFighter : MonoBehaviour
{
    [SerializeField] public VehicleNetwork VehicleNetwork;
    private PointGroup _pointGroup;

    [Header("References")] [SerializeField]
    GameObject _body;

    [SerializeField] GameObject _bodyExplosion;
    [SerializeField] private ExplosionVehicleControl explosionVehicleControl;

    [Header("Waypoint Settings")] [Tooltip("Sử dụng các object con làm waypoint (giống MultiHandleParent)")]
    public bool useChildrenAsWaypoints = true;

    [Tooltip("Danh sách waypoints nếu không dùng children")]
    public List<Transform> pathPoints = new List<Transform>();

    [Header("Movement Settings")] public float speed = 10f;
    [Tooltip("Tự động bay khi Start")] public bool autoStart = true;

    [Header("Rotation")] public bool enableRotation = true;
    public float rotationSpeed = 5f;

    [Header("Banking (Z-Rotation)")] [Tooltip("Bật tính năng nghiêng khi rẽ")]
    public bool enableBanking = true;

    [Tooltip("Góc nghiêng tối đa (độ)")] public float maxBankAngle = 45f;

    [Tooltip("Độ nhạy của banking (càng cao càng nghiêng nhanh)")]
    public float bankSensitivity = 50f;

    [Tooltip("Tốc độ chuyển đổi banking")] public float bankSpeed = 3f;

    [Header("Debug")] public bool showGizmos = true;
    public Color pathColor = Color.cyan;
#if UNITY_EDITOR
    [Tooltip("Bật/tắt Debug.Log trong Editor")]
    public bool enableDebugLogs = true;
#endif

    private List<Transform> activeWaypoints = new List<Transform>();

    // Cache Transform component để tăng hiệu suất
    private Transform cachedTransform;

    // Reusable list để tránh tạo mới trong OnDrawGizmos
    private List<Transform> cachedPreviewWaypoints;
    private int currentIndex = 0;
    private bool isMoving = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isPerformingManeuver = false;
    private bool bankingWasEnabled = false;

    [Header("Crash Settings")] [Tooltip("Layer mặc định của mặt đất")]
    public LayerMask groundLayer = 1 << 0;

    [Tooltip("Tốc độ lúc rơi xuống")] public float crashSpeed = 30f;
    [Tooltip("Tốc độ xoay khi rơi")] public float crashSpinRate = 180f;
    private Vector3 crashDirection;
    private bool isCrashing = false;
    private Vector3 crashTargetPosition;
    private float crashTotalTime;

    // Banking variables
    private Vector3 previousDirection = Vector3.forward;
    private float currentBankAngle = 0f;

    [Header("Attack Info")] [SerializeField]
    private GameObject gunFire;

    [SerializeField] private int[] pointAttack; //điền point tấn công phải nhỏ hơn số điểm waypoint

    [Header("Rocket Info")] [SerializeField]
    private Transform[] pointFireRocket;

    void Awake()
    {
        // Cache Transform component để tăng hiệu suất
        cachedTransform = transform;

        // Khởi tạo reusable list
        cachedPreviewWaypoints = new List<Transform>();
        // Lưu trạng thái ban đầu
        initialPosition = cachedTransform.position;
        initialRotation = cachedTransform.rotation;
    }

    /// <summary>
    /// Setup waypoints từ children hoặc manual list
    /// </summary>
    void SetupWaypoints()
    {
        activeWaypoints.Clear();

        if (useChildrenAsWaypoints)
        {
            // Lấy tất cả object con làm waypoint
            foreach (Transform child in cachedTransform)
            {
                activeWaypoints.Add(child);
            }
        }
        else
        {
            // Sử dụng danh sách manual
            activeWaypoints = new List<Transform>(pathPoints);
        }

#if UNITY_EDITOR
        if (activeWaypoints.Count < 3)
        {
            if (enableDebugLogs)
                Debug.LogWarning(
                    $"Cần ít nhất 3 waypoint để tạo đường cong Catmull-Rom! Hiện có: {activeWaypoints.Count}");
        }
#endif
    }

    /// <summary>
    /// Bắt đầu di chuyển
    /// </summary>
    public void StartMoving()
    {
#if UNITY_EDITOR
        if (activeWaypoints.Count < 3)
        {
            if (enableDebugLogs)
                Debug.LogWarning("Cần ít nhất 3 điểm để tạo đường cong Catmull-Rom!");
            return;
        }
#endif

        isMoving = true;
        currentIndex = 0;
        previousDirection = cachedTransform.forward;
        StartCoroutine(MoveAlongSpline());
    }

    /// <summary>
    /// Dừng di chuyển
    /// </summary>
    public void StopMoving()
    {
        isMoving = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Coroutine di chuyển theo Catmull-Rom spline qua tất cả các điểm - version được tối ưu
    /// </summary>
    IEnumerator MoveAlongSpline()
    {
        // Cache Transform component để tăng hiệu suất
        Transform cachedTransform = transform;

        while (isMoving)
        {
            // Lấy 4 điểm cho Catmull-Rom spline chuẩn
            Vector3 p0 = GetWaypointPosition(currentIndex - 1);
            Vector3 p1 = GetWaypointPosition(currentIndex);
            Vector3 p2 = GetWaypointPosition(currentIndex + 1);
            Vector3 p3 = GetWaypointPosition(currentIndex + 2);

            // Tính khoảng cách segment để có tốc độ đều - chỉ tính khi cần thiết
            float segmentLength = CalculateSegmentLength(p0, p1, p2, p3, 10); // Giảm samples
            float duration = segmentLength / speed;
            float elapsedTime = 0f;

            // Pre-calculate các giá trị không đổi trong vòng lặp
            float invDuration = 1.0f / duration;

            // Di chuyển từ p1 đến p2 trên spline
            while (elapsedTime < duration)
            {
                float t = elapsedTime * invDuration; // Tránh phép chia trong vòng lặp

                // Tính vị trí trên Catmull-Rom spline
                cachedTransform.position = GetCatmullRomPosition(t, p0, p1, p2, p3);

                // Xoay theo hướng di chuyển với banking
                if (enableRotation)
                {
                    Vector3 tangent = GetCatmullRomTangent(t, p0, p1, p2, p3);
                    // Kiểm tra độ dài vector thay vì so sánh với Vector3.zero để tăng hiệu suất
                    if (tangent.sqrMagnitude > 0.0001f)
                    {
                        ApplyRotationWithBanking(tangent);
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Hoàn thành segment - đã đến waypoint đích (p2)
            int reachedWaypointIndex = (currentIndex + 1) % activeWaypoints.Count;
            OnWaypointReached(reachedWaypointIndex);

            // Chuyển sang segment tiếp theo
            currentIndex = (currentIndex + 1) % activeWaypoints.Count;
        }
    }

    /// <summary>
    /// Áp dụng rotation với banking (nghiêng khi rẽ) - version được tối ưu
    /// </summary>
    void ApplyRotationWithBanking(Vector3 direction)
    {
        // Cache Transform component
        Transform cachedTransform = transform;

        // Nếu đang thực hiện roll, chỉ xoay theo hướng mà không banking
        if (isPerformingManeuver)
        {
            Quaternion maneuverRotation = Quaternion.LookRotation(direction);
            cachedTransform.rotation = Quaternion.Slerp(
                cachedTransform.rotation,
                maneuverRotation,
                rotationSpeed * Time.deltaTime
            );
            previousDirection = direction;
            return;
        }

        // Tính toán banking angle
        float targetBankAngle = 0f;

        if (enableBanking)
        {
            // Tính độ thay đổi hướng (chỉ trên mặt phẳng ngang) - tối ưu để tránh tạo mới Vector3
            // Thay vì tạo new Vector3 và normalized, ta sử dụng phép toán trực tiếp
            float currentDirX = direction.x;
            float currentDirZ = direction.z;
            float currentDirMagnitude = Mathf.Sqrt(currentDirX * currentDirX + currentDirZ * currentDirZ);

            float previousDirX = previousDirection.x;
            float previousDirZ = previousDirection.z;
            float previousDirMagnitude = Mathf.Sqrt(previousDirX * previousDirX + previousDirZ * previousDirZ);

            if (currentDirMagnitude > 0.0001f && previousDirMagnitude > 0.0001f)
            {
                // Normalize
                currentDirX /= currentDirMagnitude;
                currentDirZ /= currentDirMagnitude;
                previousDirX /= previousDirMagnitude;
                previousDirZ /= previousDirMagnitude;

                // Tính góc rẽ (dương = rẽ phải, âm = rẽ trái)
                // Sử dụng công thức tính góc giữa 2 vector 2D
                float dot = currentDirX * previousDirX + currentDirZ * previousDirZ;
                float cross = currentDirX * previousDirZ - currentDirZ * previousDirX;
                float turnAngle = Mathf.Atan2(cross, dot) * Mathf.Rad2Deg;

                // Chuyển đổi góc rẽ thành bank angle
                targetBankAngle = turnAngle * bankSensitivity;
                targetBankAngle = Mathf.Clamp(targetBankAngle, -maxBankAngle, maxBankAngle);
            }
        }

        // Smooth banking transition
        currentBankAngle = Mathf.Lerp(currentBankAngle, targetBankAngle, bankSpeed * Time.deltaTime);

        // Tạo rotation chính (look direction)
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Thêm bank rotation (xoay quanh trục Z local) - tối ưu bằng cách sử dụng Quaternion.Euler
        Quaternion targetRotation = lookRotation * Quaternion.Euler(0, 0, currentBankAngle);

        // Áp dụng rotation với smooth transition
        cachedTransform.rotation = Quaternion.Slerp(
            cachedTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // Cập nhật previous direction
        previousDirection = direction;
    }

    /// <summary>
    /// Lấy vị trí waypoint với wrap-around cho loop
    /// </summary>
    Vector3 GetWaypointPosition(int index)
    {
        // Wrap index để tạo vòng lặp
        while (index < 0) index += activeWaypoints.Count;
        index = index % activeWaypoints.Count;

        return activeWaypoints[index].position;
    }

    /// <summary>
    /// Tính khoảng cách của một segment spline - version được tối ưu
    /// </summary>
    float CalculateSegmentLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples)
    {
        // Sử dụng caching để tránh tính toán lặp lại nếu các điểm không thay đổi
        // Tính xấp xỉ đơn giản hơn để giảm chi phí tính toán
        float length = 0f;
        Vector3 prevPos = p1;

        // Giảm số samples để tăng hiệu suất, chấp nhận độ chính xác thấp hơn một chút
        int reducedSamples = Mathf.Max(5, samples / 2);

        for (int i = 1; i <= reducedSamples; i++)
        {
            float t = i / (float)reducedSamples;
            Vector3 pos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            // Sử dụng phép tính nhanh hơn thay vì Vector3.Distance
            Vector3 diff = pos - prevPos;
            length += Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y + diff.z * diff.z);

            prevPos = pos;
        }

        return length;
    }

    /// <summary>
    /// Catmull-Rom Spline chuẩn với 4 điểm - đường cong đi qua p1 và p2
    /// </summary>
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Công thức Catmull-Rom spline chuẩn
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    /// <summary>
    /// Tính hướng di chuyển trên spline (đạo hàm)
    /// </summary>
    Vector3 GetCatmullRomTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Đạo hàm của Catmull-Rom spline
        float t2 = t * t;

        return 0.5f * (
            (-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2
        );
    }

#if UNITY_EDITOR
    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Setup waypoints cho preview trong Editor
        List<Transform> previewWaypoints = new List<Transform>();

        if (useChildrenAsWaypoints)
        {
            foreach (Transform child in transform)
            {
                previewWaypoints.Add(child);
            }
        }
        else
        {
            previewWaypoints = new List<Transform>(pathPoints);
        }

        if (previewWaypoints.Count < 3) return;

        // Vẽ các waypoint
        Gizmos.color = Color.red;
        for (int i = 0; i < previewWaypoints.Count; i++)
        {
            if (previewWaypoints[i] != null)
            {
                Gizmos.DrawWireSphere(previewWaypoints[i].position, 0.3f);

                UnityEditor.Handles.Label(previewWaypoints[i].position + Vector3.up * 0.5f, i.ToString());
            }
        }

        // Vẽ đường cong Catmull-Rom
        Gizmos.color = pathColor;
        int resolution = 10;

        for (int i = 0; i < previewWaypoints.Count; i++)
        {
            // Lấy 4 điểm với wrap-around
            int i0 = (i - 1 + previewWaypoints.Count) % previewWaypoints.Count;
            int i1 = i;
            int i2 = (i + 1) % previewWaypoints.Count;
            int i3 = (i + 2) % previewWaypoints.Count;

            if (previewWaypoints[i0] == null || previewWaypoints[i1] == null ||
                previewWaypoints[i2] == null || previewWaypoints[i3] == null)
                continue;

            Vector3 p0 = previewWaypoints[i0].position;
            Vector3 p1 = previewWaypoints[i1].position;
            Vector3 p2 = previewWaypoints[i2].position;
            Vector3 p3 = previewWaypoints[i3].position;

            Vector3 prevPos = p1;
            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 pos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }

        // Hiển thị điểm hiện tại khi chạy
        if (Application.isPlaying && isMoving && currentIndex < activeWaypoints.Count)
        {
            Gizmos.color = Color.cyan;
            if (activeWaypoints[currentIndex] != null)
            {
                Gizmos.DrawWireSphere(activeWaypoints[currentIndex].position, 0.5f);
            }
        }

        // Hiển thị banking angle trong Scene view
        if (Application.isPlaying && enableBanking)
        {
            Gizmos.color = Color.yellow;
            Vector3 bankDirection = transform.right * (currentBankAngle / maxBankAngle);
            Gizmos.DrawRay(transform.position, bankDirection);
        }
    }
#endif

    /// <summary>
    /// Refresh waypoints khi thay đổi settings
    /// </summary>
    public void RefreshWaypoints()
    {
        SetupWaypoints();
    }

    /// <summary>
    /// Reset movement
    /// </summary>
    public void RestartMovement()
    {
        StopMoving();
        currentIndex = 0;
        currentBankAngle = 0f;
        previousDirection = cachedTransform.forward;
        SetupWaypoints();
        if (activeWaypoints.Count >= 3)
        {
            StartMoving();
        }
    }

    /// <summary>
    /// Hàm gọi khi máy bay chết - dừng tất cả di chuyển và lao xuống
    /// </summary>
    /// <param name="hitPoint">Điểm bị bắn trúng</param>
    /// <param name="groundLayerMask">Layer của mặt đất</param>
    /// <param name="stopDistanceFromGround">Khoảng cách dừng cách mặt đất</param>
    public void OnDead(Vector3 hitPoint, LayerMask groundLayerMask, float stopDistanceFromGround = 0f)
    {
        if (isCrashing) return; // Đang crash rồi thì không gọi lại

#if UNITY_EDITOR
        if (enableDebugLogs)
            Debug.Log($"[Su30_Clean] OnDead() - Máy bay {gameObject.name} bị bắn trúng tại {hitPoint}!");
#endif

        // Dừng tất cả di chuyển
        isMoving = false;
        StopAllCoroutines();

        // Reset các biến trạng thái
        currentBankAngle = 0f;

        // Lưu layer ground và bắt đầu crash
        groundLayer = groundLayerMask;
        StartCrash(stopDistanceFromGround);
        _body.SetActive(false);
        _bodyExplosion.SetActive(true);
        explosionVehicleControl.TriggerExplosion(0);
    }

    /// <summary>
    /// OnDead với layer mặc định
    /// </summary>
    public void OnDead()
    {
        OnDead(cachedTransform.position, groundLayer, 1f);
        _pointGroup = null;
        VehicleNetwork.botIdentity.AssignedPath = null;
    }

    public void OnDespawn(float timer)
    {
        waitDeaspawn(timer);
    }

    private IEnumerator waitDeaspawn(float timer)
    {
        yield return HelperCoroutine.GetWait(timer);
        explosionVehicleControl.ResetAllExplosions();
    }

    private Coroutine _attackCorountine;
    /// <summary>
    /// Hàm gọi mỗi khi máy bay đi qua một waypoint
    /// </summary>
    void OnWaypointReached(int waypointIndex)
    {
#if UNITY_EDITOR
        if (enableDebugLogs)
            Debug.Log($"[Su30_Clean] Đã qua waypoint {waypointIndex} - {activeWaypoints[waypointIndex].name}");
#endif

        // Kiểm tra xem waypoint hiện tại có trong danh sách điểm tấn công không
        bool shouldAttack = false;
        foreach (int attackPoint in pointAttack)
        {
            if (attackPoint == waypointIndex)
            {
                shouldAttack = true;
                break;
            }
        }

        // Kích hoạt hoặc tắt gunFire dựa trên kết quả
        gunFire.SetActive(shouldAttack);
        if (shouldAttack)
        {
            StartCoroutine(Attack_MachineGun());
            _attackCorountine = StartCoroutine(Attack());
        }
        else if(_attackCorountine!=null)
            StopCoroutine(_attackCorountine);
#if UNITY_EDITOR
        if (enableDebugLogs)
            Debug.Log($"[Su30_Clean] Waypoint {waypointIndex} - GunFire: {(shouldAttack ? "ON" : "OFF")}");
#endif

        // Thêm logic khác nếu cần
        // Ví dụ: phát âm thanh, hiệu ứng, kiểm tra mission...
    }

    public IEnumerator Attack_MachineGun()
    {
        int i = 5;
        while (i<=0)
        {
            i--;
            yield return HelperCoroutine.GetWait(.3f);
            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: 50, state: "OnlyDamage"));
        }
    }
    public IEnumerator Attack()
    {
        if (Random.Range(0,10) < 10)
        {
            yield return HelperCoroutine.GetWait(1.8f);
            for (int i = 0; i < pointFireRocket.Length; i++)
            {
                Rocket bullet = SimplePool<GameConstants.ProjectileEnemy>.Spawn<Rocket>(GameConstants.ProjectileEnemy.Projectile_Bullet_Rocket, pointFireRocket[i].position, pointFireRocket[i].rotation);
                bullet.Init(VehicleNetwork.Damage,2f);
                yield return HelperCoroutine.GetWait(.5f);
            }
        }

        _attackCorountine = null;
    }
    
    /// <summary>
    /// Reset về trạng thái ban đầu - như mới spawn
    /// </summary>
    public void ResetToInitialState()
    {
        gunFire.SetActive(false);
        explosionVehicleControl.ResetAllExplosions();
        _body.SetActive(true);
        _bodyExplosion.SetActive(false);
#if UNITY_EDITOR
        if (enableDebugLogs)
            Debug.Log($"[Su30_Clean] ResetToInitialState() - Reset về trạng thái ban đầu");
#endif

        // Dừng di chuyển hiện tại
        StopMoving();

        // Reset vị trí và rotation về ban đầu
        cachedTransform.position = initialPosition;
        cachedTransform.rotation = initialRotation;

        // Reset các biến trạng thái
        currentIndex = 0;
        currentBankAngle = 0f;
        previousDirection = initialRotation * Vector3.forward;

        // Setup lại waypoints
        SetupWaypoints();

        // Bắt đầu lại nếu autoStart = true
        if (autoStart && activeWaypoints.Count >= 3)
        {
            StartMoving();
        }
    }

    /// <summary>
    /// Thực hiện xoay 360 độ quanh trục Z (Roll)
    /// Tạm tắt banking khi xoay và bật lại sau khi hoàn thành
    /// </summary>
    /// <param name="duration">Thời gian hoàn thành (giây)</param>
    /// <param name="direction">1 = xoay phải, -1 = xoay trái</param>
    public void PerformRoll360(float duration = 1f, int direction = 1)
    {
        if (isPerformingManeuver)
        {
#if UNITY_EDITOR
            if (enableDebugLogs)
                Debug.LogWarning("[Su30_Clean] Đang thực hiện động tác khác, không thể xoay!");
#endif
            return;
        }

        // Kiểm tra direction
        direction = direction >= 0 ? 1 : -1;

#if UNITY_EDITOR
        if (enableDebugLogs)
            Debug.Log(
                $"[Su30_Clean] Bắt đầu Roll 360 - Duration: {duration}s, Direction: {(direction > 0 ? "Phải" : "Trái")}");
#endif

        StartCoroutine(Roll360Coroutine(duration, direction));
    }

    /// <summary>
    /// Coroutine thực hiện xoay 360 độ - version được tối ưu
    /// </summary>
    private IEnumerator Roll360Coroutine(float duration, int direction)
    {
        isPerformingManeuver = true;

        // Lưu trạng thái banking và tạm tắt
        bankingWasEnabled = enableBanking;
        float savedBankAngle = currentBankAngle;

        if (enableBanking)
        {
            enableBanking = false;
            // Reset bank angle để không bị cộng dồn với roll
            currentBankAngle = 0f;
#if UNITY_EDITOR
            if (enableDebugLogs)
                Debug.Log("[Su30_Clean] Tạm tắt banking và reset bank angle để thực hiện roll");
#endif
        }

        float elapsedTime = 0f;
        float totalRotation = 0f;
        float targetRotation = 360f * direction;

        // Lưu hướng bay hiện tại để xoay quanh nó
        Vector3 rollAxis = cachedTransform.forward;

        // Pre-calculate các giá trị không đổi
        float invDuration = 1.0f / duration;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * invDuration;

            // Sử dụng SmoothStep để xoay mượt hơn
            float smoothT = t * t * (3f - 2f * t); // Thay thế Mathf.SmoothStep để giảm gọi hàm
            float targetAngle = smoothT * targetRotation;
            float angleThisFrame = targetAngle - totalRotation;

            // Xoay quanh trục forward (trục Z local)
            cachedTransform.Rotate(rollAxis, angleThisFrame, Space.World);
            totalRotation = targetAngle;

            // Cập nhật rollAxis nếu hướng bay thay đổi (do di chuyển trên spline)
            rollAxis = cachedTransform.forward;

            yield return null;
        }

        // Đảm bảo xoay đủ 360 độ
        float remainingRotation = targetRotation - totalRotation;
        if (Mathf.Abs(remainingRotation) > 0.01f)
        {
            cachedTransform.Rotate(cachedTransform.forward, remainingRotation, Space.World);
        }

        // Khôi phục trạng thái banking
        if (bankingWasEnabled)
        {
            // Khôi phục bank angle ban đầu (hoặc reset về 0)
            currentBankAngle = 0f; // Reset về 0 để banking tính lại từ đầu
            enableBanking = true;
#if UNITY_EDITOR
            if (enableDebugLogs)
                Debug.Log("[Su30_Clean] Bật lại banking và reset bank angle");
#endif
        }

        isPerformingManeuver = false;
#if UNITY_EDITOR
        if (enableDebugLogs)
            Debug.Log("[Su30_Clean] Hoàn thành Roll 360!");
#endif
    }

    /// <summary>
    /// Thực hiện xoay nhanh 360 độ về trái
    /// </summary>
    public void QuickRollLeft()
    {
        PerformRoll360(1.5f, -1);
    }

    /// <summary>
    /// Tính toán đường rơi trước khi crash
    /// </summary>
    bool CalculateCrashPath(float stopDistanceFromGround)
    {
        Vector3 startPos = cachedTransform.position;
        float timeStep = 0.1f;
        float maxTime = 20f;

        // Dò tìm điểm chạm đất
        for (float t = 0; t < maxTime; t += timeStep)
        {
            // Tính vị trí theo đường parabol
            Vector3 horizontalMove = crashDirection * crashSpeed * t;
            float verticalDrop = 0.5f * Physics.gravity.magnitude * t * t;
            Vector3 testPosition = startPos + horizontalMove - Vector3.up * verticalDrop;

            // Raycast xuống dưới để tìm ground
            RaycastHit hit;
            if (Physics.Raycast(testPosition + Vector3.up * 10f, Vector3.down, out hit, 1000f, groundLayer))
            {
                // Tìm thấy ground
                if (testPosition.y <= hit.point.y + stopDistanceFromGround)
                {
                    crashTargetPosition = hit.point + Vector3.up * stopDistanceFromGround;
                    crashTotalTime = t;

#if UNITY_EDITOR
                    if (enableDebugLogs)
                        Debug.Log(
                            $"[Su30_Clean] Đã tính toán đường rơi, thời gian: {crashTotalTime}s, vị trí mục tiêu: {crashTargetPosition}");
#endif

                    return true;
                }
            }
        }

        return false; // Không tìm thấy ground
    }

    /// <summary>
    /// Thực hiện xoay nhanh 360 độ về Phair
    /// </summary>
    public void QuickRollRight()
    {
        PerformRoll360(1.5f, -1);
    }

    /// <summary>
    /// Bắt đầu crash animation - máy bay lao xuống theo đường cong
    /// </summary>
    void StartCrash(float stopDistanceFromGround = 0f)
    {
        if (isCrashing) return;

        // Tính hướng rơi: hướng hiện tại + xuống dưới
        Vector3 currentForward = cachedTransform.forward;
        Vector3 downDirection = Vector3.down;
        crashDirection = (currentForward + downDirection * 2f).normalized;

        // Tính toán đường rơi trước (raycast để tìm điểm chạm đất)
        if (CalculateCrashPath(stopDistanceFromGround))
        {
            isCrashing = true;
            StartCoroutine(CrashCoroutine(stopDistanceFromGround));
        }
        else
        {
            // Không tìm thấy ground, rơi tự do
#if UNITY_EDITOR
            if (enableDebugLogs)
                Debug.LogWarning("[Su30_Clean] Không tìm thấy ground để crash!");
#endif
            isCrashing = true;
            crashTotalTime = 4f; // Rơi tối đa 4 giây
            crashTargetPosition = cachedTransform.position + Vector3.down * 1000f;
            StartCoroutine(CrashCoroutine(stopDistanceFromGround));
        }
    }

    /// <summary>
    /// Coroutine thực hiện crash animation theo đường đã tính sẵn
    /// </summary>
    IEnumerator CrashCoroutine(float stopDistanceFromGround)
    {
        float elapsedTime = 0f;
        Vector3 startPos = cachedTransform.position;
        Quaternion startRot = cachedTransform.rotation;

        while (isCrashing && elapsedTime < crashTotalTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / crashTotalTime;

            // Tính vị trí theo đường parabol đã tính sẵn
            Vector3 horizontalMove = crashDirection * crashSpeed * elapsedTime;
            float verticalDrop = 0.5f * Physics.gravity.magnitude * elapsedTime * elapsedTime;
            Vector3 currentPosition = startPos + horizontalMove - Vector3.up * verticalDrop;

            // Giới hạn không vượt quá vị trí mục tiêu
            if (currentPosition.y <= crashTargetPosition.y)
            {
                currentPosition.y = crashTargetPosition.y;
                cachedTransform.position = currentPosition;
                isCrashing = false;

#if UNITY_EDITOR
                if (enableDebugLogs)
                    Debug.Log($"[Su30_Clean] Máy bay đã dừng tại vị trí mục tiêu: {currentPosition}");
#endif
                _bodyExplosion.SetActive(false);
                //pathPoints.Clear();
                explosionVehicleControl.TriggerExplosion(1);
                VehicleNetwork.OnDespawn(2f);
                // Optional: Disable hoặc destroy sau khi crash
                // this.enabled = false;
                // Destroy(gameObject, 2f);
                break;
            }

            // Cập nhật vị trí
            cachedTransform.position = currentPosition;

            // Xoay lộn vòng khi rơi
            cachedTransform.Rotate(Vector3.forward * crashSpinRate * Time.deltaTime, Space.Self);
            cachedTransform.Rotate(Vector3.right * crashSpinRate * 0.5f * Time.deltaTime, Space.Self);

            yield return null;
        }

        // Đảm bảo đến đúng vị trí cuối cùng
        if (isCrashing)
        {
            cachedTransform.position = crashTargetPosition;
            isCrashing = false;

#if UNITY_EDITOR
            if (enableDebugLogs)
                Debug.Log($"[Su30_Clean] Hoàn thành crash tại: {crashTargetPosition}");
#endif
        }
    }


    public void OnInit()
    {
        StopAllCoroutines();
        _pointGroup = VehicleNetwork.botIdentity.AssignedPath;
        if (_pointGroup == null)
        {
            OnInit();
            return;
        }
        pathPoints = _pointGroup.points;
        if (autoStart && activeWaypoints.Count >= 3)
            StartMoving();
        SetupWaypoints();
        ResetToInitialState();
        RestartMovement();
    }
}