using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugRaycast))]
public class DebugRaycastEditor : Editor
{
    private DebugRaycast targetScript;
    
    void OnEnable()
    {
        targetScript = (DebugRaycast)target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== EDITOR DEBUG ===", EditorStyles.boldLabel);
        
        if (targetScript.startPoint == null || targetScript.endPoint == null)
        {
            EditorGUILayout.HelpBox("Gán Start Point và End Point để debug!", MessageType.Warning);
            return;
        }
        
        // Tính toán thông tin raycast
        Vector3 startPos = targetScript.startPoint.position;
        Vector3 endPos = targetScript.endPoint.position;
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        float actualDistance = Mathf.Min(distance, targetScript.maxDistance);
        
        // Hiển thị thông tin
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Raycast Info:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Start Position: {startPos}");
        EditorGUILayout.LabelField($"End Position: {endPos}");
        EditorGUILayout.LabelField($"Direction: {direction}");
        EditorGUILayout.LabelField($"Distance: {actualDistance:F2}m");
        
        // Button để thực hiện raycast trong Editor
        EditorGUILayout.Space();
        if (GUILayout.Button("Perform Raycast (Editor Mode)", GUILayout.Height(30)))
        {
            PerformEditorRaycast();
        }
        
        if (GUILayout.Button("Clear Console", GUILayout.Height(25)))
        {
            ClearConsole();
        }
        
        // Auto refresh Scene view
        EditorGUILayout.Space();
        bool autoRefresh = EditorGUILayout.Toggle("Auto Refresh Scene View", targetScript.showDebugRay);
        if (autoRefresh != targetScript.showDebugRay)
        {
            targetScript.showDebugRay = autoRefresh;
            SceneView.RepaintAll();
        }
    }
    
    /// <summary>
    /// Thực hiện raycast trong Editor mode
    /// </summary>
    void PerformEditorRaycast()
    {
        if (targetScript.startPoint == null || targetScript.endPoint == null)
        {
            Debug.LogWarning("[DebugRaycast Editor] Start point hoặc End point chưa được gán!");
            return;
        }
        
        Vector3 startPos = targetScript.startPoint.position;
        Vector3 endPos = targetScript.endPoint.position;
        
        // Tính hướng và khoảng cách
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        float actualDistance = Mathf.Min(distance, targetScript.maxDistance);
        
        // Thực hiện raycast
        RaycastHit hit;
        bool hasHit = Physics.Raycast(startPos, direction, out hit, actualDistance, targetScript.raycastLayer);
        
        if (hasHit)
        {
            Debug.Log($"<color=green>[DebugRaycast Editor] HIT!</color>" +
                     $"\n- Object: <color=yellow>{hit.collider.gameObject.name}</color>" +
                     $"\n- Hit Point: {hit.point}" +
                     $"\n- Distance: <color=cyan>{hit.distance:F2}m</color>" +
                     $"\n- Direction: {direction}" +
                     $"\n- Normal: {hit.normal}" +
                     $"\n- Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            // Ping object trong Hierarchy
            EditorGUIUtility.PingObject(hit.collider.gameObject);
        }
        else
        {
            Debug.Log($"<color=red>[DebugRaycast Editor] NO HIT</color>" +
                     $"\n- Direction: {direction}" +
                     $"\n- Max Distance: <color=cyan>{actualDistance:F2}m</color>" +
                     $"\n- End Point: {startPos + direction * actualDistance}");
        }
        
        // Debug thông tin hướng chi tiết
        DebugDirectionInfo(startPos, endPos, direction);
        
        // Refresh Scene view để hiển thị gizmos
        SceneView.RepaintAll();
    }
    
    /// <summary>
    /// Debug thông tin hướng chi tiết trong Editor
    /// </summary>
    void DebugDirectionInfo(Vector3 startPos, Vector3 endPos, Vector3 direction)
    {
        Vector3 directionVector = endPos - startPos;
        
        Debug.Log($"<color=blue>[DebugRaycast Editor] DIRECTION INFO:</color>" +
                 $"\n- Start: {startPos}" +
                 $"\n- End: {endPos}" +
                 $"\n- Direction Vector: {directionVector}" +
                 $"\n- Direction (normalized): {direction}" +
                 $"\n- Magnitude: <color=orange>{directionVector.magnitude:F2}</color>" +
                 $"\n- Angle (Y-axis): <color=purple>{Vector3.Angle(direction, Vector3.up):F1}°</color>" +
                 $"\n- Angle (Forward): <color=purple>{Vector3.Angle(direction, Vector3.forward):F1}°</color>" +
                 $"\n- Angle (Right): <color=purple>{Vector3.Angle(direction, Vector3.right):F1}°</color>" +
                 $"\n- Euler Angles: {Quaternion.LookRotation(direction).eulerAngles}");
    }
    
    /// <summary>
    /// Clear console log
    /// </summary>
    void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
    
    /// <summary>
    /// Vẽ gizmos trong Scene view kể cả khi không play
    /// </summary>
    void OnSceneGUI()
    {
        if (!targetScript.showDebugRay) return;
        
        if (targetScript.startPoint != null && targetScript.endPoint != null)
        {
            Vector3 startPos = targetScript.startPoint.position;
            Vector3 endPos = targetScript.endPoint.position;
            Vector3 direction = (endPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPos);
            float actualDistance = Mathf.Min(distance, targetScript.maxDistance);
            
            // Vẽ ray chính
            Handles.color = targetScript.rayColorNoHit;
            Handles.DrawLine(startPos, startPos + direction * actualDistance);
            
            // Vẽ hướng vector (arrow từ start)
            Handles.color = targetScript.directionColor;
            Vector3 arrowEnd = startPos + direction * targetScript.directionArrowLength;
            Handles.DrawLine(startPos, arrowEnd);
            
            // Vẽ mũi tên cho arrow start
            Vector3 arrowTip1 = arrowEnd + (Quaternion.Euler(0, 45, 0) * -direction) * 0.5f;
            Vector3 arrowTip2 = arrowEnd + (Quaternion.Euler(0, -45, 0) * -direction) * 0.5f;
            Handles.DrawLine(arrowEnd, arrowTip1);
            Handles.DrawLine(arrowEnd, arrowTip2);
            
            // Vẽ hướng mở rộng từ start qua end và tiếp tục
            if (targetScript.showExtendedDirection)
            {
                Handles.color = targetScript.extendedDirectionColor;
                Vector3 extendedEnd = endPos + direction * targetScript.extendedLength;
                
                // Vẽ line mở rộng từ end point
                Handles.DrawLine(endPos, extendedEnd);
                
                // Vẽ mũi tên cho extended arrow
                Vector3 extendedArrowTip1 = extendedEnd + (Quaternion.Euler(0, 45, 0) * -direction) * 0.7f;
                Vector3 extendedArrowTip2 = extendedEnd + (Quaternion.Euler(0, -45, 0) * -direction) * 0.7f;
                Handles.DrawLine(extendedEnd, extendedArrowTip1);
                Handles.DrawLine(extendedEnd, extendedArrowTip2);
                
                // Vẽ điểm cuối extended
                Handles.color = Color.yellow;
                Handles.DrawWireCube(extendedEnd, Vector3.one * 0.3f);
                Handles.Label(extendedEnd + Vector3.up * 0.5f, "EXTENDED");
            }
            
            // Vẽ start và end points
            Handles.color = Color.yellow;
            Handles.DrawWireCube(startPos, Vector3.one * 0.5f);
            Handles.color = Color.magenta;
            Handles.DrawWireCube(endPos, Vector3.one * 0.5f);
            
            // Hiển thị label
            Handles.Label(startPos + Vector3.up * 0.5f, "START");
            Handles.Label(endPos + Vector3.up * 0.5f, "END");
            Handles.Label(startPos + direction * (actualDistance * 0.5f) + Vector3.up * 0.5f, 
                         $"Dist: {actualDistance:F1}m");
            
            // Vẽ direction info
            Handles.color = Color.white;
            Handles.Label(startPos + Vector3.up * 1f, $"Dir: {direction}");
        }
    }
}
