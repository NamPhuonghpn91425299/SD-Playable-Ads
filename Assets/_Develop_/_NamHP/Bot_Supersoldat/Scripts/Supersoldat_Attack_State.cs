using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtilities;
using static GameConstants;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine.Serialization;

/// <summary>
/// Trạng thái tấn công của Supersoldat:
/// - Di chuyển theo các attackPoints trong AssignedPath (loop)
/// - Luôn xoay nhìn về người chơi khi di chuyển
/// - Cập nhật hướng animation theo vector hướng path hiện tại
/// </summary>
public class Supersoldat_Attack_State : StateBase
{
    [Header("Attack Settings")]
    [Tooltip("Số lượng viên bắn mỗi lần tấn công (chưa dùng ở state này, giữ để tương thích).")]
    [SerializeField] private int _countShoot;

    [Tooltip("Thời gian cho một lần nhả đạn (chưa dùng ở state này, giữ để tương thích).")]
    [SerializeField] private float _timeOneShoot;

    [Tooltip("Hiệu ứng lửa đầu nòng khi tấn công.")]
    [SerializeField] private GameObject _muzzle;

    [Header("References")]
    [Tooltip("Thân nhân vật (nếu cần tách xoay thân với chân).")]
    [SerializeField] private Transform _body;

    [Tooltip("State di chuyển mặc định để lấy AssignedPath.")]
    [SerializeField] private DefaultMove _defaultMove;

    [Header("Movement")]
    [Tooltip("Tốc độ di chuyển giữa các attackPoints.")]
    [SerializeField] private float m_moveSpeed = 2.0f;

    [Tooltip("Tốc độ xoay mượt khi hướng về người chơi.")]
    [SerializeField] private float m_rotationSpeed = 10.0f;

    [Tooltip("Chỉ số attack point hiện tại (loop).")]
    [SerializeField] private int m_currentPointIndex = 0;

    [Tooltip("Ngưỡng khoảng cách để coi như đã tới điểm mục tiêu.")]
    [SerializeField] private float m_distanceEnd = 0.25f;
    
    [SerializeField] private float m_timeOneShoot = 1f;
    // Theo dõi điểm cuối cùng đã cập nhật anim và hướng anim cuối
    private int  m_lastPointIndex     = -1;
    private int  m_lastDirectionIndex = -1;
    private bool m_isfirst;
    int          directionIndex;
    /// <inheritdoc />
    public override void EnterState()
    {

        StartCoroutine(Attacking());
        // Đảm bảo có AssignedPath hợp lệ
        if (_defaultMove == null || _defaultMove.AssignedPath == null || _defaultMove.AssignedPath.attackPoints == null || _defaultMove.AssignedPath.attackPoints.Count == 0)
        {
            Debug.LogWarning($"{nameof(Supersoldat_Attack_State)}: Missing AssignedPath or attackPoints. State will idle.");
            return;
        }
        if (!m_isfirst)
        {
            // Reset để đảm bảo animation cập nhật lần đầu
            m_lastPointIndex     = -1;
            m_lastDirectionIndex = -1;
            directionIndex = 0;
            // Vào trạng thái tấn công với anim type mặc định (0)
        }
        UpdateAnimationDirection(directionIndex);
        // Clamp chỉ số bắt đầu
        m_currentPointIndex = Mathf.Clamp(m_currentPointIndex, 0, _defaultMove.AssignedPath.attackPoints.Count - 1);
    }

    /// <inheritdoc />
    public override void UpdateState()
    {
        // Không có path => không di chuyển
        if (_defaultMove == null || _defaultMove.AssignedPath == null || _defaultMove.AssignedPath.attackPoints == null || _defaultMove.AssignedPath.attackPoints.Count == 0 || botContext.botNetwork.IsDead)
            return;

        MoveToAttack();
    }

    private IEnumerator Attacking()
    {
            // Bật muzzle khi vào trạng thái tấn công
            if (_muzzle != null)
                _muzzle.SetActive(true);
            while (true)
            {
                yield return HelperCoroutine.GetWait(m_timeOneShoot);
                if(GameController.Instance.CurrentGameState == GameState.InGame)
                    EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state:"OnlyDamage"));
                //Debug.Log("Damage On Player: " + botContext.botNetwork.Damage);
            }

    }
    /// <summary>
    /// Di chuyển tới attack point hiện tại, xoay nhìn về người chơi,
    /// và cập nhật anim theo hướng path. Loop qua danh sách attackPoints.
    /// </summary>
    private void MoveToAttack()
    {
        var points = _defaultMove.AssignedPath.attackPoints;
        if (m_currentPointIndex < 0 || m_currentPointIndex >= points.Count)
            m_currentPointIndex = 0;

        Vector3 targetPos = points[m_currentPointIndex].position;
        Vector3 playerPos = PlayerInstant.Instance != null ? PlayerInstant.Instance.TF.position : targetPos;

        bool hasReached = WaypointMovementUtility.MoveTowards(TF, targetPos, m_moveSpeed);
        WaypointMovementUtility.RotateTowards(TF, playerPos, m_rotationSpeed);

        // Kiểm tra nếu đã đến điểm mục tiêu
        if (hasReached || WaypointMovementUtility.HasReached(TF, targetPos, m_distanceEnd))
        {
            // Cập nhật anim khi thực sự tới điểm mới
            botContext.stateController.ChangeState(EnemyState.Idle);
            if (m_currentPointIndex != m_lastPointIndex)
            {
                m_lastPointIndex = m_currentPointIndex;

                // Cập nhật hướng anim theo hướng segment hiện tại
                UpdateMovementAnimation();

                // Chuyển sang điểm tiếp theo (loop)
                m_currentPointIndex++;
                if (m_currentPointIndex >= points.Count)
                    m_currentPointIndex = 0;
            }
        }
    }

    /// <summary>
    /// Tính hướng từ điểm hiện tại sang điểm tiếp theo và gửi cho bộ xác định hướng anim.
    /// </summary>
    private void UpdateMovementAnimation()
    {
        var points = _defaultMove.AssignedPath.attackPoints;
        if (points == null || points.Count == 0)
            return;

        int nextPointIndex = m_currentPointIndex + 1;
        if (nextPointIndex >= points.Count)
            nextPointIndex = 0;

        Vector3 currentPointPos = points[m_currentPointIndex].position;
        Vector3 nextPointPos = points[nextPointIndex].position;

        Vector3 pathDirection = (nextPointPos - currentPointPos).normalized;
        if (pathDirection.sqrMagnitude < 0.0001f)
            return;

        DetermineMovementAnimationDirection(pathDirection);
    }

    /// <summary>
    /// Xác định hướng anim (0:Forward, 1:Right, 2:Backward, 3:Left) dựa trên vector hướng path.
    /// </summary>
    /// <param name="pathDirection">Vector hướng của đoạn path hiện tại.</param>
    private void DetermineMovementAnimationDirection(Vector3 pathDirection)
    {
        // Dùng -pathDirection nếu hệ toạ độ setup ngược (giữ nguyên logic hiện tại)
        Vector3 adjustedDirection = -pathDirection;

        float angle = Mathf.Atan2(adjustedDirection.x, adjustedDirection.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;


        // 4 hướng theo góc
        if (angle >= 315f || angle < 45f)        directionIndex = 0; // Forward
        else if (angle >= 45f && angle < 135f)   directionIndex = 1; // Right
        else if (angle >= 135f && angle < 225f)  directionIndex = 2; // Backward
        else                                     directionIndex = 3; // Left

        if (directionIndex != m_lastDirectionIndex)
        {
#if UNITY_EDITOR
            // var dirName = directionIndex == 0 ? "Forward" :
            //     directionIndex == 1              ? "Right" :
            //     directionIndex == 2              ? "Backward" : "Left";
            //Debug.Log($"[Supersoldat_Attack_State] Anim dir change: {m_lastDirectionIndex} -> {directionIndex} ({dirName}), angle: {angle:0.0}");
#endif
            //UpdateAnimationDirection(directionIndex);
            m_lastDirectionIndex = directionIndex;
        }
    }

    /// <summary>
    /// Gửi trigger anim tấn công với biến kiểu anim (AnimType) ứng theo hướng.
    /// </summary>
    /// <param name="directionIndex">0:Forward, 1:Right, 2:Backward, 3:Left</param>
    private void UpdateAnimationDirection(int directionIndex)
    {
        // Có thể thay HashAttack bằng hash cụ thể cho từng hướng nếu animator tách riêng
        botContext.ChangeAnimAndType(HashAttack, directionIndex);
    }

    /// <inheritdoc />
    public override void ExitState()
    {
        m_isfirst = true;
        if (_muzzle != null)
            _muzzle.SetActive(false);
        StopAllCoroutines();
    }
}
