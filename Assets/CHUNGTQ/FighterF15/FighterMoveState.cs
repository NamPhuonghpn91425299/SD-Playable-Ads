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
    private float moveSpeed;
    private bool isMovetoAttack;
    private Transform myTrans;
    private Transform target;
    [SerializeField] private AircraftMovementSettings settings;
    private float distanceTravelled = 0f;   // Khoảng cách đã di chuyển
    private BezierCurveData curveData;      // Dữ liệu đường cong
    private AircraftRotationHandler rotationHandler;  // Xử lý xoay
    private float segment;
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
            if (distance < 0.1f)
            {
                isMovetoAttack = true;
                F15TrackingMovement.enabled = true;
            }
        }
    }
    // Xử lý di chuyển trong mỗi frame
    private void HandleMovement()
    {
        UpdateMovementProgress();
        if (HasReachedEnd()) return;

        // Lấy vị trí hiện tại và tính hướng di chuyển
        Vector3 currentPosition = curveData.GetPositionAlongCurve(distanceTravelled);
        Vector3 futureDirection = CalculateFutureDirection(currentPosition);
        
        // Cập nhật xoay và vị trí
        rotationHandler.UpdateRotation(myTrans, currentPosition, futureDirection);
        transform.position = currentPosition;
    }

    // Cập nhật tiến độ di chuyển
    private void UpdateMovementProgress()
    {
        float currentSpeed = CalculateCurrentSpeed();
        distanceTravelled += currentSpeed * Time.deltaTime;
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

    // Tính toán hướng di chuyển dựa vào các điểm phía trước
    private Vector3 CalculateFutureDirection(Vector3 currentPosition)
    {
        Vector3 futureDirection = Vector3.zero;
        for (int i = 1; i <= settings.predictionSteps; i++)
        {
            float futureDistance = distanceTravelled + 
                (settings.lookAheadDistance * i / settings.predictionSteps);
            if (futureDistance > curveData.TotalLength) continue;
            
            Vector3 futurePos = curveData.GetPositionAlongCurve(
                futureDistance % curveData.TotalLength);
            futureDirection += (futurePos - currentPosition).normalized;
        }
        return futureDirection / settings.predictionSteps;
    }

    // Kiểm tra và xử lý khi đến điểm cuối
    private bool HasReachedEnd()
    {
        if (distanceTravelled >= curveData.TotalLength)
        {
                distanceTravelled = 0f;
                return false;
        }
        return false;
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
    public override void ExitState()
    {

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
}
