using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public SimplePath path; // Đường dẫn
    public float speed = 5f; // Tốc độ di chuyển
    public bool alignToPath = true; // Căn chỉnh hướng theo đường dẫn

    private float t = 0f; // Vị trí trên đoạn đường hiện tại (0-1)
    private int currentSegment = 0; // Đoạn đường hiện tại

    private void Update()
    {
        if (path == null || path.points.Count < 2) return;

        // Tính chiều dài đoạn hiện tại
        float segmentLength = GetSegmentLength(currentSegment);
        t += (speed * Time.deltaTime) / segmentLength;

        // Chuyển sang đoạn tiếp theo khi vượt qua đoạn hiện tại
        if (t >= 1f)
        {
            t = 0f;
            currentSegment++;

            if (currentSegment >= path.points.Count - (path.loop ? 0 : 1))
            {
                currentSegment = path.loop ? 0 : path.points.Count - 1;
            }
        }

        // Lấy vị trí mới
        Vector3 newPos = GetPointOnPath(currentSegment, t);
        transform.position = newPos;

        // Căn chỉnh hướng theo đường dẫn
        if (alignToPath)
        {
            Vector3 nextPos = GetPointOnPath(currentSegment, Mathf.Clamp01(t + 0.01f));
            Vector3 direction = (nextPos - newPos).normalized;
            if (direction.magnitude > 0f)
            {
                transform.forward = direction;
            }
        }
    }

    private Vector3 GetPointOnPath(int segmentIndex, float t)
    {
        if (path == null || path.points.Count < 2) return Vector3.zero;

        Vector3 p0 = path.GetPreviousPoint(segmentIndex);
        Vector3 p1 = path.points[ClampIndex(segmentIndex)].position;
        Vector3 p2 = path.points[ClampIndex(segmentIndex + 1)].position;
        Vector3 p3 = path.GetNextPoint(segmentIndex);

        return path.GetCatmullRomPoint(p0, p1, p2, p3, t);
    }

    private float GetSegmentLength(int segmentIndex)
    {
        if (path == null || path.points.Count < 2) return 0f;

        Vector3 p0 = path.GetPreviousPoint(segmentIndex);
        Vector3 p1 = path.points[ClampIndex(segmentIndex)].position;
        Vector3 p2 = path.points[ClampIndex(segmentIndex + 1)].position;
        Vector3 p3 = path.GetNextPoint(segmentIndex);

        // Chia đoạn đường thành các phần nhỏ để tính chiều dài gần đúng
        float length = 0f;
        Vector3 lastPos = path.GetCatmullRomPoint(p0, p1, p2, p3, 0f);
        for (int i = 1; i <= path.resolution; i++)
        {
            float t = i / (float)path.resolution;
            Vector3 newPos = path.GetCatmullRomPoint(p0, p1, p2, p3, t);
            length += Vector3.Distance(lastPos, newPos);
            lastPos = newPos;
        }
        return length;
    }

    private int ClampIndex(int index)
    {
        if (path.loop)
        {
            return (index + path.points.Count) % path.points.Count;
        }
        return Mathf.Clamp(index, 0, path.points.Count - 1);
    }
}
