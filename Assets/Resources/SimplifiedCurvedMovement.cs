using UnityEngine;
using System.Collections.Generic;

public class SimplifiedCurvedMovement : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Transform> pathPoints = new List<Transform>();
    public float controlPointDistance = 5f;
    public float controlPointAngle = 45f;
    
    [Header("Movement Settings")]
    public float movementSpeed = 1f;
    public float rotationSpeed = 5f;
    public bool loopPath = true;

    [Header("Advanced Rotation")]
    public float maxBankAngle = 45f;
    public float maxPitchAngle = 30f;
    public float rotationSmoothing = 5f;

    [Header("Debug Settings")]
    public int debugSegments = 20;
    public bool showControlPoints = true;

    private int currentSegment = 0;
    private float segmentProgress = 0f;
    private bool isMovingForward = true;

    private void Update()
    {
        if (pathPoints.Count < 2) return;

        // Update movement progress
        segmentProgress += Time.deltaTime * movementSpeed * (isMovingForward ? 1 : -1);

        // Handle segment transitions
        if (segmentProgress >= 1f)
        {
            segmentProgress = 0f;
            if (isMovingForward)
            {
                currentSegment++;
                if (currentSegment >= pathPoints.Count - 1)
                {
                    if (loopPath)
                    {
                        currentSegment = 0;
                    }
                    else
                    {
                        currentSegment = pathPoints.Count - 2;
                        isMovingForward = false;
                    }
                }
            }
        }
        else if (segmentProgress <= 0f)
        {
            segmentProgress = 1f;
            if (!isMovingForward)
            {
                currentSegment--;
                if (currentSegment < 0)
                {
                    if (loopPath)
                    {
                        currentSegment = pathPoints.Count - 2;
                    }
                    else
                    {
                        currentSegment = 0;
                        isMovingForward = true;
                    }
                }
            }
        }

        // Get current segment points
        Vector3 startPos = pathPoints[currentSegment].position;
        Vector3 endPos = pathPoints[currentSegment + 1].position;
        Vector3 controlPoint = CalculateControlPoint(currentSegment);

        // Calculate positions
        Vector3 currentPosition = CalculateBezierPoint(segmentProgress, startPos, controlPoint, endPos);
        Vector3 nextPosition = CalculateBezierPoint(Mathf.Clamp01(segmentProgress + 0.01f), startPos, controlPoint, endPos);
        Vector3 direction = (nextPosition - currentPosition).normalized;

        // Update position and rotation
        transform.position = currentPosition;
        UpdateRotation(direction);
    }

    private Vector3 CalculateControlPoint(int segmentIndex)
    {
        Vector3 startPos = pathPoints[segmentIndex].position;
        Vector3 endPos = pathPoints[segmentIndex + 1].position;
        
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
        if (pathPoints.Count < 2) return;

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] == null || pathPoints[i + 1] == null) continue;

            Vector3 startPos = pathPoints[i].position;
            Vector3 endPos = pathPoints[i + 1].position;
            Vector3 controlPoint = CalculateControlPoint(i);

            // Draw path
            Gizmos.color = Color.green;
            Vector3 previousPoint = startPos;
            for (int j = 1; j <= debugSegments; j++)
            {
                float t = j / (float)debugSegments;
                Vector3 currentPoint = CalculateBezierPoint(t, startPos, controlPoint, endPos);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            // Draw control points and connections
            if (showControlPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(startPos, 0.1f);
                Gizmos.DrawSphere(controlPoint, 0.1f);
                if (i == pathPoints.Count - 2)
                {
                    Gizmos.DrawSphere(endPos, 0.1f);
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(startPos, controlPoint);
                Gizmos.DrawLine(controlPoint, endPos);
            }
        }
    }

    public void ReverseDirection()
    {
        isMovingForward = !isMovingForward;
    }
}