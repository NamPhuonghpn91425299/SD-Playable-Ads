using UnityEngine;
using System.Collections.Generic;

namespace GameUtilities
{
    /// <summary>
    /// Lớp tiện ích cho việc di chuyển dựa trên các điểm waypoint có thể được sử dụng lại cho các loại bot khác nhau.
    /// </summary>
    public static class WaypointMovementUtility
    {
        /// <summary>
        /// Di chuyển một transform về phía một vị trí mục tiêu.
        /// </summary>
        /// <param name="transform">Transform cần di chuyển</param>
        /// <param name="targetPosition">Vị trí mục tiêu để di chuyển đến</param>
        /// <param name="speed">Tốc độ di chuyển</param>
        /// <returns>True nếu transform đã đến vị trí mục tiêu, false nếu ngược lại</returns>
        public static bool MoveTowards(Transform transform, Vector3 targetPosition, float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            return Vector3.Distance(transform.position, targetPosition) < 0.1f;
        }

        /// <summary>
        /// Xoay một transform để nhìn về phía một vị trí mục tiêu với xoay mượt.
        /// </summary>
        /// <param name="transform">Transform cần xoay</param>
        /// <param name="targetPosition">Vị trí mục tiêu để nhìn về</param>
        /// <param name="rotationSpeed">Tốc độ xoay</param>
        public static void RotateTowards(Transform transform, Vector3 targetPosition, float rotationSpeed)
        {
            Vector3 targetDirection = targetPosition - transform.position;
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Lấy vector hướng từ vị trí hiện tại đến vị trí mục tiêu.
        /// </summary>
        /// <param name="currentPosition">Vị trí hiện tại</param>
        /// <param name="targetPosition">Vị trí mục tiêu</param>
        /// <returns>Vector hướng</returns>
        public static Vector3 GetDirection(Vector3 currentPosition, Vector3 targetPosition)
        {
            return (targetPosition - currentPosition).normalized;
        }

        /// <summary>
        /// Kiểm tra xem một transform đã đến vị trí mục tiêu trong khoảng cách nhất định chưa.
        /// </summary>
        /// <param name="transform">Transform cần kiểm tra</param>
        /// <param name="targetPosition">Vị trí mục tiêu</param>
        /// <param name="threshold">Ngưỡng khoảng cách</param>
        /// <returns>True nếu đã đến, false nếu ngược lại</returns>
        public static bool HasReached(Transform transform, Vector3 targetPosition, float threshold = 0.1f)
        {
            return Vector3.Distance(transform.position, targetPosition) < threshold;
        }

        /// <summary>
        /// Lấy chỉ số điểm waypoint tiếp theo trong đường đi.
        /// </summary>
        /// <param name="currentIndex">Chỉ số điểm waypoint hiện tại</param>
        /// <param name="pathPoints">Danh sách các điểm đường đi</param>
        /// <returns>Chỉ số điểm waypoint tiếp theo</returns>
        public static int GetNextWaypointIndex(int currentIndex, List<Transform> pathPoints)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                return currentIndex;

            int nextIndex = currentIndex + 1;
            if (nextIndex >= pathPoints.Count)
                return currentIndex; // Ở lại điểm cuối cùng

            return nextIndex;
        }

        /// <summary>
        /// Lấy chỉ số điểm waypoint tiếp theo trong đường đi với chế độ loop (vòng lặp).
        /// Khi đến điểm cuối cùng, sẽ quay lại điểm đầu tiên.
        /// </summary>
        /// <param name="currentIndex">Chỉ số điểm waypoint hiện tại</param>
        /// <param name="pathPoints">Danh sách các điểm đường đi</param>
        /// <returns>Chỉ số điểm waypoint tiếp theo (sẽ quay lại 0 nếu đã đến điểm cuối)</returns>
        public static int GetNextWaypointIndexLoop(int currentIndex, List<Transform> pathPoints)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                return currentIndex;

            int nextIndex = currentIndex + 1;
            if (nextIndex >= pathPoints.Count)
                return 0; // Quay lại điểm đầu tiên khi đến điểm cuối

            return nextIndex;
        }

        /// <summary>
        /// Kiểm tra xem điểm waypoint hiện tại có phải là điểm cuối cùng trong đường đi không.
        /// </summary>
        /// <param name="currentIndex">Chỉ số điểm waypoint hiện tại</param>
        /// <param name="pathPoints">Danh sách các điểm đường đi</param>
        /// <returns>True nếu là điểm waypoint cuối cùng, false nếu ngược lại</returns>
        public static bool IsLastWaypoint(int currentIndex, List<Transform> pathPoints)
        {
            if (pathPoints == null || pathPoints.Count == 0)
                return true;

            return currentIndex >= pathPoints.Count - 1;
        }
    }
}
