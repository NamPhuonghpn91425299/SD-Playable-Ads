using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtilities;
using static GameConstants;

/// <summary>
/// State xử lý tấn công của MechRonin.
/// Flow: Kiểm tra vị trí → Di chuyển (nếu cần) → Dash + Xoay Player → Tấn công → Loop.
/// </summary>
public class MechRonin_Attack_State : StateBase
{
    // Performance Constants
    private const float MIN_DIRECTION_THRESHOLD = 0.0001f;
    private const float ROTATION_SPEED_MULTIPLIER = 0.5f;

    /// <summary>
    /// Các phase chính trong Attack State.
    /// </summary>
    private enum AttackPhase
    {
        /// <summary>Đang di chuyển đến attack point.</summary>
        MovingToPoint,
        /// <summary>Tiếp cận điểm, tăng tốc đột ngột + dash anim.</summary>
        ApproachingPoint,
        /// <summary>Đang tấn công tại attack point.</summary>
        Attacking,
    }

    [Header("Attack Settings")]
    [Tooltip("Tốc độ di chuyển cơ bản khi tiến tới attack point (m/s).")]
    [SerializeField] private float m_attackMoveSpeed = 10f;

    [Tooltip("Tốc độ xoay khi di chuyển bình thường (deg/s).")]
    [SerializeField] private float m_rotationSpeed = 3f;

    [Tooltip("Tốc độ xoay nhanh hơn khi dash (deg/s).")]
    [SerializeField] private float m_fastRotationSpeed = 6f;

    [Tooltip("Khoảng cách tối thiểu để coi như đã tới attack point (m).")]
    [SerializeField] private float m_distanceEnd = 0.5f;

    [Tooltip("Có lặp lại chuỗi attack points sau khi hoàn tất không?")]
    [SerializeField] private bool m_loopAttacks = true;

    [Header("Dash Enhancement")]
    [Tooltip("Khoảng cách từ mục tiêu để bắt đầu tăng tốc dash (m).")]
    [SerializeField] private float m_dashAccelerationDistance = 3f;

    [Tooltip("Hệ số nhân tốc độ khi dash (vd: 2 = gấp đôi tốc độ).")]
    [SerializeField] private float m_dashAccelerationMultiplier = 2f;

    [Header("Animation Timing")]
    [Tooltip("Thời gian dash (giây).")]
    [SerializeField] private float m_dashDuration = 1f;

    [Tooltip("Thời gian attack (giây).")]
    [SerializeField] private float m_attackDuration = 2f;

    [Header("Rotation Settings")]
    [Tooltip("Ngưỡng góc xoay (deg) được coi là đã xoay xong.")]
    [SerializeField] private float m_rotationThreshold = 5f;

    [Header("Debug Info (Runtime)")]
    [SerializeField] private AttackPhase m_currentPhase;
    [SerializeField] private int m_currentAttackIndex = 0;
    [SerializeField] private float m_phaseTimer = 0f;
    public bool m_isLowHealth = false;
    [SerializeField] private bool m_returnFromSpecial = false;

    private List<Transform> AttackPoints = new List<Transform>();
    // --- State runtime ---
    private bool m_isActive = false;
    private float m_sqrDistanceEnd;
    private float m_sqrDashAccelerationDistance;

    // Cached positions
    private Vector3 m_currentTargetPosition;
    private Vector3 m_nextPointPosition;

    // Performance optimizations
    private Transform m_botTransform;
    private Transform m_playerTransform;
    private Vector3 m_playerPosition;
    private bool m_playerValid;

    #region Initialization

    private void Awake()
    {
        if (botContext.botNetwork.IsDead) return;
        CacheReferences();
        CalculateSquaredDistances();
    }

    /// <summary>
    /// Cache các references để tránh truy cập mỗi frame.
    /// </summary>
    private void CacheReferences()
    {
        m_botTransform = botContext.botNetwork.TF;

        // Cache player reference
        if (PlayerInstant.Instance != null && PlayerInstant.Instance.ExplosionPos != null)
        {
            m_playerTransform = PlayerInstant.Instance.ExplosionPos;
            m_playerValid = true;
        }
        else
        {
            m_playerValid = false;
        }
    }

    /// <summary>
    /// Được gọi khi quay lại từ Special State.
    /// </summary>
    public void OnReturnFromSpecial()
    {
        if (botContext.botNetwork.IsDead) return;
        m_returnFromSpecial = true;
        UpdatePlayerCache();
    }

    /// <summary>
    /// Cập nhật player cache - gọi khi cần refresh reference.
    /// </summary>
    private void UpdatePlayerCache()
    {
        if (PlayerInstant.Instance != null && PlayerInstant.Instance.ExplosionPos != null)
        {
            m_playerTransform = PlayerInstant.Instance.ExplosionPos;
            m_playerValid = true;
        }
        else
        {
            m_playerValid = false;
        }
    }

    /// <summary>
    /// Khởi tạo các giá trị bình phương khoảng cách để tối ưu hiệu suất.
    /// </summary>
    private void CalculateSquaredDistances()
    {
        if (botContext.botNetwork.IsDead) return;
        m_sqrDistanceEnd = m_distanceEnd * m_distanceEnd;
        m_sqrDashAccelerationDistance = m_dashAccelerationDistance * m_dashAccelerationDistance;
    }

    #endregion

    #region State Management

    /// <inheritdoc />
    public override void EnterState()
    {
        if (botContext.botNetwork.IsDead) return;

        m_isActive = true;
        UpdatePlayerCache(); // Refresh cache khi enter state
        AttackPoints = botContext.botIdentity.AssignedPath.attackPoints;

        // Nếu vừa quay lại từ Special State → bỏ qua reset index, đi tiếp tới điểm kế
        if (m_returnFromSpecial)
        {
            m_returnFromSpecial = false;

            m_currentAttackIndex++;
            if (m_currentAttackIndex >= AttackPoints.Count)
                m_currentAttackIndex = 0;

            m_currentTargetPosition = AttackPoints[m_currentAttackIndex].position;
            m_currentPhase = AttackPhase.MovingToPoint;
            botContext.SetAnimation(AnimationParameters.Run);
            return;
        }

        // Reset
        m_currentAttackIndex = 0;
        m_phaseTimer = 0f;

        if (AttackPoints == null || AttackPoints.Count == 0)
        {
            Debug.LogWarning("[MechRonin_Attack] No attack points assigned!");
            m_isActive = false;
            return;
        }

        // Cache point đầu tiên
        m_currentTargetPosition = AttackPoints[m_currentAttackIndex].position;

        float sqrDistanceToPoint = (m_botTransform.position - m_currentTargetPosition).sqrMagnitude;

        if (sqrDistanceToPoint <= m_sqrDistanceEnd)
        {
            // Đã ở attack point → tấn công ngay
            m_currentPhase = AttackPhase.Attacking;
            botContext.SetAnimation(AnimationParameters.AttackGun);
        }
        else
        {
            // Di chuyển tới attack point
            m_currentPhase = AttackPhase.MovingToPoint;
            botContext.SetAnimation(AnimationParameters.Run);
        }
    }

    /// <inheritdoc />
    public override void UpdateState()
    {
        // Early exit optimization - combine all checks
        if (!m_isActive || botContext.botNetwork == null || botContext.botNetwork.IsDead || botContext.botNetwork.IsDeadExplosion)
            return;

        float deltaTime = Time.deltaTime;
        m_phaseTimer += deltaTime;

        switch (m_currentPhase)
        {
            case AttackPhase.MovingToPoint: UpdateMovingPhase(); break;
            case AttackPhase.ApproachingPoint: UpdateApproachingPointPhase(); break;
            case AttackPhase.Attacking: UpdateAttackingPhase(); break;
        }
    }

    /// <inheritdoc />
    public override void ExitState()
    {
        // Luôn cleanup state dù bot có chết hay không
        m_isActive = false;
        m_currentPhase = AttackPhase.MovingToPoint;
        m_phaseTimer = 0f;
    }

    #endregion

    #region Phase Updates

    /// <summary>
    /// Phase 1: Di chuyển đến attack point (xoay và move mượt).
    /// </summary>
    private void UpdateMovingPhase()
    {
        if (botContext.botNetwork.IsDead) return;

        float sqrDistance = (m_botTransform.position - m_currentTargetPosition).sqrMagnitude;

        if (sqrDistance <= m_sqrDashAccelerationDistance)
        {
            TransitionToPhase(AttackPhase.ApproachingPoint);
            return;
        }

        MoveAndRotate(m_currentTargetPosition, m_attackMoveSpeed, m_rotationSpeed);

        if (IsAtTarget(m_currentTargetPosition))
            TransitionToPhase(AttackPhase.Attacking);
    }

    /// <summary>
    /// Phase 2: Tiếp cận mục tiêu với dash (tăng tốc và xoay theo player).
    /// </summary>
    private void UpdateApproachingPointPhase()
    {
        if (botContext.botNetwork.IsDead) return;

        float sqrDistance = (m_botTransform.position - m_currentTargetPosition).sqrMagnitude;

        // Tối ưu: Tránh sqrt bằng cách dùng sqrDistance cho InverseLerp
        float accelerationFactor = Mathf.InverseLerp(0f, m_sqrDashAccelerationDistance, sqrDistance);
        float currentSpeed = m_attackMoveSpeed * (1f + (m_dashAccelerationMultiplier - 1f) * accelerationFactor);

        if (m_playerValid)
            MoveAndRotate(m_currentTargetPosition, currentSpeed, m_fastRotationSpeed, m_playerTransform.position);
        else
            MoveAndRotate(m_currentTargetPosition, currentSpeed, m_fastRotationSpeed);

        if (IsAtTarget(m_currentTargetPosition) && !m_isLowHealth)
            TransitionToPhase(AttackPhase.Attacking);
        else if (IsAtTarget(m_currentTargetPosition) && m_isLowHealth)
            botContext.stateController.ChangeState(EnemyState.Special);
    }

    /// <summary>
    /// Phase 3: Attack tại điểm hiện tại.
    /// </summary>
    private void UpdateAttackingPhase()
    {
        if (botContext.botNetwork.IsDead) return;

        if (m_playerValid)
        {
            m_playerPosition = m_playerTransform.position;
            RotateOnly(m_playerPosition, m_rotationSpeed * ROTATION_SPEED_MULTIPLIER);
        }

        if (m_phaseTimer >= m_attackDuration)
        {
            m_currentAttackIndex++;

            if (m_currentAttackIndex >= AttackPoints.Count)
            {
                if (m_loopAttacks) m_currentAttackIndex = 0;
                else { m_isActive = false; return; }
            }

            m_nextPointPosition = AttackPoints[m_currentAttackIndex].position;
            m_currentTargetPosition = m_nextPointPosition;

            if (IsAtTarget(m_nextPointPosition))
                TransitionToPhase(AttackPhase.Attacking);
            else
                TransitionToPhase(AttackPhase.MovingToPoint);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Di chuyển và xoay cùng lúc.
    /// </summary>
    /// <param name="targetPos">Điểm đích cần di chuyển tới.</param>
    /// <param name="moveSpeed">Tốc độ di chuyển (m/s).</param>
    /// <param name="rotSpeed">Tốc độ xoay (deg/s).</param>
    /// <param name="lookAtOverride">Điểm override để xoay nhìn vào thay vì targetPos.</param>
    private void MoveAndRotate(Vector3 targetPos, float moveSpeed, float rotSpeed, Vector3? lookAtOverride = null)
    {
        if (botContext.botNetwork.IsDead) return;

        Vector3 botPos = m_botTransform.position;
        Vector3 dir = lookAtOverride.HasValue
            ? (lookAtOverride.Value - botPos)
            : (targetPos - botPos);

        if (dir.sqrMagnitude < MIN_DIRECTION_THRESHOLD) return;

        // Tối ưu: Tránh Normalize khi không cần thiết cho LookRotation
        Quaternion targetRot = Quaternion.LookRotation(dir);
        m_botTransform.rotation = Quaternion.RotateTowards(m_botTransform.rotation, targetRot, rotSpeed * Time.deltaTime);

        m_botTransform.position = Vector3.MoveTowards(botPos, targetPos, moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Xoay mượt mà không di chuyển.
    /// </summary>
    /// <param name="targetPos">Điểm để xoay nhìn vào.</param>
    /// <param name="rotSpeed">Tốc độ xoay (deg/s).</param>
    private void RotateOnly(Vector3 targetPos, float rotSpeed)
    {
        if (botContext.botNetwork.IsDead) return;

        Vector3 dir = targetPos - m_botTransform.position;
        if (dir.sqrMagnitude < MIN_DIRECTION_THRESHOLD) return;

        // Tối ưu: LookRotation không cần direction normalized
        Quaternion targetRot = Quaternion.LookRotation(dir);
        m_botTransform.rotation = Quaternion.RotateTowards(m_botTransform.rotation, targetRot, rotSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Kiểm tra đã ở attack point chưa.
    /// </summary>
    private bool IsAtTarget(Vector3 target) =>
        (m_botTransform.position - target).sqrMagnitude <= m_sqrDistanceEnd;

    /// <summary>
    /// Kiểm tra đã trong phạm vi dash chưa.
    /// </summary>
    private bool IsWithinDashRange(Vector3 target) =>
        (m_botTransform.position - target).sqrMagnitude <= m_sqrDashAccelerationDistance;

    /// <summary>
    /// Chuyển sang phase mới và reset timer + animation.
    /// </summary>
    private void TransitionToPhase(AttackPhase newPhase)
    {
        if (botContext.botNetwork.IsDead) return;

        m_currentPhase = newPhase;
        m_phaseTimer = 0f;

        switch (newPhase)
        {
            case AttackPhase.MovingToPoint: botContext.SetAnimation(AnimationParameters.Run); break;
            case AttackPhase.ApproachingPoint: botContext.SetAnimation(AnimationParameters.Dash); break;
            case AttackPhase.Attacking: botContext.SetAnimation(AnimationParameters.AttackGun); break;
        }
    }

    #endregion
}
