using System.Collections.Generic;
using UnityEngine;
using static BotTankStateMachine;

public class BotTankMoveToAttackState : BaseState<TankState>
{
    [SerializeField] private TankBaseMovement tankMovement;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected WayPoint wayPoint;
    [SerializeField] private Transform currentPoint;
    [SerializeField] private int currentIdx = -1;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private AudioSource audioSource;
    //[SerializeField] private float moveSpeedPoint = 5f; // Tốc độ di chuyển
    //private Vector3 currentVelocity; // Dùng cho smoothDamp
    //[SerializeField] private float smoothRotateVelocity;

    public override void EnterState()
    {
        //Init();
        audioSource.Play();
        if (botNetwork == null || botNetwork.Path == null) return;
        isMoving = false;
        wayPoint = botNetwork.Path;
        //RandomNextPoint();
        RandomByIndex();
    }

    private void Init()
    {

    }
    private void MoveToAttack()
    {
        if (currentPoint == null) return;

        Vector3 targetPosition = currentPoint.position;
        Vector3 currentPosition = tankMovement.myTrans.position;

        // Tính khoảng cách đến mục tiêu
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

        if (distanceToTarget > 0.1f)
        {
            //// Xoay tank mượt mà hướng về điểm đích
            //Vector3 direction = (targetPosition - currentPosition).normalized;
            //float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            //// Sử dụng SmoothDampAngle để xoay mượt
            //float smoothedRotation = Mathf.SmoothDampAngle(
            //    tankMovement.myTrans.eulerAngles.y,
            //    targetAngle,
            //    ref smoothRotateVelocity,
            //    0.1f
            //);

            //tankMovement.myTrans.rotation = Quaternion.Euler(0, smoothedRotation, 0);

            //// Di chuyển mượt với SmoothDamp
            //Vector3 smoothPosition = Vector3.SmoothDamp(
            //    currentPosition,
            //    targetPosition,
            //    ref currentVelocity,
            //    0.5f,
            //    moveSpeedPoint
            //);

            //// Cập nhật vị trí
            //tankMovement.myTrans.position = smoothPosition;

            tankMovement.SetBotTankMove(currentPoint);
        }
        else
        {
            isMoving = true;
            //RandomNextPoint(); // Chọn điểm tiếp theo khi đến đích
            //RandomByIndex();
        }
    }

    private void RandomNextPoint()
    {
        if (wayPoint == null || wayPoint.AttackWayPoints == null || wayPoint.AttackWayPoints.Count <= 1)
        {
            Debug.LogWarning("Danh sách AttackWayPoints trống hoặc chỉ có một điểm. Không thể chọn điểm ngẫu nhiên.");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(wayPoint.AttackWayPoints);

        // Nếu currentPoint có trong danh sách, xóa đi để tránh lặp lại
        if (currentPoint != null && availablePoints.Contains(currentPoint))
        {
            availablePoints.Remove(currentPoint);
        }

        // Chọn một điểm ngẫu nhiên trong các điểm còn lại
        int randomIndex = Random.Range(0, availablePoints.Count);
        currentPoint = availablePoints[randomIndex];

        Debug.Log($"Moving to {currentPoint.name}");
        Debug.DrawLine(transform.position, currentPoint.position, Color.red, 1f);
    }

    private void RandomByIndex()
    {
        if (wayPoint == null || wayPoint.AttackWayPoints == null || wayPoint.AttackWayPoints.Count <= 1)
        {
            Debug.LogWarning("Danh sách AttackWayPoints trống hoặc chỉ có một điểm. Không thể chọn điểm ngẫu nhiên.");
            return;
        }
        // int idx;
        //do
        //{
        //    idx = Random.Range(0, wayPoint.AttackWayPoints.Count);
        //}
        //while (currentIdx == idx);

        //currentPoint = wayPoint.AttackWayPoints[idx];

        int count=100;
        while(count-- > 0)
        {
            int idx = Random.Range(0, wayPoint.AttackWayPoints.Count);
            if (currentIdx == idx) continue;
 
            currentIdx = idx;
            currentPoint = wayPoint.AttackWayPoints[idx];
            break;
        }


        //Debug.LogError($"Moving to {currentPoint.name}");
        Debug.DrawLine(transform.position, currentPoint.position, Color.red, 1f);
    }

    public override void UpdateState()
    {
        MoveToAttack();
    }

    public override void ExitState()
    {
        audioSource.Stop();
        // Có thể thêm logic cleanup nếu cần
    }

    public override TankState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return TankState.Dead;
        }
        else if (isMoving)
        {
            return TankState.Acttack;
        }
        return StateKey;
    }
#if UNITY_EDITOR
    // Thêm visualization để debug trong Editor
    private void OnDrawGizmos()
    {
        if (currentPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentPoint.position, 0.5f);

            if (tankMovement != null && tankMovement.myTrans != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tankMovement.myTrans.position, currentPoint.position);
            }
        }
    }
#endif
}