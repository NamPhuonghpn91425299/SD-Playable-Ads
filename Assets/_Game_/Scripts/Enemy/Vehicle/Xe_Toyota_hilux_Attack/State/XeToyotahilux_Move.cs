using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class XeToyotahilux_Move : StateBase
{
    [Header("Front Wheels (separate)")]
    public Transform wheelFL; // Bánh trước trái
    public Transform wheelFL_XoayTron; // Bánh trước trái
    public Transform wheelFR; // Bánh trước phải
    public Transform wheelFR_XoayTron; // Bánh trước phải

    [Header("Rear Axle (combined)")]
    public Transform rearAxle; // Trục bánh sau gộp

    [Header("Car Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 2f;
    public float wheelSpinSpeed = 360f;
    public float maxSteerAngle = 30f;
    
    [Header("Movement Mode")]
    [Tooltip("Mode 1: Attack tại mỗi waypoint (Logic cũ)\nMode 2: Đi hết waypoint → attack → loop attack points")]
    public MovementMode movementMode = MovementMode.AttackAtEachWaypoint;
    
    [Header("Pathing Info (Read-Only)")]
    [Tooltip("Tuyến đường mà bot này đang đi theo.")]
    [SerializeField] private PointGroup assignedPath; // Để debug trong Inspector
    [SerializeField] private BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    private int currentPointIndex = 0; // Điểm tiếp theo cần đến
    
    [Header("Mode 2 Settings (Debug)")]
    [SerializeField] private bool hasCompletedInitialPath = false; // Đã hoàn thành waypoint đầu tiên chưa
    [SerializeField] private int currentAttackPointIndex = 0; // Attack point hiện tại
    
    public enum MovementMode
    {
        AttackAtEachWaypoint = 1,  // Mode 1: Attack tại mỗi waypoint (Logic cũ)
        FullPathThenAttackLoop = 2 // Mode 2: Đi hết path → attack → loop attack points
    }

    private void Start()
    {
        if(assignedPath == null)
            assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        
    }

    private void OnEnable()
    {
        if(assignedPath == null)
            assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
    }

    private void OnDisable()
    {
        assignedPath = null; // Clear reference khi object bị disable
    }

    public override void EnterState()
    {
        //botIdentity = botContext.botNetwork.botIdentity;
        // assignedPath = botIdentity.AssignedPath;
    }

    public override void UpdateState()
    {
        if(!botContext.stateController.canDead)
            return;
        
        // Validate path data
        if (assignedPath == null)
        {
            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        // Xử lý theo mode
        if (movementMode == MovementMode.AttackAtEachWaypoint)
        {
            UpdateMode1_AttackAtEachWaypoint();
        }
        else if (movementMode == MovementMode.FullPathThenAttackLoop)
        {
            UpdateMode2_FullPathThenAttackLoop();
        }
        
        // Common wheel and steering animation
        AnimateWheelsAndSteering();
    }
    
    /// <summary>
    /// Mode 1: Logic cũ - Attack tại mỗi waypoint
    /// </summary>
    void UpdateMode1_AttackAtEachWaypoint()
    {
        if (currentPointIndex >= assignedPath.points.Count)
        {
            Debug.LogError($"Bot '{gameObject.name}' Mode 1: Index vượt quá waypoints.");
            return;
        }

        // Hướng đến waypoint hiện tại
        Vector3 targetPos = assignedPath.points[currentPointIndex].position;
        Vector3 directionToTarget = targetPos - TF.position;
        Vector3 flatDirection = directionToTarget;
        flatDirection.y = 0;

        // Nếu gần waypoint → chuyển sang waypoint tiếp theo và attack
        if (directionToTarget.magnitude < 1.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % assignedPath.points.Count;
            botContext.stateController.ChangeState(GameConstants.EnemyState.Attack);
            return;
        }

        // Di chuyển và xoay
        MoveAndRotateToTarget(targetPos, flatDirection);
    }
    
    /// <summary>
    /// Mode 2: Đi hết waypoint → attack → loop attack points
    /// </summary>
    void UpdateMode2_FullPathThenAttackLoop()
    {
        if (!hasCompletedInitialPath)
        {
            // Phase 1: Đi hết waypoints
            if (currentPointIndex >= assignedPath.points.Count)
            {
                // Đã đi hết waypoints
                hasCompletedInitialPath = true;
                InitializeAttackPointLoop();
                botContext.stateController.ChangeState(GameConstants.EnemyState.Attack);
                return;
            }
            
            // Di chuyển theo waypoints
            Vector3 targetPos = assignedPath.points[currentPointIndex].position;
            Vector3 directionToTarget = targetPos - TF.position;
            Vector3 flatDirection = directionToTarget;
            flatDirection.y = 0;

            if (directionToTarget.magnitude < 1.5f)
            {
                currentPointIndex++; // Chuyển waypoint tiếp theo (không loop)
                return;
            }
            
            MoveAndRotateToTarget(targetPos, flatDirection);
        }
        else
        {
            // Phase 2: Di chuyển giữa attack points
            if (assignedPath.attackPoints == null || assignedPath.attackPoints.Count == 0)
            {
                Debug.LogWarning($"Bot '{gameObject.name}' Mode 2: Không có attack points để di chuyển.");
                return;
            }
            
            if (currentAttackPointIndex >= assignedPath.attackPoints.Count)
            {
                currentAttackPointIndex = 0; // Wrap around
            }
            
            Vector3 targetPos = assignedPath.attackPoints[currentAttackPointIndex].position;
            Vector3 directionToTarget = targetPos - TF.position;
            Vector3 flatDirection = directionToTarget;
            flatDirection.y = 0;

            if (directionToTarget.magnitude < 1.5f)
            {
                // Chọn attack point ngẫu nhiên tiếp theo
                SelectRandomAttackPoint();
                botContext.stateController.ChangeState(GameConstants.EnemyState.Attack);
                return;
            }
            
            MoveAndRotateToTarget(targetPos, flatDirection);
        }
    }
    
    /// <summary>
    /// Di chuyển và xoay về phía target
    /// </summary>
    void MoveAndRotateToTarget(Vector3 targetPos, Vector3 flatDirection)
    {
        // Xoay thân xe (chỉ xoay theo mặt phẳng XZ)
        Quaternion targetRot = Quaternion.LookRotation(flatDirection);
        TF.rotation = Quaternion.Slerp(TF.rotation, targetRot, turnSpeed * Time.deltaTime);

        // Di chuyển theo hướng có độ cao
        TF.position = Vector3.MoveTowards(TF.position, targetPos, moveSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Animate bánh xe và lái
    /// </summary>
    void AnimateWheelsAndSteering()
    {
        // Quay bánh xe khi xe di chuyển
        float spinAmount = wheelSpinSpeed * Time.deltaTime;
        wheelFL_XoayTron.Rotate(Vector3.right, spinAmount);
        wheelFR_XoayTron.Rotate(Vector3.right, spinAmount);
        rearAxle.Rotate(Vector3.right, spinAmount); // quay cả trục sau

        // Xoay góc lái bánh trước (trục Y)
        UpdateSteering();
    }
    
    /// <summary>
    /// Khởi tạo attack point loop cho Mode 2
    /// </summary>
    void InitializeAttackPointLoop()
    {
        if (assignedPath.attackPoints != null && assignedPath.attackPoints.Count > 0)
        {
            currentAttackPointIndex = Random.Range(0, assignedPath.attackPoints.Count);
        }
        else
        {
            Debug.LogWarning($"Bot '{gameObject.name}' Mode 2: Không có attack points, sẽ dùng waypoints.");
            // Fallback: dùng waypoints như attack points
            currentAttackPointIndex = Random.Range(0, assignedPath.points.Count);
        }
    }
    
    /// <summary>
    /// Chọn attack point ngẫu nhiên khác với hiện tại
    /// </summary>
    void SelectRandomAttackPoint()
    {
        if (assignedPath.attackPoints == null || assignedPath.attackPoints.Count <= 1)
        {
            return; // Không đủ attack points để chọn
        }
        
        int newIndex;
        do {
            newIndex = Random.Range(0, assignedPath.attackPoints.Count);
        } while (newIndex == currentAttackPointIndex);
        
        currentAttackPointIndex = newIndex;
    }

    public override void ExitState()
    {
        
    }

    public void RotateBanhXe()
    {
        // Quay bánh xe khi xe di chuyển
        float spinAmount = wheelSpinSpeed * Time.deltaTime;
        wheelFL_XoayTron.Rotate(Vector3.right, spinAmount);
        wheelFR_XoayTron.Rotate(Vector3.right, spinAmount);
        rearAxle.Rotate(Vector3.right, spinAmount); // quay cả trục sau
    }
    
    void UpdateSteering()
    {
        // Lấy target position dựa trên mode hiện tại
        Vector3 targetPosition = GetCurrentTargetPosition();
        if (targetPosition == Vector3.zero) return; // Không có target hợp lệ
        
        Vector3 localTarget = TF.InverseTransformPoint(targetPosition);
        float steerAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        steerAngle = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
    
        // Combine spinning and steering rotations
        Quaternion spinRotation = Quaternion.Euler(wheelFL.localRotation.eulerAngles.x, 0, 0);
        Quaternion steerRotation = Quaternion.Euler(0, steerAngle, 0);
    
        wheelFL.localRotation = steerRotation * spinRotation;
        wheelFR.localRotation = steerRotation * spinRotation;
    }
    
    /// <summary>
    /// Lấy vị trí target hiện tại dựa trên mode
    /// </summary>
    Vector3 GetCurrentTargetPosition()
    {
        if (movementMode == MovementMode.AttackAtEachWaypoint)
        {
            // Mode 1: Dùng waypoints
            if (currentPointIndex < assignedPath.points.Count)
                return assignedPath.points[currentPointIndex].position;
        }
        else if (movementMode == MovementMode.FullPathThenAttackLoop)
        {
            if (!hasCompletedInitialPath)
            {
                // Phase 1: Dùng waypoints
                if (currentPointIndex < assignedPath.points.Count)
                    return assignedPath.points[currentPointIndex].position;
            }
            else
            {
                // Phase 2: Dùng attack points
                if (assignedPath.attackPoints != null && currentAttackPointIndex < assignedPath.attackPoints.Count)
                    return assignedPath.attackPoints[currentAttackPointIndex].position;
            }
        }
        
        return Vector3.zero; // Không tìm thấy target hợp lệ
    }
    
    /// <summary>
    /// Reset trạng thái movement cho object pooling
    /// </summary>
    public void ResetMovementState()
    {
        currentPointIndex = 0;
        currentAttackPointIndex = 0;
        hasCompletedInitialPath = false;
    }
    
    /// <summary>
    /// Reset góc quay của bánh xe về trạng thái ban đầu
    /// </summary>
    public void ResetWheelRotations()
    {
        // Reset bánh trước trái
        if (wheelFL != null)
            wheelFL.localRotation = Quaternion.Euler(18.35f,0,0);
            
        // Reset bánh trước phải  
        if (wheelFR != null)
            wheelFR.localRotation = Quaternion.Euler(-19.5f, 0, 0);
    }
}
