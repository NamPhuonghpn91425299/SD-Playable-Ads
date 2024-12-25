using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component sử dụng QuadraticBezierPath
public class WaypointFollower : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float controlPointOffset = -0.1f;
    [SerializeField] private bool loop = true;
    
    private QuadraticBezierPath path;
    private float distanceTraveled = 0f;
    private bool isMoving = true;

    private void Start()
    {
        path = new QuadraticBezierPath(waypoints, controlPointOffset);
    }

    private void Update()
    {
        if (!isMoving) return;

        // Di chuyển dọc theo đường cong
        distanceTraveled += moveSpeed * Time.deltaTime;

        if (distanceTraveled >= path.TotalLength)
        {
            if (loop)
            {
                distanceTraveled = 0f;
            }
            else
            {
                isMoving = false;
                return;
            }
        }

        // Cập nhật vị trí và hướng
        Vector3 newPosition = path.GetPositionAlongPath(distanceTraveled);
        Vector3 direction = path.GetDirectionAtDistance(distanceTraveled);
        
        transform.position = newPosition;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawGizmos(Color.blue, Color.yellow);
        }
    }

    // Phương thức để khởi tạo lại đường đi với waypoints mới
    public void SetNewWaypoints(List<Transform> newWaypoints)
    {
        waypoints = newWaypoints;
        path = new QuadraticBezierPath(waypoints, controlPointOffset);
        distanceTraveled = 0f;
        isMoving = true;
    }
}
