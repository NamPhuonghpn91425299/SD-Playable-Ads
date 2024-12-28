using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SimplePath : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();
    [Range(1, 50)] public int resolution = 10;
    [Range(0f, 1f)] public float tension = 0.5f; // Thêm điều khiển độ căng
    public bool loop = false;
    public Color pathColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (points == null || points.Count < 2) return;

        Gizmos.color = pathColor;

        for (int i = 0; i < points.Count; i++)
        {
            Transform current = points[i];
            Transform next = points[(i + 1) % points.Count];

            if (current == null || next == null) continue;

            // Vẽ điểm chính
            Gizmos.DrawSphere(current.position, 0.1f);

            // Vẽ đoạn đường giữa hai điểm
            if (i < points.Count - 1 || loop)
            {
                DrawSegment(current.position, next.position, i);
            }
        }
    }

    private void DrawSegment(Vector3 p0, Vector3 p1, int index)
    {
        Vector3 lastPos = p0;
        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 interpolatedPos = GetCatmullRomPoint(GetPreviousPoint(index), p0, p1, GetNextPoint(index), t);
            Gizmos.DrawLine(lastPos, interpolatedPos);
            lastPos = interpolatedPos;
        }
    }

    public Vector3 GetPreviousPoint(int index)
    {
        if (index - 1 < 0)
            return loop ? points[points.Count - 1].position : points[0].position;

        return points[index - 1].position;
    }

    public Vector3 GetNextPoint(int index)
    {
        if (index + 1 >= points.Count)
            return loop ? points[0].position : points[points.Count - 1].position;

        return points[index + 1].position;
    }

    public Vector3 GetCatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Áp dụng tension vào công thức Catmull-Rom
        float alpha = tension; // alpha càng nhỏ, đường cong càng mượt
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 a = (-alpha * p0 + (2f - alpha) * p1 + (alpha - 2f) * p2 + alpha * p3);
        Vector3 b = (2f * alpha * p0 + (alpha - 3f) * p1 + (3f - 2f * alpha) * p2 - alpha * p3);
        Vector3 c = (-alpha * p0 + alpha * p2);
        Vector3 d = (p1);

        return t3 * a + t2 * b + t * c + d;
    }

    // Thêm phương thức để lấy điểm trên đường path
    public Vector3 GetPointOnPath(float t)
    {
        if (points.Count < 2) return Vector3.zero;

        // Đảm bảo t nằm trong khoảng [0,1]
        t = Mathf.Clamp01(t);
        
        float segmentLength = 1f / (points.Count - (loop ? 0 : 1));
        int index = Mathf.FloorToInt(t / segmentLength);
        float segmentT = (t - index * segmentLength) / segmentLength;

        if (index >= points.Count - 1 && !loop)
        {
            return points[points.Count - 1].position;
        }

        Vector3 p0 = GetPreviousPoint(index);
        Vector3 p1 = points[index].position;
        Vector3 p2 = points[(index + 1) % points.Count].position;
        Vector3 p3 = GetNextPoint(index);

        return GetCatmullRomPoint(p0, p1, p2, p3, segmentT);
    }
}