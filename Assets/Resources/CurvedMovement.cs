using UnityEngine;

public class CurvedMovement : MonoBehaviour
{
    [Header("Control Points")]
    public Transform startPoint; // Điểm bắt đầu
    public Transform endPoint;   // Điểm kết thúc
    public Transform controlPoint; // Điểm điều khiển để điều chỉnh độ cong

    [Header("Movement Settings")]
    public float movementSpeed = 1f; // Tốc độ di chuyển
    public float rotationSpeed = 1f; // Tốc độ di chuyển
    private float t = 0f; // Biến tiến độ (0 đến 1)

    [Header("Debug Settings")]
    public int debugSegments = 20; // Số đoạn để vẽ đường cong

    void Update()
    {
        // Tăng tiến độ theo thời gian
        t += Time.deltaTime * movementSpeed;

        if (t > 1f)
        {
            t = 0f; // Reset khi hoàn thành (nếu cần lặp)
        }

        // Tính toán vị trí mới theo đường cong Bezier
        Vector3 currentPosition = CalculateBezierPoint(t, startPoint.position, controlPoint.position, endPoint.position);

        // Hướng về phía điểm kế tiếp
        Vector3 nextPosition = CalculateBezierPoint(Mathf.Clamp01(t + 0.01f), startPoint.position, controlPoint.position, endPoint.position);
        Vector3 directionToNext = (nextPosition - currentPosition).normalized;

        // Xoay đối tượng hướng về phía điểm kế tiếp
        if (directionToNext != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToNext, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Gán vị trí mới cho đối tượng
        transform.position = currentPosition;
    }

    // Hàm tính toán điểm trên đường Bezier bậc hai
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        t = Mathf.Clamp01(t); // Đảm bảo t trong khoảng [0, 1]

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = uu * p0;        // (1-t)^2 * p0
        point += 2 * u * t * p1;        // 2*(1-t)*t*p1
        point += tt * p2;               // t^2 * p2

        return point;
    }

    // Vẽ Gizmos trong Scene View
    private void OnDrawGizmos()
    {
        if (startPoint == null || controlPoint == null || endPoint == null) return;

        Gizmos.color = Color.green;

        // Vẽ đường cong Bezier
        Vector3 previousPoint = startPoint.position;

        for (int i = 1; i <= debugSegments; i++)
        {
            float t = i / (float)debugSegments;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint.position, controlPoint.position, endPoint.position);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        // Vẽ các điểm điều khiển
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPoint.position, 0.1f); // Điểm bắt đầu
        Gizmos.DrawSphere(controlPoint.position, 0.1f); // Điểm điều khiển
        Gizmos.DrawSphere(endPoint.position, 0.1f); // Điểm kết thúc

        // Vẽ đường nối các điểm điều khiển
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPoint.position, controlPoint.position);
        Gizmos.DrawLine(controlPoint.position, endPoint.position);
    }
}
