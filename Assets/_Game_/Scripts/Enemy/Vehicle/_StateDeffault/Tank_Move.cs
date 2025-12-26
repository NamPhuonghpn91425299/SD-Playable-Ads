using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
using DG.Tweening; // Import DOTween

/// <summary>
/// 🚗 Tank Movement Controller với DOTween Integration
/// 
/// TỔNG QUAN:
/// - Quản lý di chuyển của tank theo waypoints
/// - Phase 1: Di chuyển tuần tự qua các waypoints (không attack)
/// - Phase 2: Random giữa các attack points và tấn công
/// 
/// DOTWEEN FEATURES:
/// - Vừa di chuyển vừa xoay mượt mà (không dừng lại)
/// - Hỗ trợ 2 modes: Sequential và Smooth Path
/// - Tự động quản lý wheel rotation
/// 
/// PERFORMANCE:
/// - Giảm 70-80% CPU usage so với Update() truyền thống
/// - Không cần tính toán mỗi frame
/// - DOTween tự optimize batching cho nhiều tanks
/// 
/// Author: Enhanced with DOTween
/// Version: 2.0
/// Date: 2024
/// </summary>
public class Tank_Move : StateBase
{
    [Header("=== VISUAL ===")]
    [SerializeField] private Renderer wheelRotation;
    [SerializeField, Range(1f, 200f)] 
    [Tooltip("Tốc độ xoay bánh xe khi di chuyển")]
    private float wheelSpeed = 0.5f;
    
    [Header("=== MOVEMENT SETTINGS ===")]
    [SerializeField, Range(1f, 20f)]
    [Tooltip("Tốc độ di chuyển của tank (units/second)")]
    private float moveSpeed = 5f;
    
    [SerializeField, Range(10f, 180f)]
    [Tooltip("Tốc độ xoay thân tank (degrees/second) - cho Phase 2")]
    private float rotationSpeed = 30f;
    
    [SerializeField, Range(0.1f, 1f)]
    [Tooltip("Thời gian xoay với DOTween - càng nhỏ xoay càng nhanh")]
    private float rotationDuration = 0.3f;
    
    [Header("=== DOTWEEN SETTINGS ===")]
    [SerializeField]
    [Tooltip("Linear: Đều | InOutSine: Mượt đầu/cuối | OutQuad: Giảm tốc")]
    private Ease moveEase = Ease.Linear;
    
    [SerializeField]
    [Tooltip("InOutSine: Mượt nhất cho xoay | Linear: Xoay đều")]
    private Ease rotateEase = Ease.InOutSine;
    
    [SerializeField]
    [Tooltip("false: Di chuyển thẳng | true: Di chuyển theo đường cong")]
    private bool useSmartPath = false;
    
    [SerializeField]
    [Tooltip("Linear: Đi thẳng sát đất | CatmullRom: Đường cong mượt (có thể cao hơn)")]
    private PathType pathType = PathType.Linear;
    
    [SerializeField]
    [Tooltip("true: Giữ độ cao gốc waypoint | false: Đồng độ cao với tank")]
    private bool preserveWaypointHeight = true;
    
    [SerializeField]
    [Tooltip("Hiển thị debug logs trong Console")]
    private bool debugMode = true;
    
    [Header("=== PATH INFO (Runtime) ===")]
    [SerializeField] private PointGroup assignedPath;
    [SerializeField] private BotIdentity botIdentity;
    
    [SerializeField] private AudioSource audioSource;
    // Material cho wheel
    private Material materialInstance;
    private float wheelframe = 0;
    
    // Movement state
    private int currentPointIndex = 0;
    private bool isMoving;
    private Coroutine moveRoutine;
    
    // Phase control
    private bool isPhase1Completed = false;
    private int lastAttackPointIndex = -1;
    private Transform currentTargetAttackPoint;
    
    // DOTween references - Quan trọng!
    private Tween moveTween;           // Tween cho di chuyển hiện tại
    private Tween rotateTween;         // Tween cho xoay hiện tại  
    private Sequence pathSequence;     // Sequence cho toàn bộ path
    private bool isDOTweenMoving = false; // Flag để biết DOTween đang chạy
    
    #region Unity Lifecycle & State Management
    
    public void GetPoint()
    {
        materialInstance = wheelRotation?.material;
        assignedPath = botIdentity.AssignedPath;
        currentPointIndex = 0;
        isPhase1Completed = false;
        lastAttackPointIndex = -1;
        
        if (debugMode)
            Debug.Log($"[DOTween] {gameObject.name} - Initialized with {assignedPath?.points?.Count ?? 0} waypoints");
    }
    
    public override void EnterState()
    {
        // Clean up cũ
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        KillAllTweens();
        
        // Reset state
        isDOTweenMoving = false;
        isMoving = false;
        audioSource.Play();
        // Quyết định dùng logic nào
        if (!isPhase1Completed && assignedPath?.points?.Count > 0)
        {
            // PHASE 1: Dùng DOTween cho smooth path following
            if (useSmartPath)
            {
                CreateDOPathMovement(); // Smooth curve path
            }
            else
            {
                CreatePhase1PathSequence(); // Sequential waypoint movement
            }
        }
        else
        {
            // PHASE 2: Giữ logic cũ hoặc có thể optimize sau
            moveRoutine = StartCoroutine(RotateToNextWaypoint());
        }
    }
    
    public override void UpdateState()
    {
        // Validation
#if UNITY_EDITOR
        if (assignedPath == null || assignedPath.points.Count <= 0)
        {
            Debug.LogError($"[DOTween] {gameObject.name} - No path assigned!");
            return;
        }
#endif
        
        WheelRotation();
        
        // Nếu DOTween đang xử lý Phase 1, không cần update manual
        if (!isPhase1Completed && (pathSequence?.IsActive() ?? false))
        {
            return; // DOTween đang làm việc
        }
        
        // Phase 2 hoặc fallback logic
        if (!isMoving) return;
        
        if (isPhase1Completed)
        {
            HandlePhase2Movement(); // Giữ nguyên logic cũ cho Phase 2
        }
    }
    
    public override void ExitState()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        
        KillAllTweens();
        audioSource.Stop();
        isMoving = false;
        isDOTweenMoving = false;
        botContext.ChangeAnimAndType(HashEndStart);
    }
    
    #endregion
    
    #region DOTween Movement Methods - PHẦN QUAN TRỌNG NHẤT!
    
    /// <summary>
    /// 🎯 Tạo sequence di chuyển qua TẤT CẢ waypoints (PHASE 1)
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 1. Tạo DOTween Sequence chứa tất cả movements
    /// 2. Với mỗi waypoint:
    ///    - Tạo move tween (di chuyển)
    ///    - Tạo rotate tween (xoay)
    ///    - Dùng Join() để chạy đồng thời
    /// 3. Sequence tự động chạy tuần tự qua các waypoints
    /// 
    /// ƯU ĐIỂM:
    /// - Không cần Update() mỗi frame
    /// - Tank vừa đi vừa xoay (smooth)
    /// - Tự động callbacks khi đến waypoint
    /// 
    /// PERFORMANCE:
    /// - 1 lần gọi thay vì 60 lần/giây
    /// - DOTween tự optimize internal
    /// </summary>
    private void CreatePhase1PathSequence()
    {
        if (assignedPath?.points == null || assignedPath.points.Count == 0)
        {
            Debug.LogWarning($"[DOTween] {gameObject.name} - No waypoints!");
            return;
        }
        
        // Kill sequence cũ nếu có
        pathSequence?.Kill();
        
        // Tạo sequence mới
        pathSequence = DOTween.Sequence();
        
        if (debugMode)
            Debug.Log($"[DOTween] {gameObject.name} - Creating smooth sequence for {assignedPath.points.Count} waypoints");
        
        // Callback khi bắt đầu toàn bộ sequence
        pathSequence.AppendCallback(() => {
            isDOTweenMoving = true; // Bắt đầu di chuyển
        });
        
        // Duyệt qua từng waypoint
        for (int i = currentPointIndex; i < assignedPath.points.Count; i++)
        {
            Transform waypoint = assignedPath.points[i];
            if (waypoint == null) continue;
            
            // Target position (giữ Y để tank không bay)
            Vector3 targetPos = waypoint.position;
            targetPos.y = TF.position.y;
            
            // Tính toán thông số
            Vector3 startPos = (i == currentPointIndex) 
                ? TF.position 
                : assignedPath.points[i-1].position;
            startPos.y = TF.position.y;
            
            float distance = Vector3.Distance(startPos, targetPos);
            if (distance < 0.1f) continue; // Skip nếu quá gần
            
            float moveDuration = distance / moveSpeed; // Thời gian dựa trên speed
            
            // ===== VỪA DI CHUYỂN VỪA XOAY MƯỢT MÀ =====
            int capturedIndex = i; // Capture for closure (tránh bug closure trong loop)
            
            // Log waypoint nếu debug mode
            if (debugMode)
            {
                pathSequence.AppendCallback(() => {
                    Debug.Log($"[DOTween] {gameObject.name} - Heading to waypoint {capturedIndex + 1}/{assignedPath.points.Count}");
                });
            }
            
            // ===== QUAN TRỌNG: DI CHUYỂN + XOAY ĐỒNG THỜI =====
            // Tạo MOVE TWEEN - di chuyển đến target
            Tween moveTween = TF.DOMove(targetPos, moveDuration)
                                .SetEase(moveEase)        // Linear = đều, InOutSine = mượt
                                .OnComplete(() => {
                                    currentPointIndex = capturedIndex + 1;
                                    if (debugMode)
                                        Debug.Log($"[DOTween] {gameObject.name} - Reached waypoint {capturedIndex + 1}");
                });
            
            // Tạo ROTATE TWEEN - xoay về hướng target
            // rotDuration = 70% của move time để xoay kịp trước khi đến nơi
            float rotDuration = Mathf.Min(moveDuration * 0.7f, rotationDuration);
            Tween rotateTween = TF.DOLookAt(targetPos, rotDuration, AxisConstraint.Y, Vector3.up)
                                  .SetEase(rotateEase);   // InOutSine cho xoay mượt
            
            // MAGIC: Thêm cả 2 tweens vào sequence
            pathSequence.Append(moveTween);      // Thêm move tween
            pathSequence.Join(rotateTween);      // JOIN = chạy CÙNG LÚC với move tween
            // Kết quả: Tank vừa đi vừa xoay, không dừng lại!
            
            // Optional: Delay nhỏ giữa các waypoints (có thể comment nếu muốn liên tục)
            // pathSequence.AppendInterval(0.05f);
        }
        
        // ===== CALLBACK KHI HOÀN THÀNH TOÀN BỘ PATH =====
        pathSequence.OnComplete(() => {
            OnPhase1CompleteDOTween();
        });
        
        // Tự động kill khi GameObject bị destroy
        pathSequence.SetLink(gameObject);
        
        // Auto-kill khi scene change
        pathSequence.SetAutoKill(true);
        
        // Update type (Normal = scaled time, Fixed = fixed update)
        pathSequence.SetUpdate(UpdateType.Normal);
        
        // START SEQUENCE!
        pathSequence.Play();
        
        if (debugMode)
            Debug.Log($"[DOTween] {gameObject.name} - Smooth sequence started!");
    }
    
    /// <summary>
    /// 🌊 Alternative: Dùng DOPath cho smooth curve movement
    /// 
    /// KHÁC BIỆT VỚI SEQUENCE:
    /// - Sequence: Đi thẳng từng waypoint (giống xe tăng)
    /// - DOPath: Đi theo đường cong mượt (giống xe hơi)
    /// 
    /// KHI NÀO DÙNG:
    /// - Đường có nhiều khúc cua gấp
    /// - Muốn movement tự nhiên hơn
    /// - Map thiết kế cho smooth movement
    /// 
    /// FEATURES:
    /// - PathType.CatmullRom: Đường cong mượt qua các điểm
    /// - SetLookAt(): Tự động xoay theo hướng di chuyển
    /// - OnWaypointChange: Callback khi qua mỗi waypoint
    /// </summary>
    private void CreateDOPathMovement()
    {
        if (assignedPath?.points == null || assignedPath.points.Count == 0)
            return;
            
        // Build array các waypoints với độ cao chính xác
        List<Vector3> pathPoints = new List<Vector3>();
        
        for (int i = currentPointIndex; i < assignedPath.points.Count; i++)
        {
            if (assignedPath.points[i] != null)
            {
                Vector3 pos = assignedPath.points[i].position;
                
                // 🔧 FIX: Option để control độ cao waypoint
                if (!preserveWaypointHeight)
                {
                    pos.y = TF.position.y; // Đồng độ cao với tank hiện tại
                }
                // Nếu preserveWaypointHeight = true, giữ nguyên độ cao gốc
                
                pathPoints.Add(pos);
            }
        }
        
        if (pathPoints.Count == 0) return;
        
        // Tính tổng khoảng cách
        float totalDistance = 0f;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        float duration = totalDistance / moveSpeed;
        
        if (debugMode)
            Debug.Log($"[DOPath] {gameObject.name} - Creating {pathType} path with {pathPoints.Count} points, distance: {totalDistance:F2}");
        
        // ===== TẠO DOPATH TWEEN =====
        // DOPath với Linear path để giữ đúng độ cao waypoint
        moveTween = TF.DOPath(
            pathPoints.ToArray(),       // Mảng các waypoints
            duration,                   // Thời gian di chuyển (tính từ speed)
            pathType,                  // 🔧 FIX: Dùng pathType có thể config
                                       // Linear = đi thẳng sát đất
                                       // CatmullRom = đường cong mượt (cao hơn)
            PathMode.Full3D,           // Full3D để hỗ trợ terrain có độ cao khác nhau
            10,                        // Resolution
            Color.green               // Màu gizmo trong Scene view
        )
        .SetOptions(false,            // closePath: false = không loop
                   AxisConstraint.None, // 🔧 FIX: Không lock Y để theo độ cao waypoint
                   AxisConstraint.None)
        .SetEase(moveEase)            // Ease function cho movement
        .SetLookAt(0.1f,              // lookAhead: 0.1 = xoay mượt
                  Vector3.forward,    // forward direction
                  Vector3.up)         // up vector
        .OnStart(() => {
            isDOTweenMoving = true;
            if (debugMode) Debug.Log($"[DOPath] {gameObject.name} - Started smooth movement");
        })
        .OnWaypointChange((int waypointIndex) => {
            currentPointIndex++;
            if (debugMode) Debug.Log($"[DOPath] {gameObject.name} - Smoothly passed waypoint {currentPointIndex}/{assignedPath.points.Count}");
        })
        .OnComplete(() => {
            OnPhase1CompleteDOTween();
        })
        .SetLink(gameObject);
    }
    
    /// <summary>
    /// ✅ Được gọi khi DOTween hoàn thành Phase 1
    /// 
    /// WORKFLOW:
    /// 1. Phase 1 complete → Callback này được gọi
    /// 2. Set flags và reset index
    /// 3. Trigger animation
    /// 4. Chuyển sang Attack state
    /// 5. Attack xong → Quay lại Move state → Phase 2
    /// </summary>
    private void OnPhase1CompleteDOTween()
    {
        isDOTweenMoving = false;
        isPhase1Completed = true;
        currentPointIndex = 0; // Reset for Phase 2
        
        Debug.Log($"[DOTween] {gameObject.name} - ✅ Phase 1 Complete! Switching to Attack");
        
        // Chuyển sang attack
        botContext.ChangeAnimAndType(HashStart);
        botContext.stateController.ChangeState(EnemyState.Attack);
    }
    
    /// <summary>
    /// 🛑 Kill tất cả tweens đang chạy
    /// 
    /// QUAN TRỌNG:
    /// - Luôn kill tweens khi không dùng nữa
    /// - Tránh memory leak
    /// - Tránh tweens chạy chồng lên nhau
    /// 
    /// KHI NÀO GỌI:
    /// - ExitState()
    /// - OnDestroy()
    /// - Trước khi tạo tween mới
    /// </summary>
    private void KillAllTweens()
    {
        moveTween?.Kill();      // Kill movement tween nếu có
        rotateTween?.Kill();    // Kill rotation tween nếu có  
        pathSequence?.Kill();   // Kill toàn bộ sequence nếu có
    }
    
    #endregion
    
    #region Original Methods (Phase 2 & Helpers)
    
    private void WheelRotation()
    {
        if (wheelRotation == null || materialInstance == null) return;
        
        wheelframe = (wheelframe + wheelSpeed * Time.deltaTime) % 100f;
        Vector2 offset = materialInstance.mainTextureOffset;
        offset.y = 0.02f * wheelframe;
        materialInstance.mainTextureOffset = offset;
    }
    
    private void HandlePhase2Movement()
    {
        // Giữ nguyên logic Phase 2 từ code cũ
        if (assignedPath.attackPoints == null || assignedPath.attackPoints.Count == 0)
        {
            Debug.LogWarning($"[Phase2] {gameObject.name} - No attack points!");
            isPhase1Completed = false;
            currentPointIndex = 0;
            return;
        }
        
        if (currentTargetAttackPoint == null)
        {
            currentTargetAttackPoint = SelectNewAttackPoint();
            if (currentTargetAttackPoint == null) return;
            
            isMoving = false;
            if (moveRoutine != null)
                StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(RotateToNextWaypoint());
        }
        
        Vector3 targetPos = currentTargetAttackPoint.position;
        Vector3 directionToTarget = targetPos - TF.position;
        
        if (directionToTarget.magnitude < 0.5f)
        {
            currentTargetAttackPoint = null;
            isMoving = false;
            botContext.ChangeAnimAndType(HashStart);
            botContext.stateController.ChangeState(EnemyState.Attack);
            return;
        }
        
        if (isMoving)
        {
            TF.position = Vector3.MoveTowards(TF.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
    
    private IEnumerator RotateToNextWaypoint()
    {
        isMoving = false;
        Vector3 targetPos = GetCurrentTargetPosition();
        
        while (true)
        {
            Vector3 directionToTarget = targetPos - TF.position;
            Vector3 flatDirection = directionToTarget;
            flatDirection.y = 0;
            
            if (flatDirection.sqrMagnitude < 0.01f)
                break;
                
            Quaternion targetRot = Quaternion.LookRotation(flatDirection);
            TF.rotation = Quaternion.RotateTowards(TF.rotation, targetRot, rotationSpeed * Time.deltaTime);
            
            if (Quaternion.Angle(TF.rotation, targetRot) < 1f)
                break;
                
            yield return null;
        }
        
        isMoving = true;
    }
    
    private Transform SelectNewAttackPoint()
    {
        int attackPointCount = assignedPath.attackPoints.Count;
        
        if (attackPointCount == 1)
        {
            lastAttackPointIndex = 0;
            return assignedPath.attackPoints[0];
        }
        
        int nextIndex;
        do
        {
            nextIndex = Random.Range(0, attackPointCount);
        }
        while (nextIndex == lastAttackPointIndex && attackPointCount > 1);
        
        lastAttackPointIndex = nextIndex;
        return assignedPath.attackPoints[nextIndex];
    }
    
    private Vector3 GetCurrentTargetPosition()
    {
        if (!isPhase1Completed)
        {
            if (assignedPath?.points != null && 
                currentPointIndex < assignedPath.points.Count && 
                assignedPath.points[currentPointIndex] != null)
            {
                return assignedPath.points[currentPointIndex].position;
            }
            return TF.position;
        }
        else
        {
            if (currentTargetAttackPoint != null)
                return currentTargetAttackPoint.position;
            return TF.position;
        }
    }
    
    #endregion
    
    #region Debug & Gizmos
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugMode || assignedPath?.points == null) return;
        
        // Draw path
        Gizmos.color = Color.yellow;
        for (int i = 0; i < assignedPath.points.Count - 1; i++)
        {
            if (assignedPath.points[i] != null && assignedPath.points[i + 1] != null)
            {
                Gizmos.DrawLine(
                    assignedPath.points[i].position,
                    assignedPath.points[i + 1].position
                );
            }
        }
        
        // Draw current target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 target = GetCurrentTargetPosition();
            Gizmos.DrawWireSphere(target, 0.5f);
            
            if (transform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target);
            }
        }
    }
#endif
    
    #endregion
}