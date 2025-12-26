using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
// BACKUP FILE - DO NOT USE DIRECTLY
// Original class renamed to avoid conflicts
public class TankPzv_Move_BACKUP: StateBase
{
    [SerializeField] private Renderer wheelRotation;
    [SerializeField] private float wheelSpeed = 0;
    [Header("Pathing Info (Read-Only)")]
    [SerializeField] private PointGroup assignedPath;
    [SerializeField] private BotIdentity botIdentity;
    [Header("Tank Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private Material materialInstance;
    private bool isMoving;
    private Coroutine moveRoutine;
    private float wheelframe = 0;
    private int currentPointIndex = 0;
    private bool isPhase1Completed = false;  // Đã hoàn thành Phase 1 chưa?
    private int lastAttackPointIndex = -1;   // Tránh repeat attack point
    private Transform currentTargetAttackPoint; // Cache attack point hiện tại
    public void GetPoint()
    {
        materialInstance = wheelRotation.material;
        assignedPath = botIdentity.AssignedPath;
        currentPointIndex = 0;
        isPhase1Completed = false;  // Reset khi get point mới
        lastAttackPointIndex = -1;
    }

    public override void EnterState()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        
        // Bắt đầu xoay về hướng waypoint tiếp theo
        moveRoutine = StartCoroutine(RotateToNextWaypoint());
    }

    public override void UpdateState()
    {
#if UNITY_EDITOR
        if (assignedPath == null || assignedPath.points.Count<=0)
        {
            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }
#endif
        WheelRotation();
        if (!isMoving) return;
        HandleTankMovement(); 
        
        
    }
    public void WheelRotation()
    {
        if (wheelRotation == null || materialInstance == null) return;
        wheelframe = (wheelframe + wheelSpeed * Time.deltaTime) % 100f;
        Vector2 offset = materialInstance.mainTextureOffset;
        offset.y = 0.02f * wheelframe;
        materialInstance.mainTextureOffset = offset;
    }
    
    /// <summary>
    /// 🚀 HandleTankMovement: Phase 1 - Đi hết path, Phase 2 - Random attack points
    /// </summary>
    private void HandleTankMovement()
    {
        if (!isPhase1Completed)
        {
            // PHASE 1: Đi hết tất cả waypoints mà KHÔNG attack
            HandlePhase1Movement();
        }
        else
        {
            // PHASE 2: Random giữa attack points và attack
            HandlePhase2Movement();
        }
    }
    /// <summary>
    /// Phase 1: Đi từ waypoint đầu đến cuối (không attack)
    /// </summary>
    private void HandlePhase1Movement()
    {
        Vector3 targetPos = assignedPath.points[currentPointIndex].position;
        Vector3 directionToTarget = targetPos - TF.position;
        
        // Kiểm tra đã đến target chưa
        if (directionToTarget.magnitude < 0.5f)
        {
            currentPointIndex++;
            
            // Kiểm tra đã đi hết waypoints chưa?
            if (currentPointIndex >= assignedPath.points.Count)
            {
                // ✅ Hoàn thành Phase 1 - chuyển sang Phase 2
                isPhase1Completed = true;
                //Debug.Log($"[{gameObject.name}] Phase 1 Complete! Switching to Phase 2 - Attack Loop Mode");
                
                // Attack lần đầu tiên sau khi hoàn thành Phase 1
                // Dừng và chạy animation trước khi attack
                isMoving = false;
                botContext.ChangeAnimAndType(HashStart);
                botContext.stateController.ChangeState(EnemyState.Attack);
                // if (moveRoutine != null)
                //     StopCoroutine(moveRoutine);
                // moveRoutine = StartCoroutine(RotateBeforeAttack());
            }
            else
            {
                // Đến waypoint mới, cần xoay về hướng đó trước khi di chuyển
                isMoving = false;
                if (moveRoutine != null)
                    StopCoroutine(moveRoutine);
                moveRoutine = StartCoroutine(RotateToNextWaypoint());
            }
            return;
        }
        
        // Chỉ di chuyển nếu đã xoay xong (isMoving = true)
        if (isMoving)
        {
            // Di chuyển về phía target
            TF.position = Vector3.MoveTowards(TF.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// Phase 2: Random di chuyển giữa attack points và attack
    /// </summary>
    private void HandlePhase2Movement()
    {
        // Kiểm tra có attack points không
        if (assignedPath.attackPoints == null || assignedPath.attackPoints.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] No attack points found! Falling back to Mode 1");
            isPhase1Completed = false;
            currentPointIndex = 0;
            return;
        }

        // Nếu chưa có target hoặc đã đến target, chọn target mới
        if (currentTargetAttackPoint == null)
        {
            currentTargetAttackPoint = SelectNewAttackPoint();
            if (currentTargetAttackPoint == null)
            {
                Debug.LogError($"[{gameObject.name}] SelectNewAttackPoint returned null!");
                return;
            }
            
            // Xoay về hướng attack point mới trước khi di chuyển
            isMoving = false;
            if (moveRoutine != null)
                StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(RotateToNextWaypoint());
            //Debug.Log($"[{gameObject.name}] Phase 2 - New target: {currentTargetAttackPoint.name}");
        }

        // Di chuyển đến attack point
        Vector3 targetPos = currentTargetAttackPoint.position;
        Vector3 directionToTarget = targetPos - TF.position;
        float distance = directionToTarget.magnitude;
        
        // Kiểm tra đã đến target chưa
        if (distance < 0.5f)
        {
            //Debug.Log($"[{gameObject.name}] Reached: {currentTargetAttackPoint.name} - Switching to Attack!");
            // Clear target để chọn target mới sau khi attack xong
            currentTargetAttackPoint = null;
            
            // Dừng và chạy animation trước khi attack
            isMoving = false;
            botContext.ChangeAnimAndType(HashStart);
            botContext.stateController.ChangeState(EnemyState.Attack);
            // if (moveRoutine != null)
            //     StopCoroutine(moveRoutine);
            // moveRoutine = StartCoroutine(RotateBeforeAttack());
            return;
        }
        
        // Chỉ di chuyển nếu đã xoay xong (isMoving = true)
        if (isMoving)
        {
            // Di chuyển về phía attack point
            TF.position = Vector3.MoveTowards(TF.position, targetPos, moveSpeed * Time.deltaTime);
            
            // Debug thông tin di chuyển
            if (Time.frameCount % 60 == 0) // Log mỗi giây
            {
                //Debug.Log($"[{gameObject.name}] Phase 2 - Moving to: {currentTargetAttackPoint.name}, Distance: {distance:F2}");
            }
        }
    }
    /// <summary>
    /// Xoay về phía player và chạy animation trước khi attack
    /// </summary>
    private IEnumerator RotateBeforeAttack()
    {
        isMoving = false;
        
        // Lấy vị trí player để xoay về phía player
        Vector3 playerPos = GameController.Instance.GetPosLocalPlayer();
        
        // Chỉ chạy animation khi chuẩn bị tấn công
        //botContext.ChangeAnimAndType(HashStart);
        
        // Xoay về phía player
        while (true)
        {
            Vector3 directionToPlayer = playerPos - TF.position;
            Vector3 flatDirection = directionToPlayer;
            flatDirection.y = 0;
            
            if (flatDirection.sqrMagnitude < 0.01f)
                break;

            Quaternion targetRot = Quaternion.LookRotation(flatDirection);
            TF.rotation = Quaternion.RotateTowards(TF.rotation, targetRot, rotationSpeed * 2f * Time.deltaTime); // Xoay nhanh hơn khi attack

            if (Quaternion.Angle(TF.rotation, targetRot) < 1f)
                break;

            yield return null;
        }

        // Sau khi quay xong, chuyển sang attack
        botContext.stateController.ChangeState(EnemyState.Attack);
    }
    /// <summary>
    /// Xoay về hướng waypoint tiếp theo trước khi di chuyển (sau khi attack xong)
    /// </summary>
    private IEnumerator RotateToNextWaypoint()
    {
        isMoving = false;
        
        // Xác định target position dựa trên mode và phase
        Vector3 targetPos = GetCurrentTargetPosition();
        
        
        while (true)
        {
            Vector3 directionToTarget = targetPos - TF.position;
            Vector3 flatDirection = directionToTarget;
            flatDirection.y = 0;
            
            if (flatDirection.sqrMagnitude < 0.01f)
                break;

            Quaternion targetRot = Quaternion.LookRotation(flatDirection);
            TF.rotation = Quaternion.RotateTowards(TF.rotation, targetRot, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(TF.rotation, targetRot) < 1f)
                break;

            yield return null;
        }

        // Sau khi xoay xong, cho phép di chuyển
        isMoving = true;
    }
    /// <summary>
    /// Chọn attack point mới cho Phase 2 (tránh lặp lại)
    /// </summary>
    private Transform SelectNewAttackPoint()
    {
        int attackPointCount = assignedPath.attackPoints.Count;
        
        // Nếu chỉ có 1 attack point thì không có choice
        if (attackPointCount == 1)
        {
            lastAttackPointIndex = 0;
            return assignedPath.attackPoints != null && assignedPath.attackPoints.Count > 0 && assignedPath.attackPoints[0] != null 
                ? assignedPath.attackPoints[0] 
                : null;
        }

        // Random một attack point khác với cái trước đó
        int nextIndex;
        do
        {
            nextIndex = Random.Range(0, attackPointCount);
        } 
        while (nextIndex == lastAttackPointIndex && attackPointCount > 1);

        lastAttackPointIndex = nextIndex;
        return assignedPath.attackPoints[nextIndex];
    }
    /// <summary>
    /// Lấy vị trí target hiện tại dựa trên mode và phase
    /// </summary>
    private Vector3 GetCurrentTargetPosition()
    {
        if (!isPhase1Completed)
        {
            // Phase 1: target là waypoint hiện tại
            if (assignedPath != null && assignedPath.points != null && 
                currentPointIndex < assignedPath.points.Count && 
                assignedPath.points[currentPointIndex] != null)
            {
                return assignedPath.points[currentPointIndex].position;
            }
            return TF.position;
        }
        else
        {
            // Phase 2: sử dụng cached attack point hoặc current position nếu chưa có
            if (currentTargetAttackPoint != null)
                return currentTargetAttackPoint.position;
            else
                return TF.position; // Sử dụng vị trí hiện tại nếu chưa có target
        }
        

    }
    public override void ExitState()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        isMoving = false;
        botContext.ChangeAnimAndType(HashEndStart);
    }
}
