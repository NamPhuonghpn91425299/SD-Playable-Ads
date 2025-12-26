using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Panzerwerfer_Move : StateBase
{
    [Header("Tank Track Transforms")]
    [SerializeField] private MeshRenderer _wheelChain;
    [SerializeField] private Transform _wheelLeftRotaX;
    [SerializeField] private Transform _wheelRightRotaX;
    [SerializeField] private Transform _wheelLeftRotaY;
    [SerializeField] private Transform _wheelRightRotaY;



    [Header("Tank Body Animation")]
    [SerializeField] protected Transform rotaStopBody;

    [Header("Tank Movement Settings")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _rotaSpeed = 90f;
    [SerializeField] private float _turnAroundSpeed = 45f;
    [SerializeField] private float trackSpinSpeed = 180f;
    [SerializeField] private float stopShakeIntensity = 5f; // Cường độ rung lắc khi dừng
    [SerializeField] private float moveShakeIntensity = 2f; // Cường độ rung lắc khi di chuyển
    
    [Header("Movement Mode")]
    [Tooltip("Mode 1: Attack tại mỗi waypoint (Logic cũ)\nMode 2: Đi hết waypoint → attack → loop attack points")]
    public MovementMode movementMode = MovementMode.AttackAtEachWaypoint;
    
   
    [SerializeField] private PointGroup assignedPath; // Để debug trong Inspector
    [SerializeField] private BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    private int currentPointIndex = 0; // Điểm tiếp theo cần đến
    
    [Header("Mode 2 Settings (Debug)")]
    [SerializeField] private bool hasCompletedInitialPath = false; // Đã hoàn thành waypoint đầu tiên chưa
    [SerializeField] private int currentAttackPointIndex = 0; // Attack point hiện tại

    [Header("List of index attack points")]
    [SerializeField] private List<int> attackPointIndices = new List<int>(); // Danh sách index các attack point
    private bool isMoving = false;
    private bool wasMovingLastFrame = false;
    private float animationTimer = 0f;
    private float countTime = 0f; // Timer cho animation curves
    private Vector3 lastPosition;
    
    // Delegate cho animation actions
    private System.Action vehicleAction;

    public enum MovementMode
    {
        AttackAtEachWaypoint = 1,  // Mode 1: Attack tại mỗi waypoint (Logic cũ)
        FullPathThenAttackLoop = 2, // Mode 2: Đi hết path → attack → loop attack points
        AttackAtIndex = 3 // Mode 3: Attack tại các index xác định (dành cho map đặc biệt)
    }

    private void Start()
    {
        if(assignedPath == null && botIdentity != null)
            assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        
        lastPosition = TF.position;
    }

    private void OnEnable()
    {
        if(assignedPath == null && botIdentity != null)
            assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
    }

    public override void EnterState()
    {
        // Initialize path if needed
        if(assignedPath == null && botIdentity != null)
            assignedPath = botIdentity.AssignedPath;
    }

    public override void ExitState()
    {
        isMoving = false;
        wasMovingLastFrame = false;
        vehicleAction = null; // Clear delegates
        countTime = 0f;
    }

    public override void UpdateState()
    {
        if(!botContext.stateController.canDead)
            return;

        _wheelChain.material.SetTextureOffset("_MainTex", new Vector2(0, -Time.time * 0.5f));

        // Validate path data
        if (assignedPath == null)
        {
            Debug.LogError($"Tank '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        // Check if tank is moving
        float distanceMoved = Vector3.Distance(TF.position, lastPosition);
        wasMovingLastFrame = isMoving;
        isMoving = distanceMoved > 0.01f;

        // Reset animation timer when starting to move
        if (isMoving && !wasMovingLastFrame)
        {
            countTime = 0f;
            vehicleAction = null; // Clear stop action
           

        }
        // Start stop animation timer when stopping
        else if (!isMoving && wasMovingLastFrame)
        {
            countTime = 0f;
            vehicleAction = null; // Clear move action

        }
        
        lastPosition = TF.position;

        // Xử lý theo mode
        if (movementMode == MovementMode.AttackAtEachWaypoint)
        {
            botContext.animator.Play("Panzerwerfer_StartMove", -1, 0f);
            UpdateMode1_AttackAtEachWaypoint();
        }
        else if (movementMode == MovementMode.FullPathThenAttackLoop)
        {
            UpdateMode2_FullPathThenAttackLoop();
        }
        else if (movementMode == MovementMode.AttackAtIndex)
        {
            UpdateMode3_AttackAtIndex();
        }
        
        // Common track animation và delegate actions
            AnimateTracksAndBody();
        
        // Execute vehicle actions (stop/move animations)
        vehicleAction?.Invoke();
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

            if (directionToTarget.magnitude < 2f)
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
                Debug.LogWarning($"Tank '{gameObject.name}' Mode 2: Không có attack points để di chuyển.");
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

            if (directionToTarget.magnitude < 2f)
            {
                // Chọn attack point ngẫu nhiên tiếp theo
                SelectRandomAttackPoint();
                botContext.stateController.ChangeState(GameConstants.EnemyState.Attack);
                return;
            }
            
            MoveAndRotateToTarget(targetPos, flatDirection);
        }
    }

    private void UpdateMode3_AttackAtIndex()
    {
         if (currentPointIndex >= assignedPath.points.Count)
        {
            Debug.LogError($"Bot '{gameObject.name}' Mode 3: Index vượt quá waypoints.");
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
            foreach (int index in attackPointIndices)
            {
                if (currentPointIndex == index)
                {
                    botContext.stateController.ChangeState(GameConstants.EnemyState.Attack);
                    break; // Chỉ cần đổi state một lần
                }
            }
            return;
        }

        // Di chuyển và xoay
        MoveAndRotateToTarget(targetPos, flatDirection);
    }
    
    /// <summary>
    /// Di chuyển và xoay về phía target (tank movement)
    /// </summary>
    void MoveAndRotateToTarget(Vector3 targetPos, Vector3 flatDirection)
    {
        // Tank xoay chậm hơn xe thường
        Quaternion targetRot = Quaternion.LookRotation(flatDirection);
        TF.rotation = Quaternion.Slerp(TF.rotation, targetRot, _turnAroundSpeed * Time.deltaTime);

        // Di chuyển với tốc độ tank
        TF.position = Vector3.MoveTowards(TF.position, targetPos, _moveSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Animate tank tracks và body
    /// </summary>
    void AnimateTracksAndBody()
    {
        animationTimer += Time.deltaTime;
        
        // Animate tracks khi di chuyển
        if (isMoving)
        {
            float spinAmount = trackSpinSpeed * Time.deltaTime;
            
            // Rotate track wheels
            if (_wheelLeftRotaX != null)
                _wheelLeftRotaX.Rotate(Vector3.right, spinAmount);
            if (_wheelRightRotaX != null)
                _wheelRightRotaX.Rotate(Vector3.right, spinAmount);
            
        }
        
        // Tank specific track animation for Y rotation (steering)
        if (_wheelLeftRotaY != null && _wheelRightRotaY != null)
        {
            Vector3 targetPosition = GetCurrentTargetPosition();
            if (targetPosition != Vector3.zero)
            {
                Vector3 localTarget = TF.InverseTransformPoint(targetPosition);
                float steerAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                steerAngle = Mathf.Clamp(steerAngle, -15f, 15f); // Tank có góc lái nhỏ hơn
                
                _wheelLeftRotaY.localRotation = Quaternion.Euler(0, steerAngle, 0);
                _wheelRightRotaY.localRotation = Quaternion.Euler(0, steerAngle, 0);
            }
        }
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
            Debug.LogWarning($"Tank '{gameObject.name}' Mode 2: Không có attack points, sẽ dùng waypoints.");
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
        isMoving = false;
        wasMovingLastFrame = false;
        animationTimer = 0f;
        countTime = 0f;
        vehicleAction = null;
        lastPosition = TF.position;
    }
    
    /// <summary>
    /// Public method để rotate tracks (tương thích với hệ thống cũ)
    /// </summary>
    public void RotateTracks()
    {
        float spinAmount = trackSpinSpeed * Time.deltaTime;
        
        if (_wheelLeftRotaX != null)
            _wheelLeftRotaX.Rotate(Vector3.right, spinAmount);
        if (_wheelRightRotaX != null)
            _wheelRightRotaX.Rotate(Vector3.right, spinAmount);
    }

   
}
