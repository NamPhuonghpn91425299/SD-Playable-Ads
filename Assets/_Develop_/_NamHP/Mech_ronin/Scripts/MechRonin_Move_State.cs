using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtilities;
using static GameConstants;
using static MechRoninStateController;

/// <summary>
/// State xử lý di chuyển qua waypoints
/// </summary>
public class MechRonin_Move_State : StateBase
{
    [Header("Visual References")]
    public GameObject[] m_smokeTrails;
    public GameObject[] m_jetObjects;
    [Header("Movement Settings")]
    [SerializeField] private float m_moveSpeed = 15f;
    [SerializeField] private float m_rotationSpeed = 3f;
    [SerializeField] private float m_distanceEnd = 0.5f;
    [SerializeField] private float m_rollingTriggerDistance = 12f;

    private int m_currentWaypointIndex = 0;
    private bool m_hasTriggeredRolling = false;
    private bool m_isMoving = true;
    private float m_sqrDistanceEnd;
    private float m_sqrRollingTriggerDistance;
    private Coroutine m_movementCoroutine;

    private void Awake()
    {
        m_sqrDistanceEnd = m_distanceEnd * m_distanceEnd;
        m_sqrRollingTriggerDistance = m_rollingTriggerDistance * m_rollingTriggerDistance;
    }

    public override void EnterState()
    {
        m_currentWaypointIndex = 0;
        m_hasTriggeredRolling = false;
        m_isMoving = true;
        SetTrailEffects(true);

        botContext.SetAnimation(AnimationParameters.Fly);

        if (m_movementCoroutine != null)
            StopCoroutine(m_movementCoroutine);
        m_movementCoroutine = StartCoroutine(MovementCoroutine());
    }

    public override void UpdateState()
    {
        // Logic chạy trong coroutine
    }

    public override void ExitState()
    {
        m_isMoving = false;
        SetTrailEffects(false);

        if (m_movementCoroutine != null)
        {
            StopCoroutine(m_movementCoroutine);
            m_movementCoroutine = null;
        }
    }

    private IEnumerator MovementCoroutine()
    {
        var waypoints = botContext.botIdentity.Waypoints;
        Transform botTF = botContext.botNetwork.TF;

        // prevSqrDistXZ = +inf để biết frame khởi tạo
        float prevSqrDistXZ = float.PositiveInfinity;

        while (m_isMoving && m_currentWaypointIndex < waypoints.Count)
        {
            Transform targetWaypoint = waypoints[m_currentWaypointIndex];
            if (targetWaypoint == null)
            {
                // bảo vệ null waypoint
                m_currentWaypointIndex++;
                yield return null;
                continue;
            }

            Vector3 targetPosition = targetWaypoint.position;
            Vector3 botPos3 = botTF.position;

            // --- tính XZ (bỏ trục Y) để tránh ảnh hưởng độ cao khi bay ---
            Vector2 botXZ = new Vector2(botPos3.x, botPos3.z);
            Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);
            float currSqrDistXZ = (botXZ - targetXZ).sqrMagnitude;

            // Di chuyển + xoay (dùng utility)
            bool hasReached = WaypointMovementUtility.MoveTowards(botTF, targetPosition, m_moveSpeed);
            WaypointMovementUtility.RotateTowards(botTF, targetPosition, m_rotationSpeed);

            // --- Trigger rolling: phát hiện khi vừa bước vào vùng (prev > threshold && curr <= threshold) ---
            if (!m_hasTriggeredRolling && m_currentWaypointIndex < waypoints.Count - 1)
            {
                // Trường hợp đã ở trong vùng ngay từ đầu (prev = +inf), hoặc vừa mới bước vào
                bool enteringTrigger = (prevSqrDistXZ > m_sqrRollingTriggerDistance && currSqrDistXZ <= m_sqrRollingTriggerDistance)
                                     || (prevSqrDistXZ == float.PositiveInfinity && currSqrDistXZ <= m_sqrRollingTriggerDistance);

                if (enteringTrigger)
                {
                    float distXZ = Mathf.Sqrt(currSqrDistXZ);
                    float dist3D = Vector3.Distance(botPos3, targetPosition);
                    //Debug.Log($"[MoveState] ROLLING TRIGGERED (waypoint {m_currentWaypointIndex}) distXZ={distXZ:F2} dist3D={dist3D:F2} rollingTrigger={m_rollingTriggerDistance}");
                    botContext.SetAnimation(AnimationParameters.Rolling);
                    m_hasTriggeredRolling = true;
                }
            }

            // --- Check reached (so sánh XZ với m_sqrDistanceEnd thì nhất quán) ---
            if (hasReached || currSqrDistXZ <= m_sqrDistanceEnd)
            {
                bool isLast = m_currentWaypointIndex >= waypoints.Count - 1;
                m_currentWaypointIndex++;

                if (isLast)
                {
                    m_isMoving = false;
                    botContext.stateController.ChangeState(EnemyState.Falling);
                    yield break;
                }
                else
                {
                    // Chỉ reset trigger khi thực sự chuyển sang waypoint mới
                    m_hasTriggeredRolling = false;
                    prevSqrDistXZ = float.PositiveInfinity; // reset prev cho waypoint kế
                }
            }
            else
            {
                // cập nhật prev để phát hiện "vừa vào vùng" ở frame tiếp theo
                prevSqrDistXZ = currSqrDistXZ;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Kích hoạt/tắt smoke trails và jet effects
    /// </summary>
    public void SetTrailEffects(bool active)
    {
        if (m_smokeTrails != null)
        {
            foreach (var smoke in m_smokeTrails)
                if (smoke) smoke.SetActive(active);
        }

        if (m_jetObjects != null)
        {
            foreach (var jet in m_jetObjects)
                if (jet) jet.SetActive(active);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        var waypoints = botContext?.botIdentity?.Waypoints;
        if (waypoints == null || m_currentWaypointIndex >= waypoints.Count) return;

        Transform currentWaypoint = waypoints[m_currentWaypointIndex];
        if (currentWaypoint == null) return;

        Vector3 botPosition = botContext.botNetwork.TF.position;
        Vector3 targetPosition = currentWaypoint.position;

        // Vẽ trigger sphere nhưng nâng lên cùng cao độ bot để debug trực quan
        Vector3 triggerCenter = new Vector3(targetPosition.x, botPosition.y, targetPosition.z);

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(triggerCenter, m_rollingTriggerDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(triggerCenter, m_rollingTriggerDistance);

        // Vẽ waypoint, đường đi, hướng bot như trước
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPosition, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(botPosition, targetPosition);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(botPosition, botContext.botNetwork.TF.forward * 2f);

        Gizmos.color = Color.cyan;
        Vector3 targetDirection = (targetPosition - botPosition).normalized * 2f;
        Gizmos.DrawRay(botPosition, targetDirection);

#if UNITY_EDITOR
        float dist3D = Vector3.Distance(botPosition, targetPosition);
        Vector2 botXZ = new Vector2(botPosition.x, botPosition.z);
        Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);
        float distXZ = Vector2.Distance(botXZ, targetXZ);
        UnityEditor.Handles.Label(botPosition + Vector3.up * 2f,
            $"Dist3D: {dist3D:F2}\nDistXZ: {distXZ:F2}\nRollingTrig: {m_rollingTriggerDistance}\nHasTriggered: {m_hasTriggeredRolling}");
#endif

        // Next waypoint preview
        if (m_currentWaypointIndex < waypoints.Count - 1)
        {
            Transform nextWaypoint = waypoints[m_currentWaypointIndex + 1];
            if (nextWaypoint != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(nextWaypoint.position, 0.3f);
                Gizmos.DrawLine(targetPosition, nextWaypoint.position);
            }
        }
    }

}
