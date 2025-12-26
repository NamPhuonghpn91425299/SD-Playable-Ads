using System.Linq;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine.Serialization;

public class PathMover : MonoBehaviour
{
    [ContextMenu("Refresh Path Points From All Children")]
    public void RefreshAllChildPoints()
    {
        points = GetComponentsInChildren<Transform>()
            .Where(t => t != transform)
            .ToArray();

        Debug.Log($"[PathMover] Refreshed {points.Length} point(s) from all children.");
    }

    [Header("Object to move")]
    public Transform objectToMove;

    [FormerlySerializedAs("waypoints")] [FormerlySerializedAs("pathPoints")] [Header("Path points (in order)")]
    public Transform[] points;

    [Header("Tween Settings")]
    public float duration = 5f;
    public PathType pathType = PathType.CatmullRom;
    public int resolution = 10;
    public bool loop = false;
    public Ease ease = Ease.Linear;
    
    [Header("Rotation Limit")]
    public float maxXRotation = 30f; 
    
    [Header("Gizmos")]
    public Color pathColor = Color.green;
    private bool hasStarted = false;

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space) && !hasStarted)
    //     {
    //         transform.position = points[0].position;
    //         StartMoving();
    //         hasStarted = true;
    //     }
    // }

    void StartMoving()
    {
        if (objectToMove == null || points == null || points.Length < 2) return;

        Vector3[] positions = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            positions[i] = points[i].position;
        }

        objectToMove.DOPath(positions, duration, pathType)
            .SetEase(ease)
            .SetLoops(loop ? -1 : 0)
            .OnUpdate(OnUpdate)
            .OnWaypointChange(index =>
            {
                if (index < points.Length)
                {
                    a = 0;
                    Debug.LogWarning($"⏱️ Thời gian từ point {index} đến {index + 1}: {points[index]:F2} giây"+a);
                }
            });
    }

    private float a = 0;
    private void OnUpdate()
    {
        a += Time.deltaTime;
        print(a);
    }

    public float sphereSize = 0.1f;

    void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;

        Gizmos.color = pathColor;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
                Gizmos.DrawSphere(points[i].position, sphereSize);
        }

        // Vẽ đường cong nội suy
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p0 = i == 0 ? points[i].position : points[i - 1].position;
            Vector3 p1 = points[i].position;
            Vector3 p2 = points[i + 1].position;
            Vector3 p3 = (i + 2 < points.Length) ? points[i + 2].position : p2;

            Vector3 prevPos = p1;
            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 pos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }
    }

    // Công thức Catmull-Rom spline
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
    
}