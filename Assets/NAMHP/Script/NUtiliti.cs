using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class NUtiliti : MonoBehaviour
{
    
    /// <summary>
    /// Xoay về phía target rồi chạy animation
    /// </summary>
    /// <param name="caller">Object cần xoay</param>
    /// <param name="target">Target để nhìn</param>
    /// <param name="rotationSpeed">Tốc độ xoay (độ/giây)</param>
    /// <param name="animHash">Animation parameter hash</param>
    /// <param name="ator">Animator để chạy animation</param>
    public static IEnumerator LookAtAndAnimate(Transform caller, Transform target,Animator ator , 
        float rotationSpeed, int animHash, bool isLookAt = false)
    {
        if (caller == null || target == null) yield break;
        isLookAt = false; // Reset trạng thái nhìn
        // Tính direction một lần duy nhất
        Vector3 direction = target.position - caller.position;
        direction.y = 0f; // Chỉ xoay trên mặt phẳng XZ
        
        // Kiểm tra khoảng cách
        if (direction.sqrMagnitude < 0.001f)
        {
            // Quá gần, chỉ chạy animation
            TriggerAnimation(ator, animHash);
            yield break;
        }
        
        // Tính target rotation một lần
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        
        // Xoay cho đến khi đạt góc mong muốn
        while (Quaternion.Angle(caller.rotation, targetRotation) > 1f)
        {
            caller.rotation = Quaternion.Slerp(caller.rotation, targetRotation, 
                rotationSpeed * Time.deltaTime);
            
            yield return null; // Đơn giản nhất
        }
        
        // Hoàn tất xoay
        caller.rotation = targetRotation;
        isLookAt = true; // Đánh dấu đã nhìn về target
        EventManager.Invoke<bool>(EventName.OnRotated, isLookAt);
        TriggerAnimation(ator, animHash);
    }
    
    
    /// <summary>
    /// Trigger animation
    /// </summary>
    private static void TriggerAnimation(Animator anim, int animHash)
    {
        anim.SetBool(animHash, true);
    }
    
    
    // Bộ nhớ tạm cho kiểm tra UI - static để tái sử dụng
    private static readonly PointerEventData PointerEventData = new PointerEventData(EventSystem.current);
    private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();
    
    /// <summary>
    /// Kiểm tra xem con trỏ chuột có đang hover trên UI element không
    /// </summary>
    /// <returns>True nếu pointer đang trên UI, false nếu không</returns>
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Cập nhật vị trí con trỏ hiện tại
        PointerEventData.position = Input.mousePosition;
        RaycastResults.Clear();

        // Thực hiện raycast để kiểm tra UI elements
        EventSystem.current.RaycastAll(PointerEventData, RaycastResults);
        return RaycastResults.Count > 0;
    }
    
    /// <summary>
    /// Kiểm tra pointer tại vị trí cụ thể có trên UI không
    /// </summary>
    /// <param name="screenPosition">Vị trí trên màn hình cần kiểm tra</param>
    /// <returns>True nếu có UI element tại vị trí đó</returns>
    public static bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData.position = screenPosition;
        RaycastResults.Clear();

        EventSystem.current.RaycastAll(PointerEventData, RaycastResults);
        return RaycastResults.Count > 0;
    }
    
    /// <summary>
    /// Lấy danh sách tất cả UI elements tại vị trí con trỏ
    /// </summary>
    /// <returns>List các RaycastResult</returns>
    public static List<RaycastResult> GetUIElementsUnderPointer()
    {
        var results = new List<RaycastResult>();
        
        if (EventSystem.current == null) return results;

        PointerEventData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(PointerEventData, results);
        
        return results;
    }
    
    
    
    /// <summary>
    /// Kiểm tra khoảng cách giữa hai vị trí Vector3.
    /// Sử dụng sqrMagnitude để tối ưu hiệu suất (tránh tính căn bậc hai).
    /// </summary>
    /// <param name="pointA">Vị trí điểm A.</param>
    /// <param name="pointB">Vị trí điểm B.</param>
    /// <param name="thresholdDistance">Khoảng cách ngưỡng.</param>
    /// <returns>True nếu khoảng cách nhỏ hơn hoặc bằng ngưỡng, ngược lại là False.</returns>
    public static bool IsWithinDistance(Vector3 pointA, Vector3 pointB, float thresholdDistance)
    {
        // So sánh bình phương khoảng cách để tránh tính căn bậc hai (tốn kém)
        return (pointB - pointA).sqrMagnitude <= thresholdDistance * thresholdDistance;
    }

    /// <summary>
    /// Kiểm tra khoảng cách giữa hai Transform.
    /// </summary>
    public static bool IsCheckDistance(Transform transformA, Transform transformB, float thresholdDistance)
    {
        if (transformA == null || transformB == null)
        {
            Debug.LogWarning("DistanceChecker: Một hoặc cả hai Transform là null.");
            return false;
        }
        return IsWithinDistance(transformA.position, transformB.position, thresholdDistance);
    }
    /// <summary>
    /// Kiểm tra khoảng cách giữa hai vị trí Vector3 và thực hiện một hành động nếu trong ngưỡng.
    /// </summary>
    /// <param name="pointA">Vị trí điểm A.</param>
    /// <param name="pointB">Vị trí điểm B.</param>
    /// <param name="thresholdDistance">Khoảng cách ngưỡng.</param>
    /// <param name="onWithinDistance">Hành động (Action) sẽ được gọi nếu trong ngưỡng. Có thể là null.</param>
    /// <returns>True nếu khoảng cách nhỏ hơn hoặc bằng ngưỡng, ngược lại là False.</returns>
    public static bool CheckAndCallActionInDistance(Vector3 pointA, Vector3 pointB, float thresholdDistance, Action onWithinDistance)
    {
        if (IsWithinDistance(pointA, pointB, thresholdDistance))
        {
            onWithinDistance?.Invoke(); // Gọi hành động nếu nó không null
            return true;
        }
        return false;
    }
    /// <summary>
    /// Kiểm tra khoảng cách giữa hai Transform và thực hiện một hành động nếu trong ngưỡng.
    /// </summary>
    public static bool CheckAndActIfWithinDistance(Transform transformA, Transform transformB, float thresholdDistance, Action onWithinDistance)
    {
        if (transformA == null || transformB == null)
        {
            Debug.LogWarning("DistanceChecker: Một hoặc cả hai Transform là null.");
            return false;
        }
        return CheckAndCallActionInDistance(transformA.position, transformB.position, thresholdDistance, onWithinDistance);
    }
    public static void AlignCamera(Transform healthBarTransform, Transform mainCameraTranform)
    {
    
        if (Time.frameCount % UpdateSetting.interval == 0
            && mainCameraTranform != null
            && healthBarTransform != null)
        {
            var forward = healthBarTransform.transform.position - mainCameraTranform.position; // huong tu thanh mau den camera
            forward.Normalize();
            var up = Vector3.Cross(forward, mainCameraTranform.right);
            // phép tích có hướng (cross product) để tính toán vector "up" vuông góc với hướng forward và hướng "bên phải" (right) của camera.
            healthBarTransform.transform.rotation = Quaternion.LookRotation(forward, up);
            // xoay thanh mau theo 2 vector
        }
    }
    public static class UpdateSetting
    {
        public const int interval = 3;

        public const int fps = 30;
    }
    
    
    
}
