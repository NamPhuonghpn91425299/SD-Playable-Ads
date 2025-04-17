using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BrcBotBzkStateMachine;
public class BrcBotBzkMoveState : BaseState<BrcBotBzkState>
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private BotAI botAI;
    [SerializeField] private Animator animator;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float idleTime = 2f; // Thời gian chờ tại mỗi điểm tuần tra
    [SerializeField] private float idleTimeMax = 5f; // Thời gian Max chờ tại mỗi điểm tuần tra
    [SerializeField] private float idleTimeMin = 3f; // Thời gian Min chờ tại mỗi điểm tuần traa
    private float idleTimer = 0f;
    private int shootAngleHash = 0;
    private bool isIdle = false; // Trạng thái dừng lại tại điểm tuần tra
    private void RandomIdleTimer()
    {
        idleTime = UnityEngine.Random.Range(idleTimeMin, idleTimeMax);
    }
    void SmoothLookAt(Vector3 target)
    {
        Vector3 direction = (target - botAI.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            botAI.transform.rotation = Quaternion.Slerp(botAI.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    public override void EnterState()
    {
        animator.Rebind();
        RandomIdleTimer();
        animator.SetBool("isIdle", false);
        idleTimer = 0;
        isIdle = false;
    }

    public override void UpdateState()
    {
        if(botNetwork.Path.WayPoints.Count == 0) return;
        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                isIdle = false;
                idleTimer = 0;
                currentWaypointIndex = (currentWaypointIndex + 1) % botNetwork.Path.WayPoints.Count;
            }
            return;
        }
        Vector3 targetPosition = botNetwork.Path.WayPoints[currentWaypointIndex].position;
        botAI.transform.position = Vector3.MoveTowards(botAI.transform.position, targetPosition, Time.deltaTime * botAI.speed);
        SmoothLookAt(targetPosition);
        if (Vector3.Distance(botAI.transform.position, targetPosition) < 0.1f)
        {
            isIdle = true;
            animator.SetBool("isMoveDone", true);
        }
        else
        {
            animator.SetBool("isMoveDone", false);
        }
        
    }

    public override void ExitState()
    {
        
    }

    public override BrcBotBzkState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return BrcBotBzkState.Dead;
        }
        else
        {
            if (botAI.isChangeState && botAI.canSee)
            {
                return BrcBotBzkState.Attack;
            }
            else
            {
                return StateKey;
            }
        }
    }
}
