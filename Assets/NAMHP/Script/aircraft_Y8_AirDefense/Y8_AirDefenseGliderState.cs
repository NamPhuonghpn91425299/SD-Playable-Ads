using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FighterStateMachine;
using static Y8_AirDefenseStateMachine;
public class Y8_AirDefenseGliderState : BaseState<Y8_AirDefense>
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] protected WayPoint wayPoint;
    [SerializeField] F15TrackingMovement f15TrackingMovement;
    public AircraftMovementSettings _settings;
    private QuadraticBezierPath path;
    private float distanceTraveled = 0f;
    private bool isMoving = true;
    [SerializeField]
    private int currentWaypointIndex ;
    [SerializeField]
    private int targetWaypointIndex ;
    private Transform player;
    private bool movingForward = true;

    public override void EnterState()
    {
        wayPoint = botNetwork.Path;
        player = LocalPlayer.Instance.GetTranformPlayer();
        f15TrackingMovement.enabled = false;
        SelectNextWaypoint();
    }
    
    public override void UpdateState()
    {
        MoveAlongPath();
    }

   private void MoveAlongPath()
    {
        if (path == null) return;

        // Cập nhật quãng đường di chuyển
        distanceTraveled += _settings.movementSpeed * Time.deltaTime;
        // Nếu đã đến cuối đường dẫn
        if (distanceTraveled >= path.TotalLength)
        {
            isMoving = false; // Dừng di chuyển để chọn waypoint mới
            return;
        }

        // Di chuyển theo đường cong
        Vector3 currentPosition = path.GetPositionAlongPath(distanceTraveled);
        transform.position = currentPosition;

        // Điều chỉnh hướng
        AdjustRotation();
    }

    private void AdjustRotation()
    {
        // Lấy vị trí hiện tại và tiếp theo
        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = path.GetPositionAlongPath(Mathf.Min(distanceTraveled + 0.1f, path.TotalLength));
        Vector3 directionToNext = (nextPosition - currentPosition).normalized;

        // Tính góc nghiêng và căn chỉnh hướng
        if (directionToNext != Vector3.zero)
        {
            float bankAngle = CalculateBankAngle(directionToNext);
            float pitchAngle = movingForward ? -_settings.maxPitchAngle : _settings.maxPitchAngle;

            Quaternion targetRotation = Quaternion.Euler(
                pitchAngle,
                transform.rotation.eulerAngles.y,
                bankAngle
            );

            // Làm mượt chuyển động xoay
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _settings.smoothRotationSpeed);
        }
    }

    private float CalculateBankAngle(Vector3 movementDirection)
    {
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(movementDirection, Vector3.up);
        float bankAngle = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);
        return Mathf.Clamp(bankAngle, -_settings.maxBankAngle, _settings.maxBankAngle);
    }
# if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawGizmos(Color.blue, Color.yellow);
        }
    }
#endif    
    private void SelectNextWaypoint()
    {
        targetWaypointIndex = currentWaypointIndex;
        // Đảm bảo chỉ số random không vượt quá phạm vi danh sách
        RandomByIndex(ref currentWaypointIndex, wayPoint.AttackWayPoints.Count, targetWaypointIndex);
        if (targetWaypointIndex < 0 || targetWaypointIndex >= wayPoint.AttackWayPoints.Count ||
            currentWaypointIndex < 0 || currentWaypointIndex >= wayPoint.AttackWayPoints.Count)
        {
            Debug.LogError("Chỉ số waypoint không hợp lệ. Kiểm tra RandomByIndex.");
            return;
        }
        // Tạo đường dẫn mới
        List<Transform> currentPath = new List<Transform>
        {
            wayPoint.AttackWayPoints[targetWaypointIndex],
            wayPoint.AttackWayPoints[currentWaypointIndex]
        };

        path = new QuadraticBezierPath(currentPath, _settings.controlPointOffset);
        distanceTraveled = 0f;
        isMoving = true;
    }

    private void RandomByIndex(ref int currentIndex, int listCount, int excludeIndex)
    {
        if (listCount <= 1)
        {
            Debug.LogWarning("Danh sách có ít hơn 2 phần tử. Không thể random.");
            return;
        }

        int newIndex = excludeIndex;
        while (newIndex == excludeIndex)
        {
            newIndex = Random.Range(0, listCount);
        }

        currentIndex = newIndex;
    }
    public override Y8_AirDefense GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return Y8_AirDefense.Dead;
        }
        else if (!isMoving)
        {
            return Y8_AirDefense.Idle;
        }
        return StateKey;
    }

    public override void ExitState()
    {
        f15TrackingMovement.enabled = true;
        isMoving = true;
    }
    
}
