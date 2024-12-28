using UnityEngine;

public class CurvedMovement : MonoBehaviour
{
    [Header("Control Points")]
    public Transform startPoint;     // Điểm bắt đầu
    public Transform endPoint;       // Điểm kết thúc

    [Header("Control Point Options")]
    public bool useCustomControlPoint = false;
    public Vector3 customControlPoint;  // Điểm điều khiển tùy chỉnh
    public float controlPointAngle = 45f;  // Góc điểm điều khiển
    public float controlPointDistance = 5f;  // Khoảng cách điểm điều khiển

    [Header("Movement Settings")]
    public float movementSpeed = 1f;     // Tốc độ di chuyển
    public float rotationSpeed = 5f;     // Tốc độ xoay

    [Header("Movement Direction")]
    public bool enableBidirectionalMovement = true;  // Cho phép di chuyển hai chiều

    [Header("Debug Settings")]
    public int debugSegments = 20;       // Số đoạn vẽ đường cong

    private float t = 0f;                // Biến tiến độ (0 đến 1)
    private bool movingForward = true;   // Hướng di chuyển hiện tại

    [Header("Advanced Rotation")]
    public float maxBankAngle = 45f;
    public float maxPitchAngle = 30f;
    public float rotationSmoothing = 5f;

    [Header("Targeting")]
    public Transform lookAtTarget;  // Điểm máy bay sẽ nhìn vào
    public Vector3 lookAtPosition;  // Hoặc sử dụng Vector3 trực tiếp
    // Tính toán điểm điều khiển
    private Vector3 GetControlPoint()
    {
        if (useCustomControlPoint)
        {
            return customControlPoint;
        }

        // Tính toán điểm điều khiển dựa trên góc và khoảng cách
        Vector3 direction = (endPoint.position - startPoint.position);
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        // Xoay vector vuông góc theo góc đã chỉ định
        Quaternion rotation = Quaternion.AngleAxis(controlPointAngle, direction.normalized);
        Vector3 offsetDirection = rotation * perpendicular;

        return startPoint.position + direction / 2 + offsetDirection * controlPointDistance;
    }
    void Update()
    {
        // Tính toán điểm điều khiển động
        Vector3 controlPoint = GetControlPoint();

        // Điều chỉnh tiến độ di chuyển
        if (movingForward)
        {
            t += Time.deltaTime * movementSpeed;
            if (t >= 1f)
            {
                // Đã đến điểm cuối
                if (enableBidirectionalMovement)
                {
                    t = 1f;
                    movingForward = false;
                }
                else
                {
                    t = 1f; // Dừng lại ở điểm cuối
                }
            }
        }
        else
        {
            t -= Time.deltaTime * movementSpeed;
            if (t <= 0f)
            {
                // Đã quay lại điểm bắt đầu
                if (enableBidirectionalMovement)
                {
                    t = 0f;
                    movingForward = true;
                }
                else
                {
                    t = 0f; // Dừng lại ở điểm bắt đầu
                }
            }
        }

        // Tính toán vị trí mới theo đường cong Bezier
        Vector3 currentPosition, nextPosition, directionToNext;

        if (movingForward)
        {
            currentPosition = CalculateBezierPoint(t, startPoint.position, controlPoint, endPoint.position);
            nextPosition = CalculateBezierPoint(Mathf.Clamp01(t + 0.01f), startPoint.position, controlPoint, endPoint.position);
            directionToNext = (nextPosition - currentPosition).normalized;
        }
        else
        {
            currentPosition = CalculateBezierPoint(1f - t, endPoint.position, controlPoint, startPoint.position);
            nextPosition = CalculateBezierPoint(Mathf.Clamp01((1f - t) + 0.01f), endPoint.position, controlPoint, startPoint.position);
            directionToNext = (currentPosition - nextPosition).normalized;
        }

        // Gán vị trí mới cho đối tượng
        transform.position = currentPosition;

        // Tính toán góc xoay nâng cao
        if (directionToNext != Vector3.zero)
        {
            // Tính toán góc nghiêng (nghiêng trái hoặc phải)
            float bankAngle = movingForward ? -CalculateBankAngle(directionToNext) : CalculateBankAngle(directionToNext);

            // Tính toán góc lắc (lên hoặc xuống)
            //float pitchAngle = movingForward ? -CalculatePitchAngle(directionToNext) : CalculatePitchAngle(directionToNext);
            float pitchAngle = movingForward ? -maxPitchAngle : maxPitchAngle;

            // Tạo rotation tổng hợp
            Quaternion targetRotation = Quaternion.Euler(
                pitchAngle, // Lắc
                transform.rotation.eulerAngles.y, // Giữ nguyên góc yaw
                bankAngle  // Nghiêng
            );

            // Làm mượt chuyển động xoay
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
        }

        // Tính toán điểm nhìn
        Vector3 targetLookPosition = lookAtTarget != null
            ? lookAtTarget.position
            : lookAtPosition;

        // Tạo rotation hướng về điểm mục tiêu
        Quaternion targetLookRotation = Quaternion.LookRotation(
            (targetLookPosition - transform.position).normalized,
            Vector3.up
        );

        // Làm mượt chuyển động xoay
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetLookRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    // Tính góc nghiêng (banking)
    float CalculateBankAngle(Vector3 movementDirection)
    {
        // Sử dụng vector ngang để tính góc nghiêng
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(movementDirection, Vector3.up);
        // Tính toán góc dựa trên độ lệch so với hướng chính
        float bankAngle = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);

        // Giới hạn góc nghiêng
        return Mathf.Clamp(bankAngle, -maxBankAngle, maxBankAngle);
    }

    // Tính góc lắc (pitch)
    float CalculatePitchAngle(Vector3 movementDirection)
    {
        // Chiếu vector di chuyển xuống mặt phẳng ngang
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(movementDirection, Vector3.right);

        // Tính góc giữa hướng di chuyển và phương ngang
        float pitchAngle = Vector3.SignedAngle(Vector3.forward, movementDirection, Vector3.right);

        // Giới hạn góc lắc
        return Mathf.Clamp(pitchAngle, -maxPitchAngle, maxPitchAngle);
    }
    // Hàm tính toán điểm trên đường Bezier bậc hai
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        t = Mathf.Clamp01(t); // Đảm bảo t trong khoảng [0, 1]
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = uu * p0;         // (1-t)^2 * p0
        point += 2 * u * t * p1;         // 2*(1-t)*t*p1
        point += tt * p2;                // t^2 * p2
        return point;
    }

    // Vẽ Gizmos trong Scene View
    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        // Tính toán điểm điều khiển để hiển thị
        Vector3 controlPoint = GetControlPoint();

        Gizmos.color = Color.green;
        // Vẽ đường cong Bezier
        Vector3 previousPoint = startPoint.position;
        for (int i = 1; i <= debugSegments; i++)
        {
            float t = i / (float)debugSegments;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint.position, controlPoint, endPoint.position);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        // Vẽ các điểm điều khiển
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPoint.position, 0.1f);     // Điểm bắt đầu
        Gizmos.DrawSphere(controlPoint, 0.1f);            // Điểm điều khiển
        Gizmos.DrawSphere(endPoint.position, 0.1f);       // Điểm kết thúc

        // Vẽ đường nối các điểm điều khiển
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPoint.position, controlPoint);
        Gizmos.DrawLine(controlPoint, endPoint.position);
    }

    // Phương thức để điều khiển hướng di chuyển theo ý muốn
    public void ReverseDirection()
    {
        movingForward = !movingForward;
    }
}


