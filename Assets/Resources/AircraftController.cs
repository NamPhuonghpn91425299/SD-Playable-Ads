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

    private void Update()
    {
        if (!isMoving || player == null) return;

        MoveAlongPath();
        AdjustRotation();
    }

    private void MoveAlongPath()
    {
        // Cập nhật quãng đường đã đi
        distanceTraveled += moveSpeed * Time.deltaTime;

        if (distanceTraveled >= path.TotalLength)
        {
            currentWaypointIndex = targetWaypointIndex;
            SelectNextWaypoint();
        }
        else
        {
            transform.position = path.GetPositionAlongPath(distanceTraveled);
        }
    }

    private void AdjustRotation()
    {
        // Lấy vị trí hiện tại và tiếp theo
        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = path.GetPositionAlongPath(Mathf.Min(distanceTraveled + 0.1f, path.TotalLength));
        Vector3 directionToNext = (nextPosition - currentPosition).normalized;

        // Tính góc nghiêng và căn chỉnh hướng
        if (directionToNext != Vector3.zero)
        {
            float bankAngle = CalculateBankAngle(directionToNext);
            float pitchAngle = movingForward ? -maxPitchAngle : maxPitchAngle;

            Quaternion targetRotation = Quaternion.Euler(
                pitchAngle,
                transform.rotation.eulerAngles.y,
                bankAngle
            );

            // Làm mượt chuyển động xoay
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
        }
    }

    private void SelectNextWaypoint()
    {
        if (waypoints.Count < 2) return;

        // Random mục tiêu tiếp theo
        targetWaypointIndex = (currentWaypointIndex + Random.Range(1, waypoints.Count)) % waypoints.Count;

        // Thiết lập lại đường đi
        path = new QuadraticBezierPath(new List<Transform>
        {
            waypoints[currentWaypointIndex],
            waypoints[targetWaypointIndex]
        }, controlPointOffset);

        distanceTraveled = 0f;
        isMoving = true;
    }

    private float CalculateBankAngle(Vector3 movementDirection)
    {
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(movementDirection, Vector3.up);
        float bankAngle = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);
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
