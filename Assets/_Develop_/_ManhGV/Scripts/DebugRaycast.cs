using UnityEngine;

public class DebugRaycast : MonoBehaviour
{
    [Header("Raycast Points")]
    [Tooltip("Điểm bắt đầu raycast")]
    public Transform startPoint;
    
    [Tooltip("Điểm kết thúc raycast")]
    public Transform endPoint;
    
    [Header("Raycast Settings")]
    [Tooltip("Layer để raycast")]
    public LayerMask raycastLayer = -1;
    
    [Tooltip("Khoảng cách raycast tối đa")]
    public float maxDistance = 100f;
    
    [Header("Visual Debug")]
    [Tooltip("Hiển thị debug trong Scene view")]
    public bool showDebugRay = true;
    
    [Tooltip("Màu của ray khi không hit")]
    public Color rayColorNoHit = Color.red;
    
    [Tooltip("Màu của ray khi có hit")]
    public Color rayColorHit = Color.green;
    
    [Tooltip("Màu của hướng vector từ start")]
    public Color directionColor = Color.blue;
    
    [Tooltip("Độ dài arrow hướng từ start")]
    public float directionArrowLength = 2f;
    
    [Header("Extended Direction Arrow")]
    [Tooltip("Hiển thị arrow hướng mở rộng từ start qua end point")]
    public bool showExtendedDirection = true;
    
    [Tooltip("Màu của arrow hướng mở rộng")]
    public Color extendedDirectionColor = Color.cyan;
    
    [Tooltip("Độ dài mở rộng từ end point (độ dài tăng thì càng dài)")]
    public float extendedLength = 5f;
    
    [Header("Controls")]
    [Tooltip("Phím để thực hiện raycast")]
    public KeyCode raycastKey = KeyCode.Space;
    
    [Tooltip("Tự động raycast mỗi frame")]
    public bool autoRaycast = false;
    
    // Debug info
    private Vector3 lastDirection;
    private bool lastHit = false;
    private Vector3 lastHitPoint;
    private float lastDistance;
    private string lastHitObjectName = "";
    
    void Update()
    {
        if (Input.GetKeyDown(raycastKey) || autoRaycast)
        {
            PerformRaycast();
        }
    }
    
    /// <summary>
    /// Thực hiện raycast giữa 2 điểm
    /// </summary>
    public void PerformRaycast()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning("[DebugRaycast] Start point hoặc End point chưa được gán!");
            return;
        }
        
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        
        // Tính hướng và khoảng cách
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        
        // Giới hạn distance
        float actualDistance = Mathf.Min(distance, maxDistance);
        
        // Thực hiện raycast
        RaycastHit hit;
        bool hasHit = Physics.Raycast(startPos, direction, out hit, actualDistance, raycastLayer);
        
        // Lưu thông tin debug
        lastDirection = direction;
        lastHit = hasHit;
        lastDistance = actualDistance;
        
        if (hasHit)
        {
            lastHitPoint = hit.point;
            lastHitObjectName = hit.collider.gameObject.name;
            
            Debug.Log($"[DebugRaycast] HIT!" +
                     $"\n- Object: {lastHitObjectName}" +
                     $"\n- Hit Point: {lastHitPoint}" +
                     $"\n- Distance: {hit.distance:F2}m" +
                     $"\n- Direction: {lastDirection}" +
                     $"\n- Normal: {hit.normal}");
        }
        else
        {
            lastHitPoint = startPos + direction * actualDistance;
            lastHitObjectName = "";
            
            Debug.Log($"[DebugRaycast] NO HIT" +
                     $"\n- Direction: {lastDirection}" +
                     $"\n- Max Distance: {actualDistance:F2}m" +
                     $"\n- End Point: {lastHitPoint}");
        }
        
        // In thông tin hướng chi tiết
        DebugDirection();
    }
    
    /// <summary>
    /// Debug thông tin hướng chi tiết
    /// </summary>
    void DebugDirection()
    {
        if (startPoint == null || endPoint == null) return;
        
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        Vector3 direction = lastDirection;
        
        Debug.Log($"[DebugRaycast] DIRECTION INFO:" +
                 $"\n- Start: {startPos}" +
                 $"\n- End: {endPos}" +
                 $"\n- Direction Vector: {direction}" +
                 $"\n- Direction (normalized): {direction.normalized}" +
                 $"\n- Magnitude: {direction.magnitude}" +
                 $"\n- Angle (Y-axis): {Vector3.Angle(direction, Vector3.up):F1}°" +
                 $"\n- Angle (Forward): {Vector3.Angle(direction, Vector3.forward):F1}°" +
                 $"\n- Euler Angles: {Quaternion.LookRotation(direction).eulerAngles}");
    }
    
    /// <summary>
    /// Raycast từ vị trí hiện tại của script tới một điểm
    /// </summary>
    public void RaycastToPoint(Vector3 targetPoint)
    {
        Vector3 startPos = transform.position;
        Vector3 direction = (targetPoint - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPoint);
        
        RaycastHit hit;
        bool hasHit = Physics.Raycast(startPos, direction, out hit, distance, raycastLayer);
        
        if (hasHit)
        {
            Debug.Log($"[DebugRaycast] RaycastToPoint HIT: {hit.collider.name} at {hit.point}");
        }
        else
        {
            Debug.Log($"[DebugRaycast] RaycastToPoint NO HIT to {targetPoint}");
        }
    }
    
    /// <summary>
    /// Raycast theo hướng cụ thể
    /// </summary>
    public void RaycastInDirection(Vector3 direction, float distance = 10f)
    {
        Vector3 startPos = transform.position;
        direction = direction.normalized;
        
        RaycastHit hit;
        bool hasHit = Physics.Raycast(startPos, direction, out hit, distance, raycastLayer);
        
        if (hasHit)
        {
            Debug.Log($"[DebugRaycast] RaycastInDirection HIT: {hit.collider.name} at {hit.point}");
        }
        else
        {
            Debug.Log($"[DebugRaycast] RaycastInDirection NO HIT in direction {direction}");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugRay) return;
        
        if (startPoint != null && endPoint != null)
        {
            Vector3 startPos = startPoint.position;
            Vector3 endPos = endPoint.position;
            Vector3 direction = (endPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPos);
            float actualDistance = Mathf.Min(distance, maxDistance);
            
            // Vẽ ray chính
            Gizmos.color = lastHit ? rayColorHit : rayColorNoHit;
            
            if (Application.isPlaying && lastHit)
            {
                // Vẽ đến điểm hit
                Gizmos.DrawLine(startPos, lastHitPoint);
                Gizmos.DrawWireSphere(lastHitPoint, 0.2f);
            }
            else
            {
                // Vẽ toàn bộ ray
                Gizmos.DrawLine(startPos, startPos + direction * actualDistance);
            }
            
            // Vẽ hướng vector (arrow từ start)
            Gizmos.color = directionColor;
            Vector3 arrowEnd = startPos + direction * directionArrowLength;
            Gizmos.DrawLine(startPos, arrowEnd);
            
            // Vẽ mũi tên cho arrow start
            Vector3 arrowTip1 = arrowEnd + (Quaternion.Euler(0, 45, 0) * -direction) * 0.5f;
            Vector3 arrowTip2 = arrowEnd + (Quaternion.Euler(0, -45, 0) * -direction) * 0.5f;
            Gizmos.DrawLine(arrowEnd, arrowTip1);
            Gizmos.DrawLine(arrowEnd, arrowTip2);
            
            // Vẽ hướng mở rộng từ start qua end và tiếp tục
            if (showExtendedDirection)
            {
                Gizmos.color = extendedDirectionColor;
                Vector3 extendedEnd = endPos + direction * extendedLength;
                
                // Vẽ line mở rộng từ end point
                Gizmos.DrawLine(endPos, extendedEnd);
                
                // Vẽ mũi tên cho extended arrow
                Vector3 extendedArrowTip1 = extendedEnd + (Quaternion.Euler(0, 45, 0) * -direction) * 0.7f;
                Vector3 extendedArrowTip2 = extendedEnd + (Quaternion.Euler(0, -45, 0) * -direction) * 0.7f;
                Gizmos.DrawLine(extendedEnd, extendedArrowTip1);
                Gizmos.DrawLine(extendedEnd, extendedArrowTip2);
                
                // Vẽ đường nối giữa các đoạn (nếu cần)
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(extendedEnd, 0.15f);
            }
            
            // Vẽ start và end points
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPos, 0.3f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(endPos, 0.3f);
            
            // Hiển thị label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(startPos + Vector3.up * 0.5f, "START");
            UnityEditor.Handles.Label(endPos + Vector3.up * 0.5f, "END");
            
            if (Application.isPlaying && lastHit)
            {
                UnityEditor.Handles.Label(lastHitPoint + Vector3.up * 0.5f, $"HIT: {lastHitObjectName}");
            }
            #endif
        }
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== DEBUG RAYCAST ===");
        
        if (startPoint != null && endPoint != null)
        {
            GUILayout.Label($"Start: {startPoint.position}");
            GUILayout.Label($"End: {endPoint.position}");
            GUILayout.Label($"Direction: {lastDirection}");
            GUILayout.Label($"Distance: {lastDistance:F2}m");
            GUILayout.Label($"Hit: {(lastHit ? "YES" : "NO")}");
            
            if (lastHit)
            {
                GUILayout.Label($"Hit Object: {lastHitObjectName}");
                GUILayout.Label($"Hit Point: {lastHitPoint}");
            }
            
            if (GUILayout.Button("Perform Raycast"))
            {
                PerformRaycast();
            }
        }
        else
        {
            GUILayout.Label("Assign Start Point và End Point!");
        }
        
        GUILayout.EndArea();
    }
}
