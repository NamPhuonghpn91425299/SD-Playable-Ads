using UnityEngine;
using System.Collections.Generic;

public class RandomPathMovement : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Transform> pathPoints = new List<Transform>();
    public float controlPointDistance = 5f;
    public float controlPointAngle = 45f;
    
    [Header("Movement Settings")]
    public float movementSpeed = 1f;
    public float rotationSpeed = 5f;
    
    [Header("Advanced Rotation")]
    public float maxBankAngle = 45f;
    public float maxPitchAngle = 30f;
    public float rotationSmoothing = 5f;

    [Header("Debug Settings")]
    public bool showControlPoints = true;
    public int debugSegments = 20;

    private int currentPointIndex = 0;
    private int nextPointIndex = 0;
    private float journeyProgress = 0f;
    
    private void Start()
    {
        if (pathPoints.Count >= 2)
        {
            // Khởi tạo điểm đầu tiên
            currentPointIndex = Random.Range(0, pathPoints.Count);
            // Chọn ngẫu nhiên điểm tiếp theo (khác với điểm hiện tại)
            SelectNextRandomPoint();
        }
    }

    private void SelectNextRandomPoint()
    {
        if (pathPoints.Count < 2) return;

        // Chọn một điểm ngẫu nhiên khác với điểm hiện tại
        int newIndex;
        do
        {
            newIndex = Random.Range(0, pathPoints.Count);
        } while (newIndex == currentPointIndex);

        nextPointIndex = newIndex;
    }

    private void Update()
    {
        if (pathPoints.Count < 2) return;

        // Di chuyển theo đường cong
        journeyProgress += Time.deltaTime * movementSpeed;

        // Khi đến điểm đích
        if (journeyProgress >= 1f)
        {
            // Reset progress và cập nhật các điểm
            journeyProgress = 0f;
            currentPointIndex = nextPointIndex;
            SelectNextRandomPoint();
        }

        // Lấy vị trí các điểm
        Vector3 startPos = pathPoints[currentPointIndex].position;
        Vector3 endPos = pathPoints[nextPointIndex].position;
        Vector3 controlPoint = CalculateControlPoint(startPos, endPos);

        // Tính toán vị trí hiện tại và vị trí tiếp theo
        Vector3 currentPosition = CalculateBezierPoint(journeyProgress, startPos, controlPoint, endPos);
        Vector3 nextPosition = CalculateBezierPoint(Mathf.Clamp01(journeyProgress + 0.01f), startPos, controlPoint, endPos);
        Vector3 direction = (nextPosition - currentPosition).normalized;

        // Cập nhật vị trí và xoay
        transform.position = currentPosition;
        UpdateRotation(direction);
    }

    private Vector3 CalculateControlPoint(Vector3 startPos, Vector3 endPos)
    {
        Vector3 direction = (endPos - startPos);
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        
        Quaternion rotation = Quaternion.AngleAxis(controlPointAngle, direction.normalized);
        Vector3 offsetDirection = rotation * perpendicular;
        
        return startPos + direction / 2 + offsetDirection * controlPointDistance;
    }

    private void UpdateRotation(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            float bankAngle = CalculateBankAngle(direction);
            float pitchAngle = CalculatePitchAngle(direction);

            Quaternion targetRotation = Quaternion.Euler(
                pitchAngle,
                Quaternion.LookRotation(direction).eulerAngles.y,
                bankAngle
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSmoothing
            );
        }
    }

    private float CalculateBankAngle(Vector3 direction)
    {
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
        float bankAngle = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);
        return Mathf.Clamp(bankAngle, -maxBankAngle, maxBankAngle);
    }

    private float CalculatePitchAngle(Vector3 direction)
    {
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.right);
        float pitchAngle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.right);
        return Mathf.Clamp(pitchAngle, -maxPitchAngle, maxPitchAngle);
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        t = Mathf.Clamp01(t);
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return uu * p0 + 2 * u * t * p1 + tt * p2;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || pathPoints.Count < 2) return;

        // Vẽ đường đi hiện tại
        Vector3 startPos = pathPoints[currentPointIndex].position;
        Vector3 endPos = pathPoints[nextPointIndex].position;
        Vector3 controlPoint = CalculateControlPoint(startPos, endPos);

        // Vẽ đường cong
        Gizmos.color = Color.green;
        Vector3 previousPoint = startPos;
        for (int j = 1; j <= debugSegments; j++)
        {
            float t = j / (float)debugSegments;
            Vector3 currentPoint = CalculateBezierPoint(t, startPos, controlPoint, endPos);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        // Vẽ điểm điều khiển
        if (showControlPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(startPos, 0.1f);
            Gizmos.DrawSphere(controlPoint, 0.1f);
            Gizmos.DrawSphere(endPos, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, controlPoint);
            Gizmos.DrawLine(controlPoint, endPos);
        }
    }

    // Phương thức công khai để lấy chỉ số điểm hiện tại
    public int GetCurrentPointIndex()
    {
        return currentPointIndex;
    }

    // Phương thức công khai để lấy chỉ số điểm tiếp theo
    public int GetNextPointIndex()
    {
        return nextPointIndex;
    }

    // Phương thức để set điểm tiếp theo theo ý muốn
    public void SetNextPoint(int index)
    {
        if (index >= 0 && index < pathPoints.Count && index != currentPointIndex)
        {
            nextPointIndex = index;
            journeyProgress = 0f; // Reset tiến trình
        }
    }
}