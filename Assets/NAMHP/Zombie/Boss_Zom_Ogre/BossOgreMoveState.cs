using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossOgreMoveState : BaseState<BossOgreState>
{
    [SerializeField] private HumanMoveBase humanMoveBase;
    [SerializeField] private BotNetwork botNet;
    [SerializeField] private Animator anim;
    [SerializeField] private AnimEventBossOgre animEventBoss;
    [SerializeField] protected WayPoint path;
    [SerializeField] protected int moveIndex;
    public int[] moveIndexList;
    [SerializeField] public bool isMoveDone;
    [SerializeField] public bool isFirstTime;
    [SerializeField] public bool hasCompletedStart = false; // Đã hoàn thành Start state chưa
    [SerializeField] public int currentAttackPointIndex = 0;
    [SerializeField] public bool hasReachedAttackPoint = false;


    public readonly int runHash = Animator.StringToHash("IsMoveDone");

    private void Start()
    {
        path = botNet.Path;
        
    }

    private void OnEnable()
    {
        isFirstTime = true;
        
    }

    public override void EnterState()
    {
        Init();
        //Invoke(nameof(Init), 0.1f);
    }
 
    public void Init()
    {
        path = botNet.Path;
        if (isFirstTime)
        {
            animEventBoss.SetCanShakeStep(true);
            if (humanMoveBase.isHaveParent)
            {
                moveIndex = moveIndexList[0]; // tức là chỉ điểm đến cuối 
            }
            else
            {
                moveIndex = moveIndexList[1]; // chạy tiếp tới điểm 
            }
            anim.Play("Ogre_Move");
        }
        else
        {
            animEventBoss.SetCanShakeStep(false);
            // Reset trạng thái khi quay lại Move state từ Attack
            if (hasCompletedStart)
            {
                hasReachedAttackPoint = false;
            }
            anim.Play("Ogre_Move");
        }
        isMoveDone = false;
    }

    public override void UpdateState()
    {
        if (isFirstTime)
        {
            OnMoveFirstTime(); // Di chuyển đến điểm cuối của waypoint
        }
        else if (hasCompletedStart)
        {
            OnMoveAttackPoint(); // Di chuyển đến các attack point
        }
    }
    
    private void OnMoveFirstTime()
    {
        if (path != null) 
        {
            if (!humanMoveBase.isHaveParent && moveIndex < path.WayPoints.Count)
            {
                humanMoveBase.SetBotMove(path.WayPoints[moveIndex]);
                float distance = Vector3.Distance(humanMoveBase.myTrans.position, path.WayPoints[moveIndex].position);
                if (distance < 0.1)
                {
                    moveIndex++;
                }
            }
            if (moveIndex == path.WayPoints.Count)
            {
                //anim.SetBool("IsRoar", true);
                Debug.Log("Reached the end of waypoints, IsRoar");
                isFirstTime = false;
                isMoveDone = true; // Đã đến điểm cuối, sẵn sàng chuyển sang Start
            }
        }
    }

    private void OnMoveAttackPoint()
    {
        if (path != null && path.AttackWayPoints != null && path.AttackWayPoints.Count > 0)
        {
            // Đảm bảo index không vượt quá số lượng điểm attack
            if (currentAttackPointIndex >= path.AttackWayPoints.Count)
            {
                currentAttackPointIndex = 0; // Reset về điểm đầu tiên
            }
            
            Transform targetAttackPoint = path.AttackWayPoints[currentAttackPointIndex];
            humanMoveBase.SetBotMove(targetAttackPoint);
            
            float distance = Vector3.Distance(humanMoveBase.myTrans.position, targetAttackPoint.position);
            
            // Kiểm tra nếu đã đến điểm attack
            if (distance < 0.5f && !hasReachedAttackPoint)
            {
                hasReachedAttackPoint = true;
                isMoveDone = true;

                Debug.Log($"Reached attack point {currentAttackPointIndex}: {targetAttackPoint.name}");
            }
        }
        else
        {
            Debug.LogWarning("AttackWayPoints is null or empty!");
        }
    }
    
    
    // Được gọi từ Start State khi hoàn thành
    public void SetStartCompleted()
    {
        hasCompletedStart = true;
        
        Debug.Log("Start state completed, now will move to attack points");
    }
    
    // Phương thức để chuyển sang điểm attack tiếp theo (được gọi từ Attack State)
    public void MoveToNextAttackPoint()
    {
        if (path != null && path.AttackWayPoints != null && path.AttackWayPoints.Count > 0)
        {
            // Chuyển sang điểm attack tiếp theo
            currentAttackPointIndex++;
            
            // Nếu đã hết điểm attack, quay lại điểm đầu tiên
            if (currentAttackPointIndex >= path.AttackWayPoints.Count)
            {
                currentAttackPointIndex = 0;
            }
            
            // Reset trạng thái để bắt đầu di chuyển đến điểm mới
            hasReachedAttackPoint = false;
            isMoveDone = false;
            
            Debug.Log($"Moving to next attack point: {currentAttackPointIndex}");
        }
    }

    public override void ExitState()
    {
        isMoveDone = false; // Reset trạng thái di chuyển
        // Cleanup khi thoát khỏi state
    }
    
    public override BossOgreState GetNextState()
    {
        if (botNet.DeadExplosion)
            return BossOgreState.DeadExplosion;
        else
        {
            if(botNet.IsDead)
            {
                return BossOgreState.Dead;
            }
            else
            {
                // Lần đầu tiên: di chuyển đến điểm cuối rồi chuyển sang Start
                if (isFirstTime)
                {
                    return StateKey; // Tiếp tục ở Move state
                }
                else if (!hasCompletedStart && isMoveDone)
                {
                    return BossOgreState.Start; // Chuyển sang Start state
                }
                // Sau khi hoàn thành Start: di chuyển đến attack points
                else if (hasCompletedStart)
                {
                    if (hasReachedAttackPoint && isMoveDone)
                    {
                        return BossOgreState.Attack; // Đã đến attack point, chuyển sang tấn công
                    }
                    return StateKey; // Tiếp tục di chuyển đến attack point
                }
                else
                {
                    return StateKey;
                }
            }
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            // Vẽ waypoints (đường đi ban đầu)
            if (path.WayPoints != null && path.WayPoints.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.WayPoints.Count; i++)
                {
                    Gizmos.DrawWireSphere(path.WayPoints[i].position, 0.3f);
                    if (i == path.WayPoints.Count - 1) // Điểm cuối
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere(path.WayPoints[i].position, 0.4f);
                    }
                }
            }
            
            // Vẽ attack points
            if (path.AttackWayPoints != null && path.AttackWayPoints.Count > 0)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < path.AttackWayPoints.Count; i++)
                {
                    Gizmos.DrawWireSphere(path.AttackWayPoints[i].position, 0.3f);
                }
                
                // Vẽ điểm attack hiện tại
                if (hasCompletedStart && currentAttackPointIndex < path.AttackWayPoints.Count)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(path.AttackWayPoints[currentAttackPointIndex].position, 0.5f);
                    
                    // Vẽ đường đi từ boss đến điểm attack hiện tại
                    if (humanMoveBase != null && humanMoveBase.myTrans != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(humanMoveBase.myTrans.position, path.AttackWayPoints[currentAttackPointIndex].position);
                    }
                }
            }
        }
    }
#endif
}