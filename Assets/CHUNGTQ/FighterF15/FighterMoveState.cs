using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using static FighterStateMachine;
using Color = UnityEngine.Color;

public class FighterMoveState : BaseState<FighterState>
{
    [SerializeField] BotConfigSO fighterConfig;
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] F15TrackingMovement F15TrackingMovement;
    [SerializeField]
    private WayPoint path;
    [SerializeField] private float endTiltAngle = 30f; // Góc xoay tối đa khi phanh
    [SerializeField] private float startTiltDistance = 5f; // Khoảng cách bắt đầu nâng góc
    private float moveSpeed;
    private bool isMovetoAttack;
    private Transform myTrans;
    private Transform target;
    public AircraftMovementSettings settings;
    private float distanceTravelled = 0f;   // Khoảng cách đã di chuyển
    private BezierCurveData curveData;      // Dữ liệu đường cong
    private AircraftRotationHandler rotationHandler;  // Xử lý xoay
    private float segment;
    [SerializeField] GameObject[] effectSmoke;
    public override void EnterState()
    {
        Init();
    }
    private void Init()
    {

        F15TrackingMovement.enabled = false;
        myTrans = transform;
        path = botNetwork.Path;
        moveSpeed = fighterConfig.moveSpeed;
        //stopDistance = 1f;
        isMovetoAttack = false;
        // Khởi tạo các thành phần
        curveData = new BezierCurveData();
        curveData.Initialize(path.WayPoints);
        segment = curveData.debugSegmentsPerCurve;
        // Kiểm tra tính hợp lệ của đường đi
        if (!curveData.IsValid())
        {
            Debug.LogError("Số điểm điều khiển không hợp lệ. Phải có dạng 3n+1 (ví dụ: 4, 7, 10).");
            enabled = false;
            return;
        }
        rotationHandler = new AircraftRotationHandler(settings);
        //lookAtTarget = LocalPlayer.Instance.GetTranformPlayer();
    }
    public override void UpdateState()
    {
        if (path != null && !isMovetoAttack)
        {
            if (!curveData.IsValid()) return;
            HandleMovement();
            float distance = Vector3.Distance(transform.position, path.WayPoints[3].position); // bay tới vị trí
            if (distance < 0.13f)
            {
                isMovetoAttack = true;
                F15TrackingMovement.enabled = true;
            }
        }
    }

    private void HandleMovement()
    {
        UpdateMovementProgress();

        if (distanceTravelled >= curveData.TotalLength)
        {
            distanceTravelled = curveData.TotalLength;
            transform.position = curveData.GetPositionAlongCurve(distanceTravelled);
            return;
        }

        if (curveData == null || !curveData.IsValid())
        {
            Debug.LogError("BezierCurveData không hợp lệ hoặc chưa khởi tạo.");
            return;
        }

        // Lấy vị trí hiện tại
        Vector3 currentPosition = curveData.GetPositionAlongCurve(distanceTravelled);

        // Tính toán hướng tương lai
        Vector3 futureDirection = CalculateFutureDirectionWithFallback(currentPosition);
        if (futureDirection == Vector3.zero)
        {
            Debug.LogWarning("Future direction trả về Vector3.zero, thay thế bằng Vector3.forward.");
            futureDirection = Vector3.forward;
        }

        // Điều chỉnh góc xoay khi gần tới điểm cuối
        float remainingDistance = curveData.TotalLength - distanceTravelled;
        float tiltAngle = 0f;
        if (remainingDistance < startTiltDistance)
        {
            SetActiveSmoke(true);
            float tiltRatio = Mathf.Clamp01(remainingDistance / startTiltDistance);
            tiltAngle = Mathf.Lerp(endTiltAngle, 0, tiltRatio); // Góc xoay giảm dần khi tới điểm cuối
        }

        // Tạo góc nghiêng
        Quaternion tiltRotation = Quaternion.Euler(tiltAngle, 0, 0);

        // Xoay hướng tương lai bằng tiltRotation
        Vector3 tiltedDirection = tiltRotation * futureDirection;

        // Cập nhật vị trí và xoay
        rotationHandler.UpdateRotation(myTrans != null ? myTrans : transform, currentPosition, tiltedDirection);
        transform.position = currentPosition;
        //Debug.LogError($"Tilt Angle: {tiltAngle}, Tilted Direction: {tiltedDirection}, Remaining Distance: {remainingDistance}");

    }


    // Cập nhật tiến độ di chuyển
    private void UpdateMovementProgress()
    {
        float currentSpeed = CalculateCurrentSpeed();
        distanceTravelled += currentSpeed * Time.deltaTime;

        // Giới hạn distanceTravelled trong TotalLength
        distanceTravelled = Mathf.Clamp(distanceTravelled, 0, curveData.TotalLength);
    }

    // Tính toán tốc độ hiện tại (có giảm tốc ở cuối)
    private float CalculateCurrentSpeed()
    {
            float remainingDistance = curveData.TotalLength - distanceTravelled;
            if (remainingDistance < moveSpeed)
            {
                float slowdownRatio = Mathf.Clamp01(remainingDistance / settings.endSlowdownDistance);
                return moveSpeed * Mathf.Lerp(settings.minSpeedPercent, 1f, slowdownRatio);
            }
        return moveSpeed;
    }
    
    private Vector3 CalculateFutureDirectionWithFallback(Vector3 currentPosition)
    {
        float offsetDistance = 0.1f; // Khoảng cách nhỏ để tìm vị trí trong tương lai
        float futureDistance = distanceTravelled + offsetDistance;

        // Xử lý trường hợp vượt quá chiều dài đường cong
        if (futureDistance >= curveData.TotalLength)
        {
            futureDistance = curveData.TotalLength;
        }

        // Lấy vị trí tiếp theo trên đường cong
        Vector3 futurePosition = curveData.GetPositionAlongCurve(futureDistance);
        Vector3 futureDirection = (futurePosition - currentPosition).normalized;

        if (futureDirection == Vector3.zero)
        {
            Debug.LogWarning("Future direction is zero. Using fallback direction.");
            return Vector3.forward; // Sử dụng một hướng dự phòng
        }

        return futureDirection;
    }
    public void SetActiveSmoke(bool isActive)
    {
        foreach (GameObject obj in effectSmoke)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(isActive);
            }
        }
    }
    public override void ExitState()
    {
        isMovetoAttack = false;
        SetActiveSmoke(false);
    }
    public override FighterState GetNextState()
    {
        if(botNetwork.IsDead)
        {
            return FighterState.Dead;
        }
        else
        {
            if (isMovetoAttack)
            {
                return FighterState.Attack;
            }
            return StateKey;
        }
        

    }
    private void OnDrawGizmos()
    {
        if (path.WayPoints == null || path.WayPoints.Count < 4)
        {
            Debug.LogWarning("Không đủ điểm điều khiển để vẽ đường cong.");
            return;
        }
    
        Vector3 startPoint = path.WayPoints[0].position;
        Vector3 controlPoint1 = path.WayPoints[1].position;
        Vector3 controlPoint2 = path.WayPoints[path.WayPoints.Count - 2].position;
        Vector3 endPoint = path.WayPoints[path.WayPoints.Count - 1].position;
    
        Gizmos.color = Color.green;
    
        Vector3 previousPoint = startPoint;
        for (int j = 1; j <= segment; j++)
        {
            float t = j / (float)segment;
            Vector3 currentPoint = curveData.CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPoint, 0.1f);
        Gizmos.DrawSphere(endPoint, 0.1f);
    
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(controlPoint1, 0.1f);
        Gizmos.DrawSphere(controlPoint2, 0.1f);
    }
}
