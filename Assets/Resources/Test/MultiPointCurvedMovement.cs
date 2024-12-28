using UnityEngine;
using System.Collections.Generic;

public class MultiPointCurvedMovement : MonoBehaviour
{
    [Header("Waypoints (Control Points)")]
    public List<Transform> controlPoints = new List<Transform>();

    [Header("Movement Settings")]
    public float movementSpeed = 400f;
    public bool loop = false;

    [Header("Aircraft Rotation")]
    public float rotationSpeed = 10f;
    public float bankingStrength = 15f;        // Độ mạnh của góc nghiêng khi bay vòng
    public float pitchStrength = 1f;        // Độ mạnh của góc pitch (ngẩng lên/cúi xuống)
    public float smoothRotationSpeed = 6f;    // Tốc độ làm mượt rotation
    public float maxBankAngle = 90f;          // Góc nghiêng tối đa
    public float maxPitchAngle = 30f;         // Góc pitch tối đa

    [Header("Look Ahead Settings")]
    public float lookAheadDistance = 10f;      // Khoảng cách nhìn trước để dự đoán đường bay
    public int predictionSteps = 50;          // Số bước dự đoán để làm mượt hướng bay

    [Header("Targeting")]
    public float rotationTarget = 5f;
    public Transform lookAtTarget;
    public Vector3 lookAtPosition;

    [Header("Debug")]
    public int debugSegmentsPerCurve = 20;
    public bool showDebugLines = false;

    private float distanceTravelled = 0f;
    private List<float> curveLengths = new List<float>();
    private float totalLength = 0f;
    private int currentCurveIndex = 0;
    private Quaternion targetRotation;
    private Vector3 smoothedDirection;

    void Start()
    {
        if (controlPoints.Count < 4 || controlPoints.Count % 3 != 1)
        {
            Debug.LogError("Số lượng điểm điều khiển phải là 3n+1 (ví dụ: 4, 7, 10).");
            return;
        }
        
        CalculateCurveLengths();
        smoothedDirection = transform.forward;
    }

    void Update()
    {
        if (controlPoints.Count < 4 || controlPoints.Count % 3 != 1)
            return;

        HandleMovement();
    }

    void HandleMovement()
    {
        float movementStep = movementSpeed * Time.deltaTime;
        distanceTravelled += movementStep;

        if (distanceTravelled >= totalLength)
        {
            if (loop)
            {
                distanceTravelled = 0f;
            }
            else
            {
                distanceTravelled = totalLength;
                LookAtTarget();
                return;
            }
        }

        // Tính vị trí hiện tại
        Vector3 currentPosition = GetPositionAlongCurve(distanceTravelled);
        
        // Dự đoán hướng bay trong tương lai
        Vector3 futureDirection = Vector3.zero;
        for (int i = 1; i <= predictionSteps; i++)
        {
            float futureDistance = distanceTravelled + (lookAheadDistance * i / predictionSteps);
            if (futureDistance > totalLength && !loop) continue;
            
            Vector3 futurePos = GetPositionAlongCurve(futureDistance % totalLength);
            futureDirection += (futurePos - currentPosition).normalized;
        }
        futureDirection /= predictionSteps;

        // Làm mượt hướng bay
        smoothedDirection = Vector3.Slerp(smoothedDirection, futureDirection, Time.deltaTime * smoothRotationSpeed);

        // Tính toán rotation cho máy bay
        if (smoothedDirection != Vector3.zero)
        {
            // Tính góc nghiêng dựa trên độ cong của đường bay
            Vector3 right = Vector3.Cross(Vector3.up, smoothedDirection).normalized;
            float turnRate = Vector3.Dot(right, futureDirection);
            float bankAngle = -turnRate * bankingStrength * maxBankAngle;
            bankAngle = Mathf.Clamp(bankAngle, -maxBankAngle, maxBankAngle);

            // Tính góc pitch dựa trên hướng lên/xuống
            float pitchAngle = Mathf.Asin(smoothedDirection.y) * Mathf.Rad2Deg;
            pitchAngle = Mathf.Clamp(pitchAngle * pitchStrength, -maxPitchAngle, maxPitchAngle);

            // Tạo rotation mục tiêu
            Quaternion directionRotation = Quaternion.LookRotation(smoothedDirection);
            Quaternion bankRotation = Quaternion.Euler(pitchAngle, 0, bankAngle);
            targetRotation = directionRotation * bankRotation;

            // Áp dụng rotation một cách mượt mà
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Cập nhật vị trí
        transform.position = currentPosition;

        if (showDebugLines)
        {
            Debug.DrawLine(currentPosition, currentPosition + smoothedDirection * 5f, Color.blue);
            Debug.DrawLine(currentPosition, currentPosition + futureDirection * 5f, Color.red);
        }
    }

    Vector3 GetPositionAlongCurve(float distance)
    {
        float remainingDistance = distance;
        int curveIndex = 0;

        // Tìm đoạn cong hiện tại
        for (int i = 0; i < curveLengths.Count; i++)
        {
            if (remainingDistance <= curveLengths[i])
            {
                curveIndex = i;
                break;
            }
            remainingDistance -= curveLengths[i];
        }

        float t = remainingDistance / curveLengths[curveIndex];
        return CalculateBezierPoint(
            t,
            controlPoints[curveIndex * 3].position,
            controlPoints[curveIndex * 3 + 1].position,
            controlPoints[curveIndex * 3 + 2].position,
            controlPoints[curveIndex * 3 + 3].position
        );
    }

    void LookAtTarget()
    {
        Vector3 targetPos = lookAtTarget != null ? lookAtTarget.position : lookAtPosition;
        Vector3 directionToTarget = (targetPos - transform.position).normalized;
        
        // Tính góc nghiêng khi nhìn vào mục tiêu
        float bankAngle = Vector3.Dot(transform.right, directionToTarget) * maxBankAngle * 0.5f;
        
        Quaternion targetLookRotation = Quaternion.LookRotation(directionToTarget) * 
                                      Quaternion.Euler(0, 0, bankAngle);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetLookRotation,
            Time.deltaTime * rotationTarget
        );
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        t = Mathf.Clamp01(t);
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    void CalculateCurveLengths()
    {
        curveLengths.Clear();
        totalLength = 0f;

        for (int i = 0; i <= controlPoints.Count - 4; i += 3)
        {
            float curveLength = CalculateCurveLength(
                controlPoints[i].position,
                controlPoints[i + 1].position,
                controlPoints[i + 2].position,
                controlPoints[i + 3].position
            );
            curveLengths.Add(curveLength);
            totalLength += curveLength;
        }
    }

    float CalculateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float length = 0f;
        Vector3 previousPoint = p0;

        for (int i = 1; i <= debugSegmentsPerCurve; i++)
        {
            float t = i / (float)debugSegmentsPerCurve;
            Vector3 currentPoint = CalculateBezierPoint(t, p0, p1, p2, p3);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return length;
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
        for (int j = 1; j <= debugSegmentsPerCurve; j++)
        {
            float t = j / (float)debugSegmentsPerCurve;
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