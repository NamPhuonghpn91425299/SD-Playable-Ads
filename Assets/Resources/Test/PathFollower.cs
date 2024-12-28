using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [SerializeField] private Transform[] pathPoints; // Các điểm tạo thành đường dẫn
    [SerializeField] private bool connectPointsAsLoop = false; // Kết nối vòng lặp
    [SerializeField] private bool shouldMove = false; // Kiểm soát xem object có di chuyển không
    [SerializeField] private float moveSpeed = 2f; // Tốc độ di chuyển

    private float progress = 0f; // Theo dõi tiến trình di chuyển (0.0 - 1.0)
    private bool hasCompletedOneLoop = false; // Đánh dấu đã đi hết một vòng hay chưa

    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length < 2)
            return;

        Gizmos.color = Color.yellow;

        int segmentCount = connectPointsAsLoop ? pathPoints.Length : pathPoints.Length - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            int p0Index = i;
            int p1Index = (i + 1) % pathPoints.Length;

            Vector3 p0 = pathPoints[p0Index].position;
            Vector3 p1 = pathPoints[p1Index].position;

            Vector3 pPrev = (p0Index - 1 >= 0 || connectPointsAsLoop)
                ? pathPoints[(p0Index - 1 + pathPoints.Length) % pathPoints.Length].position
                : p0;
            Vector3 pNext = (p1Index + 1 < pathPoints.Length || connectPointsAsLoop)
                ? pathPoints[(p1Index + 1) % pathPoints.Length].position
                : p1;

            // Draw the curve in small t increments
            Vector3 lastPos = p0;
            for (float t = 0; t <= 1f; t += 0.05f)
            {
                Vector3 newPos = GetCatmullRomPosition(t, pPrev, p0, p1, pNext);
                Gizmos.DrawLine(lastPos, newPos);
                lastPos = newPos;
            }
        }
    }

    private void Update()
    {
        if (!shouldMove || pathPoints == null || pathPoints.Length < 2) return;

        // Tính tổng số segment (đường nối giữa 2 điểm)
        int segmentCount = connectPointsAsLoop ? pathPoints.Length : pathPoints.Length - 1;

        // Tiến trình di chuyển (theo thời gian thực)
        progress += Time.deltaTime * moveSpeed / segmentCount;

        // Nếu hoàn thành một vòng:
        if (progress > 1f)
        {
            hasCompletedOneLoop = true; // Đánh dấu đã hoàn thành một vòng
            progress = 0f; // Quay lại đầu đường dẫn

            if (!connectPointsAsLoop)
            {
                shouldMove = false; // Dừng lại nếu không phải vòng lặp
            }
        }

        // Tính toán vị trí hiện tại dựa trên progress
        UpdatePositionOnCurve(segmentCount);

        // Nếu đã hoàn thành một vòng, tự động dừng
        if (hasCompletedOneLoop)
        {
            shouldMove = false; // Dừng di chuyển
            hasCompletedOneLoop = false; // Reset trạng thái hoàn thành
        }
    }

    private void UpdatePositionOnCurve(int segmentCount)
    {
        float segmentProgress = progress * segmentCount; // Tổng số tiến trình trên toàn bộ các segment
        int currentSegment = Mathf.FloorToInt(segmentProgress); // Đoạn hiện tại (index)
        float t = segmentProgress - currentSegment; // Tỉ lệ nội suy trong đoạn hiện tại (0.0 - 1.0)

        // Đảm bảo index không vượt quá pathPoints
        int p0Index = currentSegment;
        int p1Index = (currentSegment + 1) % pathPoints.Length;

        // Xử lý logic không vòng lặp
        if (!connectPointsAsLoop && p1Index >= pathPoints.Length)
        {
            transform.position = pathPoints[pathPoints.Length - 1].position; // Dừng ở vị trí cuối
            return;
        }

        // Nếu chỉ có 2 điểm, sử dụng nội suy tuyến tính
        if (pathPoints.Length == 2)
        {
            transform.position = Vector3.Lerp(pathPoints[p0Index].position, pathPoints[p1Index].position, t);
            return;
        }

        // Tính toán các điểm Bezier trung gian
        int prevIndex = (p0Index - 1 + pathPoints.Length) % pathPoints.Length;
        int nextIndex = (p1Index + 1) % pathPoints.Length;

        if (!connectPointsAsLoop)
        {
            prevIndex = Mathf.Clamp(p0Index - 1, 0, pathPoints.Length - 1);
            nextIndex = Mathf.Clamp(p1Index + 1, 0, pathPoints.Length - 1);
        }

        // Sử dụng Bezier nội suy
        Vector3 p0 = pathPoints[prevIndex].position; // Điểm trước đó
        Vector3 p1 = pathPoints[p0Index].position; // Điểm hiện tại
        Vector3 p2 = pathPoints[p1Index].position; // Điểm tiếp theo
        Vector3 p3 = pathPoints[nextIndex].position; // Điểm sau nữa

        // Di chuyển trên đường cong sử dụng Catmull-Rom spline (hoặc áp dụng Bezier Cubic)
        Vector3 newPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);

        // Cập nhật vị trí object
        transform.position = newPosition;
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Công thức Catmull-Rom spline
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    public void ResumeMoving()
    {
        if (!shouldMove) // Chỉ tiếp tục khi đối tượng đang dừng
        {
            shouldMove = true; // Tiếp tục di chuyển
            hasCompletedOneLoop = false; // Đảm bảo di chuyển lại đúng logic một vòng
        }
    }

    public void StopMoving()
    {
        shouldMove = false; // Dừng di chuyển
    }
}