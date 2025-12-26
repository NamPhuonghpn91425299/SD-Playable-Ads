using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Serialization;

// Component này điều khiển việc di chuyển của một bot theo một tuyến đường (PointGroup) cho trước.
public class BotPathMovement : MonoBehaviour
{
    [SerializeField] private BotIdentity botIdentity;
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của bot.")]
    [SerializeField] private float moveSpeed = 5.0f;
    [Tooltip("Tốc độ xoay của bot khi đổi hướng.")]
    [SerializeField] private float rotationSpeed = 10.0f;

    [Header("Pathing Info (Read-Only)")]
    [Tooltip("Tuyến đường mà bot này đang đi theo.")]
    [SerializeField] private PointGroup assignedPath; // Để debug trong Inspector
    [SerializeField] private int currentPointIndex = 0; // Điểm tiếp theo cần đến

    [SerializeField] private bool isMoving = false;


    private void Start()
    {
        SetPath(botIdentity.AssignedPath);
    }

    // Biến trạng thái

    /// <summary>
    /// Đây là hàm chính để một script khác (như Spawner) gán tuyến đường cho con bot này.
    /// </summary>
    public void SetPath(PointGroup path)
    {
        // 1. Gán tuyến đường
        assignedPath = path;
        
        // 2. Reset lại chỉ số điểm và bắt đầu di chuyển
        currentPointIndex = 0;
        
        // 3. Đảm bảo bot bắt đầu ở đúng vị trí và hướng
        if (assignedPath != null && assignedPath.points.Count > 0)
        {
            // Di chuyển bot đến điểm đầu tiên ngay lập tức
            transform.position = assignedPath.points[0].position; 
            
            // Nếu có điểm thứ hai, xoay bot để hướng về phía đó
            if (assignedPath.points.Count > 1)
            {
                transform.LookAt(assignedPath.points[1]);
            }
            
            // 4. Kích hoạt trạng thái di chuyển
            isMoving = true;
        }
        else
        {
            Debug.LogError($"Bot '{gameObject.name}' được gán một tuyến đường không hợp lệ hoặc không có điểm.");
            isMoving = false;
        }
    }
    
    private void Update()
    {
        // Nếu không ở trạng thái di chuyển, hoặc không có đường đi, thì không làm gì cả.
        if (!isMoving || assignedPath == null || assignedPath.points.Count == 0)
        {
            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        // Kiểm tra xem đã đi hết tất cả các điểm chưa
        if (currentPointIndex >= assignedPath.points.Count)
        {
            // Đã đến điểm cuối cùng
            if (isMoving) // Chỉ chạy một lần
            {
                Debug.Log($"Bot '{gameObject.name}' đã hoàn thành tuyến đường.");
                isMoving = false; // Dừng di chuyển
                // Tại đây, bạn có thể gọi một sự kiện hoặc hàm khác, ví dụ: bot bắt đầu tấn công, hoặc tự hủy.
                // OnPathCompleted();
            }
            return;
        }

        // 1. Xác định mục tiêu tiếp theo
        Transform targetPoint = assignedPath.points[currentPointIndex];

        // 2. Di chuyển về phía mục tiêu
        // Vector3.MoveTowards giúp di chuyển đều với một tốc độ không đổi.
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        // 3. Xoay người về phía mục tiêu một cách mượt mà
        // Tạo vector hướng tới mục tiêu
        Vector3 directionToTarget = targetPoint.position - transform.position;
        // Kiểm tra để tránh lỗi khi ở quá gần mục tiêu
        if (directionToTarget != Vector3.zero)
        {
            // Tạo góc xoay mục tiêu
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            // Dùng Slerp để xoay từ từ và mượt mà
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. Kiểm tra xem đã đến gần mục tiêu chưa để chuyển sang điểm tiếp theo
        // Dùng khoảng cách bình phương (sqrMagnitude) nhanh hơn là Vector3.Distance
        if (Vector3.SqrMagnitude(transform.position - targetPoint.position) < 0.1f)
        {
            currentPointIndex++; // Tăng chỉ số để nhắm đến điểm tiếp theo trong frame sau
        }
        return;
    }
    
   
}