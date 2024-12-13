using UnityEngine;
using System.Collections.Generic;

public class MultiPointCurvedMovement : MonoBehaviour
{
    [Header("Waypoints (Control Points)")]
    public List<Transform> controlPoints = new List<Transform>();

    [Header("Movement Settings")]
    public float movementSpeed = 1f; // Tốc độ di chuyển
    public float rotationSpeed = 90f; // Tốc độ xoay (độ/giây)
    public float rotation = 90f; // Tốc độ xoay (độ/giây)
    public float maxRollAngle = 30f; // Góc nghiêng tối đa trên trục Z
    public float rollSpeed = 5f; // Tốc độ làm mượt nghiêng góc Z
    public int debugSegmentsPerCurve = 20; // Số đoạn debug mỗi đường cong
    public bool loop = false; // Lặp lại
    public bool reverseAtEnd = false; // Đi ngược lại khi đến điểm cuối

    private float distanceTravelled = 0f; // Quãng đường đã đi
    private List<float> curveLengths = new List<float>(); // Chiều dài các đường cong
    private float totalLength = 0f; // Tổng chiều dài quỹ đạo
    private int currentCurveIndex = 0; // Chỉ số đoạn cong hiện tại
    private bool isReversing = false; // Đang đảo chiều không
    private bool isRotating = false; // Có đang xoay không
    private Vector3 rotationTargetDirection; // Hướng mục tiêu khi xoay

    private float currentRollAngle = 0f; // Góc nghiêng hiện tại

    void Start()
    {
        if (controlPoints.Count < 4 || controlPoints.Count % 3 != 1)
        {
            Debug.LogError("Số lượng điểm điều khiển phải là 3n+1 (ví dụ: 4, 7, 10).");
            return;
        }

        CalculateCurveLengths(); // Tính chiều dài các đường cong
    }

    void Update()
    {
        if (controlPoints.Count < 4 || controlPoints.Count % 3 != 1)
            return;

        if (isRotating)
        {
            HandleRotation(); // Xử lý xoay
            return; // Không di chuyển khi đang xoay
        }

        HandleMovement(); // Xử lý di chuyển
    }

    void HandleMovement()
    {
        // Di chuyển
        float movementStep = movementSpeed * Time.deltaTime;
        distanceTravelled += isReversing ? -movementStep : movementStep;

        // Kiểm tra vượt qua điểm cuối hoặc đầu
        if (distanceTravelled >= totalLength)
        {
            if (reverseAtEnd)
            {
                isReversing = true; // Đảo chiều
                distanceTravelled = totalLength; // Giữ vị trí ở cuối
                StartRotation(true); // Xoay ngược lại
            }
            else if (loop)
            {
                distanceTravelled = 0f; // Quay lại đầu
            }
            else
            {
                distanceTravelled = totalLength; // Dừng ở cuối
                return;
            }
        }
        else if (distanceTravelled <= 0f)
        {
            if (reverseAtEnd)
            {
                isReversing = false; // Đảo chiều
                distanceTravelled = 0f; // Giữ vị trí ở đầu
                StartRotation(false); // Xoay về phía trước
            }
            else if (loop)
            {
                distanceTravelled = totalLength; // Quay lại cuối
            }
            else
            {
                distanceTravelled = 0f; // Dừng ở đầu
                return;
            }
        }

        // Xác định `t` dựa trên chiều dài cung
        float targetLength = distanceTravelled;
        for (int i = 0; i < curveLengths.Count; i++)
        {
            if (targetLength <= curveLengths[i])
            {
                currentCurveIndex = i;
                break;
            }
            targetLength -= curveLengths[i];
        }

        float t = targetLength / curveLengths[currentCurveIndex];
        Vector3 newPosition = CalculateBezierPoint(
            t,
            controlPoints[currentCurveIndex * 3].position,
            controlPoints[currentCurveIndex * 3 + 1].position,
            controlPoints[currentCurveIndex * 3 + 2].position,
            controlPoints[currentCurveIndex * 3 + 3].position
        );

        // Xoay về hướng di chuyển
        Vector3 nextPosition = CalculateBezierPoint(
            Mathf.Clamp01(isReversing ? t - 0.01f : t + 0.01f),
            controlPoints[currentCurveIndex * 3].position,
            controlPoints[currentCurveIndex * 3 + 1].position,
            controlPoints[currentCurveIndex * 3 + 2].position,
            controlPoints[currentCurveIndex * 3 + 3].position
        );

        Vector3 directionToNext = (nextPosition - newPosition).normalized;

        if (directionToNext != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToNext, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Tính góc nghiêng Z (chao đảo)
            float targetRollAngle = Vector3.Dot(Vector3.right, directionToNext) * maxRollAngle;
            currentRollAngle = Mathf.Lerp(currentRollAngle, targetRollAngle, Time.deltaTime * rollSpeed);

            // Áp dụng góc Z (roll) vào rotation
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -currentRollAngle);
        }

        transform.position = newPosition;
    }

    void StartRotation(bool reverse)
    {
        isRotating = true;

        if (reverse)
        {
            // Quay lại từ cuối về đầu
            Vector3 lastCurveStart = controlPoints[controlPoints.Count - 4].position;
            Vector3 lastCurveEnd = controlPoints[controlPoints.Count - 1].position;
            rotationTargetDirection = (lastCurveStart - lastCurveEnd).normalized;
        }
        else
        {
            // Quay từ đầu về phía trước
            Vector3 firstCurveStart = controlPoints[0].position;
            Vector3 firstCurveEnd = controlPoints[3].position;
            rotationTargetDirection = (firstCurveEnd - firstCurveStart).normalized;
        }
    }

    void HandleRotation()
    {
        if (rotationTargetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rotationTargetDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotation * Time.deltaTime);

            // Dừng xoay khi đạt góc mục tiêu
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false; // Hoàn tất xoay
            }
        }
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

        // Xác định startpoint, endpoint và các điểm điều khiển
        Vector3 startPoint = controlPoints[0].position; // Điểm bắt đầu
        Vector3 controlPoint1 = controlPoints[1].position; // Điểm điều khiển 1
        Vector3 controlPoint2 = controlPoints[controlPoints.Count - 2].position; // Điểm điều khiển 2
        Vector3 endPoint = controlPoints[controlPoints.Count - 1].position; // Điểm kết thúc

        Gizmos.color = Color.green;

        // Vẽ đường cong Bezier từ startpoint đến endpoint
        Vector3 previousPoint = startPoint;
        for (int j = 1; j <= debugSegmentsPerCurve; j++)
        {
            float t = j / (float)debugSegmentsPerCurve;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        // Vẽ các điểm điều khiển để dễ quan sát
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPoint, 0.1f); // Startpoint
        Gizmos.DrawSphere(endPoint, 0.1f);   // Endpoint

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(controlPoint1, 0.1f); // Control Point 1
        Gizmos.DrawSphere(controlPoint2, 0.1f); // Control Point 2
    }

}
