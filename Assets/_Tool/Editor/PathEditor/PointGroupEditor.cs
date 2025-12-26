#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq; // Cần thiết cho LINQ operations
using System; // Cần thiết cho Cast operations
using System.Collections.Generic; // Cần thiết cho List
// Script này tùy chỉnh giao diện Inspector và các công cụ trong Scene View cho PointGroup.
[CustomEditor(typeof(PointGroup))]
public class PointGroupEditor : Editor
{
    private PointGroup pointGroup;

    // Biến để lưu trữ các cài đặt của Editor, không cần serialize
    private bool autoRenameChildren = true;
    private int groundLayerIndex = 3;
    
    /// <summary>
    /// Được gọi mỗi khi script được kích hoạt hoặc khi người dùng chọn đối tượng.
    /// </summary>
    private void OnEnable()
    {
        // Lấy tham chiếu đến component PointGroup đang được chỉnh sửa.
        pointGroup = (PointGroup)target;
        // Luôn cập nhật danh sách các điểm để đảm bảo dữ liệu là mới nhất.
        if (pointGroup != null)
        {
            pointGroup.UpdatePoints();
        }
    }

    /// <summary>
    /// Hàm này được Unity gọi để vẽ các công cụ (Handles) trong cửa sổ Scene View.
    /// </summary>
    private void OnSceneGUI()
    {
        if (pointGroup == null || pointGroup.points == null) return;

        // Vẽ handles cho waypoints và attack points với màu sắc khác nhau
        DrawPointHandles();
    }
    
    /// <summary>
    /// Vẽ handles cho các điểm với màu sắc khác nhau cho waypoints và attack points
    /// </summary>
    private void DrawPointHandles()
    {
        // Vẽ handles cho waypoints
        Handles.color = Color.cyan;
        DrawHandlesForPointList(pointGroup.points, "Move Waypoint Handle");
        
        // Vẽ handles cho attack points
        Handles.color = Color.red;
        DrawHandlesForPointList(pointGroup.attackPoints, "Move Attack Point Handle");
        
        // Vẽ handles cho left points
        Handles.color = Color.blue;
        DrawHandlesForPointList(pointGroup.leftPoints, "Move Left Point Handle");
        
        // Vẽ handles cho right points
        Handles.color = Color.green;
        DrawHandlesForPointList(pointGroup.rightPoints, "Move Right Point Handle");
        
        // Reset màu
        Handles.color = Color.white;
    }
    
    /// <summary>
    /// Vẽ handles cho một danh sách điểm
    /// </summary>
    private void DrawHandlesForPointList(List<Transform> pointList, string undoName)
    {
        for (int i = 0; i < pointList.Count; i++)
        {
            Transform currentPoint = pointList[i];
            if (currentPoint == null) continue;

            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(currentPoint.position, Quaternion.identity);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(currentPoint, undoName);
                currentPoint.position = newPosition;
                EditorUtility.SetDirty(currentPoint);
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra xem điểm có phải là attack point không dựa trên tên
    /// </summary>
    private bool IsAttackPoint(string pointName)
    {
        if (string.IsNullOrEmpty(pointName)) return false;
        return pointName.ToLower().Contains("attackpoint");
    }
    
    /// <summary>
    /// Lấy chỉ số của điểm từ tên
    /// </summary>
    private string GetPointIndex(string pointName)
    {
        if (string.IsNullOrEmpty(pointName)) return "?";
        
        string[] parts = pointName.Split('_');
        if (parts.Length > 0)
        {
            return parts[parts.Length - 1]; // Lấy phần cuối cùng
        }
        
        return "?";
    }

    /// <summary>
    /// Hàm này chịu trách nhiệm vẽ giao diện tùy chỉnh trong cửa sổ Inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Vẽ các trường public mặc định của PointGroup (như routeType, baseName, gizmo settings).
        DrawDefaultInspector();

        // Hiển thị thống kê điểm
        DrawPointStatistics();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Automatic Management", EditorStyles.boldLabel);
        
        autoRenameChildren = EditorGUILayout.Toggle("Auto Rename Children", autoRenameChildren);
        
        if (GUILayout.Button("Update and Rename Points Now"))
        {
            UpdateAndRenamePoints();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Ground Snapping Utility", EditorStyles.boldLabel);
        
        // Vẽ một menu thả xuống cho phép chọn layer.
        groundLayerIndex = EditorGUILayout.LayerField("Ground Layer", groundLayerIndex);
        
        // Núts snap riêng biệt
        DrawSnapButtons();
    }
    
    /// <summary>
    /// Hiển thị thống kê số lượng waypoints và attack points
    /// </summary>
    private void DrawPointStatistics()
    {
        if (pointGroup == null) return;
        
        int waypointCount = pointGroup.points != null ? pointGroup.points.Count : 0;
        int attackPointCount = pointGroup.attackPoints != null ? pointGroup.attackPoints.Count : 0;
        int leftPointCount = pointGroup.leftPoints != null ? pointGroup.leftPoints.Count : 0;
        int rightPointCount = pointGroup.rightPoints != null ? pointGroup.rightPoints.Count : 0;
        int totalCount = waypointCount + attackPointCount + leftPointCount + rightPointCount;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical("box");
        
        // First row
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Waypoints: {waypointCount}", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Attack: {attackPointCount}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        
        // Second row - Aircraft points
        if (IsAircraftType())
        {
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.blue;
            GUILayout.Label($"Left: {leftPointCount}", EditorStyles.miniLabel);
            GUI.color = Color.green;
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Right: {rightPointCount}", EditorStyles.miniLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        
        // Total
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Total: {totalCount}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// Kiểm tra xem đây có phải là aircraft type không
    /// </summary>
    private bool IsAircraftType()
    {
        return pointGroup.botMoveType == GameConstants.BotMoveType.Aircraft_Y8_01 ||
               pointGroup.botMoveType == GameConstants.BotMoveType.Aircraft_Swordfish;
    }
    
    /// <summary>
    /// Vẽ các nút snap riêng biệt cho waypoints và attack points
    /// </summary>
    private void DrawSnapButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Snap Waypoints"))
        {
            SnapSpecificPointsToGround(pointGroup.points, "Waypoints");
        }
        
        if (GUILayout.Button("Snap Attack Points"))
        {
            SnapSpecificPointsToGround(pointGroup.attackPoints, "Attack Points");
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Aircraft-specific snap buttons
        if (IsAircraftType())
        {
            EditorGUILayout.BeginHorizontal();
            
            GUI.color = Color.blue;
            if (GUILayout.Button("Snap Left Points"))
            {
                SnapSpecificPointsToGround(pointGroup.leftPoints, "Left Points");
            }
            
            GUI.color = Color.green;
            if (GUILayout.Button("Snap Right Points"))
            {
                SnapSpecificPointsToGround(pointGroup.rightPoints, "Right Points");
            }
            
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("Snap All Points to Ground"))
        {
            SnapAllPointsToGround();
        }
    }

    /// <summary>
    /// Hàm thực hiện việc cập nhật danh sách và đổi tên các điểm con.
    /// </summary>
    private void UpdateAndRenamePoints()
    {
        if (pointGroup == null) return;
        
        // Luôn cập nhật danh sách trước khi đổi tên để đảm bảo thứ tự đúng.
        pointGroup.UpdatePoints();

        if (autoRenameChildren)
        {
            for (int i = 0; i < pointGroup.points.Count; i++)
            {
                string newName = $"{pointGroup.baseName}_{i}"; 
                Transform currentPoint = pointGroup.points[i];
                if (currentPoint != null && currentPoint.name != newName)
                {
                    Undo.RecordObject(currentPoint.gameObject, "Rename Point");
                    currentPoint.name = newName;
                    EditorUtility.SetDirty(currentPoint.gameObject);
                }
            }
        }
        // Đánh dấu PointGroup là "bẩn" để thay đổi (như danh sách `points`) được lưu.
        EditorUtility.SetDirty(pointGroup);
    }
    
    /// <summary>
    /// Snap các điểm cụ thể xuống đất
    /// </summary>
    private void SnapSpecificPointsToGround(List<Transform> pointsList, string pointTypeName)
    {
        if (pointGroup == null || pointsList == null) return;
        
        LayerMask groundMask = (1 << groundLayerIndex);
        float groundCheckDistance = 200f;
        int snappedCount = 0;
        
        if (pointsList.Count == 0)
        {
            Debug.Log($"No {pointTypeName.ToLower()} found in group '{pointGroup.name}'.");
            return;
        }
        
        var pointsToSnap = pointsList.Where(p => p != null).ToArray();
        
        if (pointsToSnap.Length == 0)
        {
            Debug.Log($"No valid {pointTypeName.ToLower()} found in group '{pointGroup.name}'.");
            return;
        }
        
        foreach (Transform point in pointsToSnap)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position + Vector3.up * (groundCheckDistance / 2), 
                Vector3.down, out hit, groundCheckDistance, groundMask))
            {
                point.position = hit.point;
                snappedCount++;
            }
        }
        
        Debug.Log($"Snapped {snappedCount}/{pointsToSnap.Length} {pointTypeName.ToLower()} for group '{pointGroup.name}'.");
    }
    
    /// <summary>
    /// Hàm mới thực hiện việc "thả" tất cả các điểm về lại mặt đất.
    /// </summary>
    private void SnapAllPointsToGround()
    {
        if (pointGroup == null) return;
        
        LayerMask groundMask = (1 << groundLayerIndex);
        float groundCheckDistance = 200f;
        int snappedCount = 0;
        int totalPoints = 0;
        
        // Thu thập tất cả các điểm từ tất cả danh sách
        var allPoints = new List<Transform>();
        
        if (pointGroup.points != null)
        {
            allPoints.AddRange(pointGroup.points.Where(p => p != null));
        }
        
        if (pointGroup.attackPoints != null)
        {
            allPoints.AddRange(pointGroup.attackPoints.Where(p => p != null));
        }
        
        if (pointGroup.leftPoints != null)
        {
            allPoints.AddRange(pointGroup.leftPoints.Where(p => p != null));
        }
        
        if (pointGroup.rightPoints != null)
        {
            allPoints.AddRange(pointGroup.rightPoints.Where(p => p != null));
        }
        
        totalPoints = allPoints.Count;
        
        if (totalPoints == 0)
        {
            Debug.Log($"No points found in group '{pointGroup.name}'.");
            return;
        }
        
        foreach (Transform point in allPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position + Vector3.up * (groundCheckDistance / 2), 
                Vector3.down, out hit, groundCheckDistance, groundMask))
            {
                point.position = hit.point;
                snappedCount++;
            }
        }
        
        Debug.Log($"Snapped {snappedCount}/{totalPoints} points for group '{pointGroup.name}'.");
    }
}
#endif