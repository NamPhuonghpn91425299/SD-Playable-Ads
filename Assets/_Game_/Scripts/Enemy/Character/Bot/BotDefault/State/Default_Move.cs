using static GameConstants;
using UnityEngine;
using GameUtilities;
using UnityEngine.Serialization;

public class DefaultMove : StateBase
{
    [FormerlySerializedAs("moveSpeed")]
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của bot.")]
    [SerializeField]
    private float m_moveSpeed = 5.0f;

    [FormerlySerializedAs("rotationSpeed")]
    [Tooltip("Tốc độ xoay của bot khi đổi hướng.")]
    [SerializeField]
    private float m_rotationSpeed = 10.0f;

    [FormerlySerializedAs("assignedPath")]
    [Header("Pathing Info (Read-Only)")]
    [Tooltip("Tuyến đường mà bot này đang đi theo.")]

    public PointGroup AssignedPath; // Để debug trong Inspector

    [FormerlySerializedAs("botIdentity")]
    public BotIdentity BotIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    [FormerlySerializedAs("currentPointIndex")]
    [SerializeField] private int m_currentPointIndex = 0; // Điểm tiếp theo cần đến
    private float m_distanceEnd;
    private void OnEnable()
    {
        //assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        m_currentPointIndex = 0;
    }

    public override void EnterState()
    {
        //assignedPath = botIdentity.AssignedPath;
        Invoke(nameof(Init), .1f);
    }

    void Init()
    {
        m_distanceEnd = Random.Range(0.1f, 1f);
        if (botContext.botNetwork.IsDeadExplosion || botContext.botNetwork.IsDead)
            return;
        AssignedPath = BotIdentity.AssignedPath; // Lấy đường đi từ BotIdentity

        // Smooth initial rotation to first point using utility
        if (AssignedPath != null && AssignedPath.points.Count > 0)
        {
            Vector3 targetPosition = AssignedPath.points[m_currentPointIndex].position;
            Vector3 direction = WaypointMovementUtility.GetDirection(TF.position, targetPosition);
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                TF.rotation = targetRotation; // Initial rotation without animation
            }
        }
        botContext.ChangeAnimAndType(HashMove, 0);
    }

    public override void UpdateState()
    {
        // Nếu không ở trạng thái di chuyển, hoặc không có đường đi, thì không làm gì cả.
        if (AssignedPath == null || AssignedPath.points.Count == 0)
        {
            //            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        // Get target position
        Vector3 targetPosition = AssignedPath.points[m_currentPointIndex].position;

        // Move towards target point using utility
        bool hasReached = WaypointMovementUtility.MoveTowards(TF, targetPosition, m_moveSpeed);

        // Smooth rotation towards current target using utility
        WaypointMovementUtility.RotateTowards(TF, targetPosition, m_rotationSpeed);

        // Check if reached current point
        if (!hasReached && !WaypointMovementUtility.HasReached(TF, targetPosition, m_distanceEnd))
            return;
        m_currentPointIndex++;
        if (m_currentPointIndex >= AssignedPath.points.Count)
            botContext.stateController.ChangeState(EnemyState.Attack);
        // No need for immediate LookAt here since smooth rotation handles it
    }

    public override void ExitState()
    {

    }
}
