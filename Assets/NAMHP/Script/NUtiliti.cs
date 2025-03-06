using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NUtiliti : MonoBehaviour
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
    public static class UpdateSetting
    {
        public const int interval = 3;

        public const int fps = 30;
    }
}
