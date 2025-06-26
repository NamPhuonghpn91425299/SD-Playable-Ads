using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lớp xử lý đường cong Bezier bậc 2
public class QuadraticBezierPath
{
    private List<Transform> waypoints;      // Các điểm cần đi qua
    private List<Vector3> controlPoints;    // Các điểm điều khiển tự động tính
    private List<float> segmentLengths;     // Độ dài của từng đoạn
    private float totalLength;              // Tổng độ dài đường đi
    private int samplesPerCurve = 50;       // Số điểm lấy mẫu cho mỗi đoạn cong

    public float TotalLength => totalLength;
    public IReadOnlyList<float> SegmentLengths => segmentLengths;

    public QuadraticBezierPath(List<Transform> points, float controlPointOffset = 0.5f)
    {
        Initialize(points, controlPointOffset);
    }

    // Khởi tạo đường đi với các điểm waypoint
    public void Initialize(List<Transform> points, float controlPointOffset)
    {
        if (points.Count < 2)
        {
            Debug.LogError("Cần ít nhất 2 điểm để tạo đường cong!");
            return;
        }

        waypoints = points;
        CalculateControlPoints(controlPointOffset);
        CalculateSegmentLengths();
    }

    // Tính toán các điểm điều khiển tự động
    private void CalculateControlPoints(float offset)
    {
        controlPoints = new List<Vector3>();
        
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 current = waypoints[i].position;
            Vector3 next = waypoints[i + 1].position;
            Vector3 direction = (next - current).normalized;
            
            // Tính điểm điều khiển ở giữa nhưng cao hơn một chút
            Vector3 controlPoint = (current + next) * 0.5f + Vector3.up * 
                (offset * Vector3.Distance(current, next));
            
            controlPoints.Add(controlPoint);
        }
    }

    // Tính độ dài của từng đoạn cong
    private void CalculateSegmentLengths()
    {
        segmentLengths = new List<float>();
        totalLength = 0f;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float length = CalculateSegmentLength(
                waypoints[i].position,
                controlPoints[i],
                waypoints[i + 1].position
            );
            segmentLengths.Add(length);
            totalLength += length;
        }
    }

    // Tính độ dài của một đoạn cong
    private float CalculateSegmentLength(Vector3 start, Vector3 control, Vector3 end)
    {
        float length = 0;
        Vector3 previousPoint = start;

        for (int i = 1; i <= samplesPerCurve; i++)
        {
            float t = i / (float)samplesPerCurve;
            Vector3 point = CalculateQuadraticBezierPoint(t, start, control, end);
            length += Vector3.Distance(previousPoint, point);
            previousPoint = point;
        }

        return length;
    }

    // Lấy vị trí trên đường cong theo khoảng cách
    public Vector3 GetPositionAlongPath(float distance)
    {
        if (distance <= 0) return waypoints[0].position;
        if (distance >= totalLength) return waypoints[waypoints.Count - 1].position;

        float remainingDistance = distance;
        int segmentIndex = 0;

        // Tìm đoạn cong hiện tại
        while (segmentIndex < segmentLengths.Count)
        {
            if (remainingDistance <= segmentLengths[segmentIndex])
                break;
            
            remainingDistance -= segmentLengths[segmentIndex];
            segmentIndex++;
        }

        // Tính tỉ lệ t trên đoạn cong hiện tại
        float t = remainingDistance / segmentLengths[segmentIndex];
        return CalculateQuadraticBezierPoint(t,
            waypoints[segmentIndex].position,
            controlPoints[segmentIndex],
            waypoints[segmentIndex + 1].position);
    }

    // Tính điểm trên đường cong Bezier bậc 2
    public Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    // Lấy hướng tại một điểm trên đường cong
    public Vector3 GetDirectionAtDistance(float distance, float deltaDistance = 0.1f)
    {
        Vector3 current = GetPositionAlongPath(distance);
        Vector3 next = GetPositionAlongPath(distance + deltaDistance);
        return (next - current).normalized;
    }
# if UNITY_EDITOR
    // Vẽ đường cong trong Scene view để debug
    public void DrawGizmos(Color pathColor, Color controlPointColor)
    {
        if (waypoints == null || waypoints.Count < 2 || controlPoints == null) return;

        Gizmos.color = pathColor;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 start = waypoints[i].position;
            Vector3 end = waypoints[i + 1].position;
            Vector3 control = controlPoints[i];

            Vector3 previous = start;
            for (int j = 1; j <= samplesPerCurve; j++)
            {
                float t = j / (float)samplesPerCurve;
                Vector3 current = CalculateQuadraticBezierPoint(t, start, control, end);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

        // Vẽ điểm điều khiển
        Gizmos.color = controlPointColor;
        foreach (var point in controlPoints)
        {
            Gizmos.DrawSphere(point, 0.5f);
        }
    }
#endif
}