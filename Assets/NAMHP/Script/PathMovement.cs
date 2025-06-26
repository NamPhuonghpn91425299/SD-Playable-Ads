using UnityEngine;
using System.Collections.Generic;

public class PathMovement : MonoBehaviour
{
    [Header("Path Configuration")] [SerializeField]
    private List<Transform> pathPoints = new List<Transform>();

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float delayTime = 1f; // Thời gian dùng cho cả delay ban đầu và tại điểm

    [Header("Tilt Configuration")] [SerializeField]
    private float moveRotationSpeed = 1f;

    [SerializeField] private float tiltAngleX = 30f;
    [SerializeField] private float tiltAngleZ = 30f;

    private int currentPointIndex = 0; // Chỉ mục điểm hiện tại
    private bool isMovingForward = true; // Hướng di chuyển (tiến/lùi)
    private bool isInitialDelay = true; // Xác định delay ban đầu đã hết hay chưa
    private float delayTimer = 0f; // Bộ đếm để xử lý thời gian dừng
    public bool IsMoving = true;

    private void Start()
    {
        InitializeDelay();
    }

    private void LateUpdate()
    {
        if (!IsMoving || pathPoints.Count < 2) return;

        // Xử lý các trạng thái chính (Delay / Di chuyển)
        if (delayTimer > 0)
        {
            HandleDelay();
        }
        else
        {
            HandleMovement();
        }
    }

    #region Delay Logic

    private void InitializeDelay()
    {
        delayTimer = delayTime; // Khởi tạo thời gian delay ban đầu
        isInitialDelay = true;
    }

    private void HandleDelay()
    {
        delayTimer -= Time.deltaTime;

        // Trong khi delay, trả đối tượng về góc gốc
        ResetToDefaultRotation();

        // Sau khi xong delay ban đầu, đặt cờ "kết thúc delay ban đầu"
        if (isInitialDelay && delayTimer <= 0)
        {
            isInitialDelay = false;
            delayTimer = 0; // Đặt lại bộ đếm
        }
    }

    #endregion

    #region Movement Logic

    private void HandleMovement()
    {
        Transform targetPoint = pathPoints[currentPointIndex];

        // Nếu chưa đến điểm, tiếp tục di chuyển về phía đó
        if (!MoveTowardsTarget(targetPoint.position))
            return;

        // Nếu đã đến mục tiêu, đặt thời gian delay và cập nhật điểm tiếp theo
        delayTimer = delayTime;
        UpdatePointIndex();
    }

    private bool MoveTowardsTarget(Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget > 0.1f)
        {
            // Di chuyển đối tượng
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Cập nhật và nghiêng góc đối tượng khi di chuyển
            AdjustRotation(targetPosition);
            return false;
        }

        return true; // Đã đến mục tiêu
    }

    private void UpdatePointIndex()
    {
        // Di chuyển đến điểm kế tiếp (theo hướng)
        currentPointIndex += isMovingForward ? 1 : -1;

        // Đảo ngược hướng nếu đến cuối hoặc đầu danh sách
        if (currentPointIndex >= pathPoints.Count)
        {
            currentPointIndex = pathPoints.Count - 2;
            isMovingForward = false;
        }
        else if (currentPointIndex < 0)
        {
            currentPointIndex = 1;
            isMovingForward = true;
        }
    }

    #endregion

    #region Rotation Logic

    private void AdjustRotation(Vector3 targetPosition)
    {
        Vector3 movementDirection = (targetPosition - transform.position).normalized;

        if (movementDirection == Vector3.zero)
            return;

        // Tạo góc mục tiêu dựa trên hướng di chuyển và nghiêng
        float bankAngle = CalculateTiltAngle(movementDirection);
        float pitchAngle = isMovingForward ? -tiltAngleX : tiltAngleX;

        Quaternion targetRotation = Quaternion.Euler(
            pitchAngle,
            transform.rotation.eulerAngles.y,
            bankAngle
        );

        // Làm mượt quá trình xoay
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * moveRotationSpeed
        );
    }

    private float CalculateTiltAngle(Vector3 movementDirection)
    {
        // Giảm chiều cao (để xoay chỉ theo trục Z/Y)
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(movementDirection, Vector3.up);

        // Tính toán góc nghiêng
        float bankAngle = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);
        return Mathf.Clamp(bankAngle, -tiltAngleZ, tiltAngleZ);
    }

    private void ResetToDefaultRotation()
    {
        Quaternion defaultRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            defaultRotation,
            Time.deltaTime * moveRotationSpeed
        );
    }

    #endregion

    #region Gizmos Logic

    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] != null && pathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }
    }

    #endregion
}