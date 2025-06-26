using UnityEngine;
using System.Collections.Generic;
[ExecuteInEditMode]
public class MultiPointCurvedMovement : MonoBehaviour
{
    [SerializeField] private List<Transform> controlPoints = new List<Transform>();
    // [SerializeField] private AircraftMovementSettings settings;
    //
    // [Header("Look At Target")]
    // public float rotationTarget = 5f;       // Tốc độ xoay khi nhìn mục tiêu
    // public Transform lookAtTarget;          // Transform mục tiêu cần nhìn vào
    // public Vector3 lookAtPosition;          // Vị trí cần nhìn vào (nếu không có transform)
    // public bool loop = false;               // Có lặp lại đường đi không
    //
    // private float distanceTravelled = 0f;   // Khoảng cách đã di chuyển
    // private BezierCurveData curveData;      // Dữ liệu đường cong
    //private AircraftRotationHandler rotationHandler;  // Xử lý xoay
    private float segment = 100f;
    // private void Start()
    // {
    //     // Khởi tạo các thành phần
    //     curveData = new BezierCurveData();
    //     curveData.Initialize(controlPoints);
    //     segment = curveData.debugSegmentsPerCurve;
    //     // Kiểm tra tính hợp lệ của đường đi
    //     if (!curveData.IsValid())
    //     {
    //         Debug.LogError("Số điểm điều khiển không hợp lệ. Phải có dạng 3n+1 (ví dụ: 4, 7, 10).");
    //         enabled = false;
    //         return;
    //     }
    //     transform.position = transform.forward;
    //     //rotationHandler = new AircraftRotationHandler(settings);
    //     //lookAtTarget = LocalPlayer.Instance.GetTranformPlayer();
    // }
    //
    // private void Update()
    // {
    //     //if (!curveData.IsValid()) return;
    //     //HandleMovement();
    // }
    //
    // // Xử lý di chuyển trong mỗi frame
    // private void HandleMovement()
    // {
    //     UpdateMovementProgress();
    //     if (HasReachedEnd()) return;
    //
    //     // Lấy vị trí hiện tại và tính hướng di chuyển
    //     Vector3 currentPosition = curveData.GetPositionAlongCurve(distanceTravelled);
    //     Vector3 futureDirection = CalculateFutureDirection(currentPosition);
    //     
    //     // Cập nhật xoay và vị trí
    //     //rotationHandler.UpdateRotation(transform, currentPosition, futureDirection);
    //     transform.position = currentPosition;
    // }
    //
    // // Cập nhật tiến độ di chuyển
    // private void UpdateMovementProgress()
    // {
    //     float currentSpeed = CalculateCurrentSpeed();
    //     distanceTravelled += currentSpeed * Time.deltaTime;
    // }
    //
    // // Tính toán tốc độ hiện tại (có giảm tốc ở cuối)
    // private float CalculateCurrentSpeed()
    // {
    //     if (!loop)
    //     {
    //         float remainingDistance = curveData.TotalLength - distanceTravelled;
    //         if (remainingDistance < settings.endSlowdownDistance)
    //         {
    //             float slowdownRatio = Mathf.Clamp01(remainingDistance / settings.endSlowdownDistance);
    //             return settings.movementSpeed * Mathf.Lerp(settings.minSpeedPercent, 1f, slowdownRatio);
    //         }
    //     }
    //     return settings.movementSpeed;
    // }
    //
    // // Tính toán hướng di chuyển dựa vào các điểm phía trước
    // private Vector3 CalculateFutureDirection(Vector3 currentPosition)
    // {
    //     Vector3 futureDirection = Vector3.zero;
    //     for (int i = 1; i <= settings.predictionSteps; i++)
    //     {
    //         float futureDistance = distanceTravelled + 
    //             (settings.lookAheadDistance * i / settings.predictionSteps);
    //         if (futureDistance > curveData.TotalLength && !loop) continue;
    //         
    //         Vector3 futurePos = curveData.GetPositionAlongCurve(
    //             futureDistance % curveData.TotalLength);
    //         futureDirection += (futurePos - currentPosition).normalized;
    //     }
    //     return futureDirection / settings.predictionSteps;
    // }
    //
    // // Kiểm tra và xử lý khi đến điểm cuối
    // private bool HasReachedEnd()
    // {
    //     if (distanceTravelled >= curveData.TotalLength)
    //     {
    //         if (loop)
    //         {
    //             distanceTravelled = 0f;
    //             return false;
    //         }
    //         
    //         distanceTravelled = curveData.TotalLength;
    //         Vector3 targetPos = lookAtTarget != null ? lookAtTarget.position : lookAtPosition;
    //         //rotationHandler.LookAtTarget(transform, targetPos, rotationTarget);
    //         return true;
    //     }
    //     return false;
    // }
    public Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        t = Mathf.Clamp01(t); // Đảm bảo t nằm trong khoảng [0,1]
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        // Công thức đường cong Bezier bậc 3
        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }
    private void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Count < 4)
        {
            Debug.LogWarning("Không đủ điểm điều khiển để vẽ đường cong.");
            return;
        }
    
        Vector3 startPoint = controlPoints[0].position;
        Vector3 controlPoint1 = controlPoints[1].position;
        Vector3 controlPoint2 = controlPoints[controlPoints.Count - 2].position;
        Vector3 endPoint = controlPoints[controlPoints.Count - 1].position;
    
        Gizmos.color = Color.green;
    
        Vector3 previousPoint = startPoint;
        for (int j = 1; j <= segment; j++)
        {
            float t = j / (float)segment;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);
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