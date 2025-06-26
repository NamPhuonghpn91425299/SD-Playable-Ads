using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lớp xử lý dữ liệu và tính toán đường cong Bezier
public class BezierCurveData
{
    // Các điểm điều khiển để tạo đường cong
    private List<Transform> controlPoints = new List<Transform>();
    // Độ dài của từng đoạn cong
    private List<float> curveLengths = new List<float>();
    // Tổng độ dài của toàn bộ đường đi
    private float totalLength = 0f;
    // Số đoạn được chia để tính toán và vẽ debug
    public int debugSegmentsPerCurve = 50;

    // Properties chỉ đọc để truy cập dữ liệu
    public float TotalLength => totalLength;
    public IReadOnlyList<float> CurveLengths => curveLengths;
    public IReadOnlyList<Transform> ControlPoints => controlPoints;

    // Khởi tạo dữ liệu đường cong
    public void Initialize(List<Transform> points, int segments = 50)
    {
        controlPoints = points;
        debugSegmentsPerCurve = segments;
        CalculateCurveLengths();
    }

    // Kiểm tra tính hợp lệ của số điểm điều khiển
    // Số điểm phải >= 4 và có dạng 3n+1
    public bool IsValid() => controlPoints.Count >= 4 && controlPoints.Count % 3 == 1;

    // Tính toán độ dài của từng đoạn cong
    private void CalculateCurveLengths()
    {
        curveLengths.Clear();
        totalLength = 0f;

        // Tính độ dài cho từng đoạn cong Bezier
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

    // Tính độ dài của một đoạn cong bằng cách chia nhỏ thành các đoạn thẳng
    private float CalculateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float length = 0f;
        Vector3 previousPoint = p0;

        for (int i = 1; i <= debugSegmentsPerCurve; i++)
        {
            float t = Mathf.Max((float)i / debugSegmentsPerCurve, Mathf.Epsilon);
            Vector3 currentPoint = CalculateBezierPoint(t, p0, p1, p2, p3);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return length;
    }

    // Lấy vị trí trên đường cong dựa vào khoảng cách đã di chuyển
    public Vector3  GetPositionAlongCurve(float distance)
    {
        distance = Mathf.Clamp(distance, 0, TotalLength);
        float remainingDistance = distance;
        int curveIndex = 0;

        // Tìm đoạn cong hiện tại dựa vào khoảng cách
        for (int i = 0; i < curveLengths.Count; i++)
        {
            if (remainingDistance <= curveLengths[i])
            {
                curveIndex = i;
                break;
            }
            remainingDistance -= curveLengths[i];
        }

        // Tính tỉ lệ t trên đoạn cong hiện tại
        float t = remainingDistance / curveLengths[curveIndex];
        return CalculateBezierPoint(
            t,
            controlPoints[curveIndex * 3].position,
            controlPoints[curveIndex * 3 + 1].position,
            controlPoints[curveIndex * 3 + 2].position,
            controlPoints[curveIndex * 3 + 3].position
        );
    }

    // Tính toán điểm trên đường cong Bezier với tham số t
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
}