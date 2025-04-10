using System;
using System.Collections;
using System.Collections.Generic;
using static BrcPlayitaStateMachine;
using UnityEngine;

public class BrcPlayitaMoveState : BaseState<BrcPlayitaState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BotAI bot;
    [SerializeField] private Animator animator;
    [SerializeField] private int patrolIndex = 0;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float idleTime = 2f; // Thời gian chờ tại mỗi điểm tuần tra
    [SerializeField] private float idleTimeMax = 5f; // Thời gian Max chờ tại mỗi điểm tuần tra
    [SerializeField] private float idleTimeMin = 2f; // Thời gian Min chờ tại mỗi điểm tuần tra
    private float idleTimer = 0f;
    private int moveHash = 0;
    private int shootAngleHash = 0;
    private bool isIdle = false; // Trạng thái dừng lại tại điểm tuần tra
    [SerializeField] private bool isAttack = false; // Trạng thái dừng lại tại điểm tuần tra
    private float currentMoveValue = 0f;
    private float velocity = 0f;
    [SerializeField] private float smoothTime = 0.2f;
    private void Awake()
    {
        isAttack = bot.canSee;
    }

    public override void EnterState()
    {
        RandomIdleTimer();
        animator.SetBool("isIdle", false);
        animator.Play("Move 0");
        moveHash = Animator.StringToHash("Move");
        idleTimer = 0f;
        isIdle = false; // Reset trạng thái khi vào PatrolState
    }
    void SmoothLookAt(Vector3 target)
    {
        Vector3 direction = (target - bot.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            bot.transform.rotation = Quaternion.Slerp(bot.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    public void SetMoveAngle(float _shootAngle)
    {
        animator.SetFloat(moveHash, _shootAngle);
        animator.Play("Move 0");
    }
    // public void SetMoveAngle(float _shootAngle)
    // {
    //     // Kiểm tra nếu góc thay đổi quá nhỏ, tránh cập nhật không cần thiết
    //     if (Mathf.Abs(_shootAngle - currentMoveValue) > 0.01f)
    //     {
    //         currentMoveValue = Mathf.SmoothDamp(currentMoveValue, _shootAngle, ref velocity, smoothTime);
    //     }
    //
    //     // Cập nhật giá trị đã làm mượt vào Animator
    //     animator.SetFloat(moveHash, currentMoveValue);
    //
    //     // Phát animation nếu cần
    //     animator.Play("Move 0");
    // }

    public override void UpdateState()
    {
        if (botNetwork.Path.WayPoints.Count == 0) return;

        if (isIdle)
        {
            // Khi bot đang chờ tại điểm tuần tra, tăng timer
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                isIdle = false;
                idleTimer = 0f;
                patrolIndex = (patrolIndex + 1) % botNetwork.Path.WayPoints.Count; // Chuyển sang điểm tiếp theo
            }
            return; // Dừng UpdateState khi bot đang chờ
        }

        // Di chuyển đến điểm tuần tra hiện tại
        Vector3 targetPoint = botNetwork.Path.WayPoints[patrolIndex].position; //bot.patrolPoints[patrolIndex].position;
        bot.transform.position = Vector3.MoveTowards(bot.transform.position, targetPoint, bot.speed * Time.deltaTime);
        SmoothLookAt(targetPoint);

        // Nếu bot đến điểm tuần tra, dừng lại
        if (Vector3.Distance(bot.transform.position, targetPoint) < 1f)
        {
            animator.SetBool("isMoveDone", true);
            //animator.Play("Idle 0");
            isIdle = true; // Bắt đầu trạng thái chờ
        }
        else
        {
            animator.SetBool("isMoveDone", false);
            //animator.Play("Move 0");
        }
    }

    private void RandomIdleTimer()
    {
        idleTime = UnityEngine.Random.Range(idleTimeMin, idleTimeMax);
    }
    public override void ExitState()
    {

    }

    public override BrcPlayitaState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return BrcPlayitaState.Dead;
        }
        else
        {
            if (bot.isChangeState && bot.canSee)
            {
                return BrcPlayitaState.Attack;
            }
            else
            {
                return StateKey;
            }
        }
    }
}
