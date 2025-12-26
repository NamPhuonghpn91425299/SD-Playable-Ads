using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NUtiliti :MonoBehaviour
{
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

    public static void AlignCameraYOnly(Transform targetTransform, Transform mainCameraTransform)
    {
        if (Time.frameCount % UpdateSetting.interval != 0 || mainCameraTransform == null || targetTransform == null)
            return;

        // Lấy hướng từ target đến camera trên mặt phẳng XZ
        Vector3 directionToCamera = mainCameraTransform.position - targetTransform.position;
        directionToCamera.y = 0; // Bỏ thành phần Y để tránh nghiêng

        if (directionToCamera.sqrMagnitude > 0.0001f)
        {
            // Tính góc cần xoay quanh trục Y sao cho trục Z hướng về camera
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

            // Giữ nguyên X và Z rotation, chỉ thay đổi Y
            Vector3 currentEuler = targetTransform.eulerAngles;
            float newY = targetRotation.eulerAngles.y;
            targetTransform.rotation = Quaternion.Euler(currentEuler.x, newY, currentEuler.z);
        }
    }

    public static class UpdateSetting
    {
        public const int interval = 3;

        public const int fps = 30;
    }
}
