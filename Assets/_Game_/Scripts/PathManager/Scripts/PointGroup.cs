using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Linq;
using static GameConstants;

public class PointGroup : MonoBehaviour
{
    // [HideInInspector] để Editor script có thể kiểm soát hoàn toàn việc hiển thị.
    //[HideInInspector]
    public List<Transform> points = new List<Transform>();
    //[HideInInspector]
    public List<Transform> attackPoints = new List<Transform>();
    public List<Transform> pointsSpecial = new List<Transform>();
    
    [Header("Aircraft Flight Pattern")]
    [Tooltip("Điểm bay về phía trái sau khi tấn công - dùng cho aircraft")]
    public List<Transform> leftPoints = new List<Transform>();
    
    [Tooltip("Điểm bay về phía phải sau khi tấn công - dùng cho aircraft")]
    public List<Transform> rightPoints = new List<Transform>();
    
    // Runtime optimization cache
    private int cachedChildCount = -1;
    private bool pointsNeedUpdate = true;
    [FormerlySerializedAs("botType")]
    [Header("Route Classification")]
    [Tooltip("Tuyến đường này dành cho loại bot nào?")]
    public BotMoveType botMoveType = BotMoveType.Infantry;
    public bool isBeingUsed = false;
    [Tooltip("Tên cơ sở để tự động đánh số. Ví dụ: 'Waypoint', 'CheckPoint', 'SpawnPoint'.")]
    public string baseName = "Point";
    [Header("Attack Points Settings")]
    [Tooltip("Tên cơ sở cho attack points")]
    public string attackPointBaseName = "AttackPoint";

    [Header("Point Child GetMove ")][Tooltip("Để spawn bot con có thể lấy được điểm này di chuyển, hãy kéo nó vào đây.")]
    [SerializeField] List<PointGroup> pointChindCanMove = new List<PointGroup>();
    public List<PointGroup> PointChindCanMove => pointChindCanMove;
    
    [Header("Gizmos Settings")]
    [Tooltip("Màu sắc cho attack points")]
    public Color attackPointColor = Color.red;
    public Color lineColor = Color.magenta;
    public float pointRadius = 0.4f;
    [Tooltip("Hiển thị số thứ tự của điểm và Gizmos trong Scene View.")]
    public bool showDebug = true;
    
    /// <summary>
    /// Khởi tạo danh sách points khi game bắt đầu
    /// </summary>
    // private void Awake()
    // {
    //     // Chỉ update một lần khi bắt đầu runtime
    //     if (Application.isPlaying)
    //     {
    //         UpdatePoints();
    //     }
    // }
    //
    /// <summary>
    /// Cập nhật danh sách points và attackPoints (chỉ dùng trong editor và khởi tạo runtime)
    /// </summary>
    public void UpdatePoints()
    {
        // Optimize: Chỉ update khi cần thiết
        int currentChildCount = transform.childCount;
        if (!pointsNeedUpdate && cachedChildCount == currentChildCount && Application.isPlaying)
        {
            return; // Không cần update
        }
        
        points.Clear();
        attackPoints.Clear();
        leftPoints.Clear();
        rightPoints.Clear();
        
        foreach (Transform child in transform)
        {
            if (child != null && child.gameObject.activeSelf)
            {
                if (IsAttackPoint(child))
                {
                    attackPoints.Add(child);
                }
                else if (IsLeftPoint(child))
                {
                    leftPoints.Add(child);
                }
                else if (IsRightPoint(child))
                {
                    rightPoints.Add(child);
                }
                else
                {
                    points.Add(child);
                }
            }
        }
        
        // Cache để optimize lần sau
        cachedChildCount = currentChildCount;
        pointsNeedUpdate = false;
    }
    
    /// <summary>
    /// Kiểm tra xem GameObject con có phải là attack point không dựa trên tên (optimized)
    /// </summary>
    /// <param name="child">Transform con cần kiểm tra</param>
    /// <returns>True nếu là attack point, false nếu là waypoint thông thường</returns>
    private bool IsAttackPoint(Transform child)
    {
        if (string.IsNullOrEmpty(child.name)) return false;
        
        // Optimize: Cache ToLower() result
        string lowerName = child.name.ToLower();
        return lowerName.Contains("attack") || 
               lowerName.Contains("fire") ||
               lowerName.Contains("combat");
    }
    
    /// <summary>
    /// Kiểm tra xem GameObject con có phải là left point không dựa trên tên
    /// </summary>
    private bool IsLeftPoint(Transform child)
    {
        if (string.IsNullOrEmpty(child.name)) return false;
        
        string lowerName = child.name.ToLower();
        return lowerName.Contains("left") || lowerName.Contains("l_");
    }
    
    /// <summary>
    /// Kiểm tra xem GameObject con có phải là right point không dựa trên tên
    /// </summary>
    private bool IsRightPoint(Transform child)
    {
        if (string.IsNullOrEmpty(child.name)) return false;
        
        string lowerName = child.name.ToLower();
        return lowerName.Contains("right") || lowerName.Contains("r_");
    }
    
    /// <summary>
    /// Lấy attack point theo chỉ số thứ tự
    /// </summary>
    /// <param name="index">Chỉ số thứ tự (bắt đầu từ 0)</param>
    /// <returns>Transform của attack point, hoặc null nếu index không hợp lệ</returns>
    public Transform GetAttackPoint(int index)
    {
        if (index >= 0 && index < attackPoints.Count)
            return attackPoints[index];
        return null;
    }
    
    /// <summary>
    /// Lấy một attack point ngẫu nhiên từ danh sách
    /// </summary>
    /// <returns>Transform của attack point ngẫu nhiên, hoặc null nếu không có attack point nào</returns>
    public Transform GetRandomAttackPoint()
    {
        if (attackPoints.Count == 0) return null;
        return attackPoints[Random.Range(0, attackPoints.Count)];
    }
    
    #region Aircraft Flight Pattern Methods
    
    /// <summary>
    /// Lấy left point theo chỉ số thứ tự
    /// </summary>
    public Transform GetLeftPoint(int index)
    {
        if (index >= 0 && index < leftPoints.Count)
            return leftPoints[index];
        return null;
    }
    
    /// <summary>
    /// Lấy right point theo chỉ số thứ tự
    /// </summary>
    public Transform GetRightPoint(int index)
    {
        if (index >= 0 && index < rightPoints.Count)
            return rightPoints[index];
        return null;
    }
    
    /// <summary>
    /// Lấy một left point ngẫu nhiên
    /// </summary>
    public Transform GetRandomLeftPoint()
    {
        if (leftPoints.Count == 0) return null;
        return leftPoints[Random.Range(0, leftPoints.Count)];
    }
    
    /// <summary>
    /// Lấy một right point ngẫu nhiên
    /// </summary>
    public Transform GetRandomRightPoint()
    {
        if (rightPoints.Count == 0) return null;
        return rightPoints[Random.Range(0, rightPoints.Count)];
    }
    
    /// <summary>
    /// Lấy tất cả left points dưới dạng mảng Vector3 cho DOTween
    /// </summary>
    public Vector3[] GetLeftPath()
    {
        return leftPoints.Where(p => p != null).Select(p => p.position).ToArray();
    }
    
    /// <summary>
    /// Lấy tất cả right points dưới dạng mảng Vector3 cho DOTween
    /// </summary>
    public Vector3[] GetRightPath()
    {
        return rightPoints.Where(p => p != null).Select(p => p.position).ToArray();
    }
    
    #endregion
    
    #region Waypoint Methods for Bot Runtime
    
    /// <summary>
    /// Lấy waypoint theo chỉ số thứ tự
    /// </summary>
    /// <param name="index">Chỉ số thứ tự (bắt đầu từ 0)</param>
    /// <returns>Transform của waypoint, hoặc null nếu index không hợp lệ</returns>
    public Transform GetWaypoint(int index)
    {
        if (index >= 0 && index < points.Count)
            return points[index];
        return null;
    }
    
    /// <summary>
    /// Lấy một waypoint ngẫu nhiên từ danh sách
    /// </summary>
    /// <returns>Transform của waypoint ngẫu nhiên, hoặc null nếu không có waypoint nào</returns>
    public Transform GetRandomWaypoint()
    {
        if (points.Count == 0) return null;
        return points[Random.Range(0, points.Count)];
    }
    
    /// <summary>
    /// Tìm waypoint gần nhất với vị trí được chỉ định (optimized cho bot runtime)
    /// </summary>
    /// <param name="position">Vị trí tham chiếu để tìm waypoint gần nhất</param>
    /// <returns>Transform của waypoint gần nhất, hoặc null nếu không có waypoint nào</returns>
    public Transform GetNearestWaypoint(Vector3 position)
    {
        if (points.Count == 0) return null;
        
        Transform nearestPoint = null;
        float nearestSqrDistance = Mathf.Infinity;
        
        foreach (Transform point in points)
        {
            if (point != null)
            {
                float sqrDistance = (position - point.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestPoint = point;
                }
            }
        }
        
        return nearestPoint;
    }
    
    #endregion
    
    /// <summary>
    /// Tìm attack point gần nhất với vị trí được chỉ định (optimized cho bot runtime)
    /// </summary>
    /// <param name="position">Vị trí tham chiếu để tìm attack point gần nhất</param>
    /// <returns>Transform của attack point gần nhất, hoặc null nếu không có attack point nào</returns>
    public Transform GetNearestAttackPoint(Vector3 position)
    {
        if (attackPoints.Count == 0) return null;
        
        Transform nearestPoint = null;
        float nearestSqrDistance = Mathf.Infinity;
        
        foreach (Transform point in attackPoints)
        {
            if (point != null)
            {
                // Optimize: Sử dụng sqrMagnitude thay vì Distance() (nhanh hơn 3x)
                float sqrDistance = (position - point.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestPoint = point;
                }
            }
        }
        
        return nearestPoint;
    }
    
    // OnDrawGizmos được gọi mỗi khi Scene View được vẽ lại.
    private void OnDrawGizmos()
    {
        if (points == null || !showDebug) return;
        // Tự động cập nhật nếu danh sách trống khi vào Play Mode hoặc lần đầu
        if (Application.isPlaying && points.Count != transform.childCount)
        {
             UpdatePoints();
        }

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] != null)
            {
                // Vẽ Gizmos như cũ
                Gizmos.color = lineColor;
                Gizmos.DrawWireSphere(points[i].position, pointRadius);
                if (i < points.Count - 1 && points[i+1] != null)
                {
                    Gizmos.DrawLine(points[i].position, points[i+1].position);
                }

                // --- PHẦN MỚI: HIỂN THỊ LABEL TRONG SCENE ---
                #if UNITY_EDITOR // Đảm bảo phần này chỉ chạy trong Editor
                if (showDebug)
                {
                    // Tạo một style cho chữ
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = lineColor;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;

                    // Tạo nội dung label là số thứ tự của điểm
                    string labelText = (i+1).ToString();

                    // Hiển thị label phía trên điểm một chút
                    UnityEditor.Handles.Label(points[i].position + Vector3.up * pointRadius * 2, labelText, style);
                }
                #endif
            }
        }
        
        // NEW: Draw attack points
        Gizmos.color = attackPointColor;
        for (int i = 0; i < attackPoints.Count; i++)
        {
            if (attackPoints[i] != null)
            {
                // Vẽ attack point (lớn hơn waypoint một chút)
                Gizmos.DrawWireSphere(attackPoints[i].position, pointRadius * 1.2f);
                
                // Vẽ đường nối attack points
                if (i < attackPoints.Count - 1 && attackPoints[i+1] != null)
                {
                    Gizmos.color = attackPointColor * 0.7f;
                    Gizmos.DrawLine(attackPoints[i].position, attackPoints[i+1].position);
                    Gizmos.color = attackPointColor;
                }

                #if UNITY_EDITOR
                if (showDebug)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = attackPointColor;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;

                    string labelText = $"A{(i+1)}";
                    UnityEditor.Handles.Label(attackPoints[i].position + Vector3.up * pointRadius * 3, labelText, style);
                }
                #endif
            }
        }
        
        // NEW: Draw left points - Blue
        Gizmos.color = Color.blue;
        for (int i = 0; i < leftPoints.Count; i++)
        {
            if (leftPoints[i] != null)
            {
                Gizmos.DrawWireSphere(leftPoints[i].position, pointRadius * 1.3f);
                
                // Vẽ đường nối left points
                if (i < leftPoints.Count - 1 && leftPoints[i+1] != null)
                {
                    Gizmos.color = Color.blue * 0.8f;
                    Gizmos.DrawLine(leftPoints[i].position, leftPoints[i+1].position);
                    Gizmos.color = Color.blue;
                }

                #if UNITY_EDITOR
                if (showDebug)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.blue;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;

                    string labelText = $"L{(i+1)}";
                    UnityEditor.Handles.Label(leftPoints[i].position + Vector3.up * pointRadius * 4, labelText, style);
                }
                #endif
            }
        }
        
        // NEW: Draw right points - Green
        Gizmos.color = Color.green;
        for (int i = 0; i < rightPoints.Count; i++)
        {
            if (rightPoints[i] != null)
            {
                Gizmos.DrawWireSphere(rightPoints[i].position, pointRadius * 1.3f);
                
                // Vẽ đường nối right points
                if (i < rightPoints.Count - 1 && rightPoints[i+1] != null)
                {
                    Gizmos.color = Color.green * 0.8f;
                    Gizmos.DrawLine(rightPoints[i].position, rightPoints[i+1].position);
                    Gizmos.color = Color.green;
                }

                #if UNITY_EDITOR
                if (showDebug)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.green;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;

                    string labelText = $"R{(i+1)}";
                    UnityEditor.Handles.Label(rightPoints[i].position + Vector3.up * pointRadius * 4, labelText, style);
                }
                #endif
            }
        }
    }
}