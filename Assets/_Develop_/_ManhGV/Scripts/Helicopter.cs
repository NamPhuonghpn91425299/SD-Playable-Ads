using System.Collections.Generic;
using UnityEngine;

public class HelicopterPathFollower : MonoBehaviour
{
    [Header("Path Points (List of Waypoints)")]
    public List<Transform> pathPoints;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Tilt Settings")]
    public float maxPitch = 30f;
    public float maxRoll = 30f;

    private int currentIndex = 0;
    private float t = 0f;

    void Update()
    {
        if (pathPoints == null || pathPoints.Count < 4)
            return;

        if (currentIndex + 3 >= pathPoints.Count)
        {
            currentIndex = 0;
            transform.position = pathPoints[currentIndex].position;
            return;
        }

        // Lấy 4 điểm spline
        Vector3 p0 = pathPoints[currentIndex].position;
        Vector3 p1 = pathPoints[currentIndex + 1].position;
        Vector3 p2 = pathPoints[currentIndex + 2].position;
        Vector3 p3 = pathPoints[currentIndex + 3].position;

        // Di chuyển theo spline
        t += Time.deltaTime * speed * 0.1f;
        Vector3 targetPos = CatmullRom(t, p0, p1, p2, p3);
        transform.position = targetPos;

        // Hướng di chuyển
        Vector3 direction = CatmullRom(t + 0.01f, p0, p1, p2, p3) - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = GetHelicopterRotation(transform.forward, direction.normalized, maxPitch, maxRoll);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // Chuyển đoạn spline
        if (t >= 1f)
        {
            currentIndex++;
            t = 0f;
        }
    }

    /// <summary>
    /// Tính toán góc quay tự nhiên như trực thăng (nghiêng XZ, xoay Y)
    /// </summary>
    Quaternion GetHelicopterRotation(Vector3 currentForward, Vector3 moveDirection, float maxPitch = 30f, float maxRoll = 30f)
    {
        Vector3 dir = moveDirection.normalized;

        // Yaw
        Quaternion targetYaw = Quaternion.LookRotation(dir);

        // Pitch (nghiêng lên xuống theo Y)
        float pitch = -dir.y * maxPitch;

        // Roll (nghiêng trái phải khi rẽ)
        Vector3 cross = Vector3.Cross(currentForward, dir);
        float rollSign = Mathf.Sign(cross.y);
        float angleDiff = Vector3.Angle(currentForward, dir) / 90f;
        float roll = rollSign * angleDiff * maxRoll;

        // Tạo Quaternion từ Pitch, Yaw, Roll
        Quaternion tilt = Quaternion.Euler(pitch, targetYaw.eulerAngles.y, roll);
        return tilt;
    }

    /// <summary>
    /// Tính điểm spline theo công thức Catmull-Rom
    /// </summary>
    Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
        );
    }

    /// <summary>
    /// Vẽ Gizmos cho đường bay
    /// </summary>
    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Count < 4)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < pathPoints.Count - 3; i++)
        {
            Vector3 previousPoint = pathPoints[i + 1].position;

            for (float t = 0; t <= 1f; t += 0.05f)
            {
                Vector3 point = CatmullRom(t,
                    pathPoints[i].position,
                    pathPoints[i + 1].position,
                    pathPoints[i + 2].position,
                    pathPoints[i + 3].position);

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }

        // Vẽ điểm
        Gizmos.color = Color.red;
        foreach (var point in pathPoints)
        {
            if (point != null)
                Gizmos.DrawSphere(point.position, 0.2f);
        }
    }
}
