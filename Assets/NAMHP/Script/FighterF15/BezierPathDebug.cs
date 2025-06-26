using System.Collections.Generic;
using UnityEngine;

// Script sử dụng `QuadraticBezierPath` để vẽ debug trong Scene
[ExecuteInEditMode] // Cho phép hoạt động ngay trong chế độ Edit Mode
public class BezierPathDebug : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>(); // Các điểm waypoint

    [Header("Path Settings")]
    [SerializeField] private Color pathColor = Color.cyan; // Màu của đường cong
    [SerializeField] private Color controlPointColor = Color.red; // Màu của các điểm điều khiển
    [SerializeField] private float controlPointOffset = 0.5f; // Offset theo chiều cao để tính điểm điều khiển 
    [SerializeField] private int samplesPerCurve = 50; // Số lượng mẫu mỗi đoạn cong Bezier
#if UNITY_EDITOR
    
    private QuadraticBezierPath bezierPath; // Đối tượng xử lý logic đường cong Bezier

    private void OnValidate()
    {
        if (waypoints.Count >= 2)
        {
            // Khởi tạo lại BezierPath khi có thay đổi
            bezierPath = new QuadraticBezierPath(waypoints, controlPointOffset);
        }
    }

    private void OnDrawGizmos()
    {
        if (bezierPath != null)
        {
            // Vẽ debug trong Scene View sử dụng phương thức sẵn có
            bezierPath.DrawGizmos(pathColor, controlPointColor);
        }
    }
#endif
}
