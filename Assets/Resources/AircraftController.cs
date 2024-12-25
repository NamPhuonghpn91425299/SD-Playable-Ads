using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftController : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float controlPointOffset = -0.1f;
    [SerializeField] private float maxBankAngle = 45f; // Góc nghiêng tối đa khi lượn
    [SerializeField] private float maxPitchAngle = 30f; // Góc pitch tối đa
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationSmoothing = 2f;
    
    private QuadraticBezierPath path;
    private float distanceTraveled = 0f;
    private bool isMoving = true;
    private int currentWaypointIndex = -1;
    private int targetWaypointIndex = -1;
    private Transform player;
    private bool movingForward = true;
    
    private void Start()
    {
        if (waypoints.Count == 0) return;
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogWarning("Player not found!");
            
        currentWaypointIndex = Random.Range(0, waypoints.Count);
        transform.position = waypoints[currentWaypointIndex].position;
        SelectNextWaypoint();
    }

    private void SelectNextWaypoint()
    {
        int newTargetIndex;
        do
        {
            newTargetIndex = Random.Range(0, waypoints.Count);
        } while (newTargetIndex == currentWaypointIndex && waypoints.Count > 1);

        targetWaypointIndex = newTargetIndex;
        movingForward = true;

        List<Transform> currentPath = new List<Transform>
        {
            waypoints[currentWaypointIndex],
            waypoints[targetWaypointIndex]
        };

        path = new QuadraticBezierPath(currentPath, controlPointOffset);
        distanceTraveled = 0f;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving || player == null) return;

        distanceTraveled += moveSpeed * Time.deltaTime;

        if (distanceTraveled >= path.TotalLength)
        {
            currentWaypointIndex = targetWaypointIndex;
            SelectNextWaypoint();
            return;
        }

        // Di chuyển theo đường cong
        Vector3 currentPosition = path.GetPositionAlongPath(distanceTraveled);
        transform.position = currentPosition;

        // Lấy hướng di chuyển tiếp theo
        Vector3 nextPosition = path.GetPositionAlongPath(Mathf.Min(distanceTraveled + 0.1f, path.TotalLength));
        Vector3 directionToNext = (nextPosition - currentPosition).normalized;

        // Hướng về player
        Vector3 directionToPlayer = (player.position - currentPosition).normalized;
        
        if (directionToNext != Vector3.zero)
        {
            // Tính góc nghiêng dựa trên hướng di chuyển
            float bankAngle = movingForward ? 
                -CalculateBankAngle(directionToNext) : 
                CalculateBankAngle(directionToNext);
            
            // Tạo rotation hướng về player với góc nghiêng
            Quaternion lookAtRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            Vector3 targetEulerAngles = lookAtRotation.eulerAngles;
            targetEulerAngles.z = bankAngle;
            float pitchAngle = movingForward ? -maxPitchAngle : maxPitchAngle;

            // Tạo rotation tổng hợp
            Quaternion targetRotation = Quaternion.Euler(
                pitchAngle, // Lắc
                transform.rotation.eulerAngles.y, // Giữ nguyên góc yaw
                bankAngle  // Nghiêng
            );
            // Làm mượt chuyển động xoay
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
            // Áp dụng rotation cuối cùng
            Quaternion finalRotation = Quaternion.Euler(targetEulerAngles);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                finalRotation, 
                Time.deltaTime * rotationSmoothing
            );
        }
    }

    private float CalculateBankAngle(Vector3 movementDirection)
    {
        // Sử dụng vector ngang để tính góc nghiêng
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(movementDirection, Vector3.up);
        // Tính toán góc dựa trên độ lệch so với hướng chính
        float bankAngle = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);

        // Giới hạn góc nghiêng
        return Mathf.Clamp(bankAngle, -maxBankAngle, maxBankAngle);
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawGizmos(Color.blue, Color.yellow);
        }
    }

    public void SetWaypoints(List<Transform> newWaypoints)
    {
        waypoints = newWaypoints;
        if (waypoints.Count > 0)
        {
            currentWaypointIndex = Random.Range(0, waypoints.Count);
            transform.position = waypoints[currentWaypointIndex].position;
            SelectNextWaypoint();
        }
    }
}