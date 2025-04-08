using System;
using UnityEngine;

public class BotPatrolState : MonoBehaviour, IBotState
{
    
    [SerializeField] private BotAI bot;
    [SerializeField] private Animator animator;
    [SerializeField] private int patrolIndex = 0;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float idleTime = 2f; // Thời gian chờ tại mỗi điểm tuần tra
    private float idleTimer = 0f;
    private bool isIdle = false; // Trạng thái dừng lại tại điểm tuần tra
    private int moveHash = 0;

    private void Awake()
    {
        moveHash = Animator.StringToHash("Move");
    }

    public BotPatrolState(BotAI bot)
    {
        this.bot = bot;
    }

    public void EnterState()
    {
        idleTimer = 0f;
        isIdle = false; // Reset trạng thái khi vào PatrolState
    }

    public void UpdateState()
    {
        // if (bot.patrolPoints.Count == 0) return;
        //
        // if (isIdle)
        // {
        //     // Khi bot đang chờ tại điểm tuần tra, tăng timer
        //     idleTimer += Time.deltaTime;
        //     if (idleTimer >= idleTime)
        //     {
        //         isIdle = false;
        //         idleTimer = 0f;
        //         patrolIndex = (patrolIndex + 1) % bot.patrolPoints.Count; // Chuyển sang điểm tiếp theo
        //     }
        //     return; // Dừng UpdateState khi bot đang chờ
        // }
        //
        // // Di chuyển đến điểm tuần tra hiện tại
        // Vector3 targetPoint = bot.patrolPoints[patrolIndex].position;
        // bot.transform.position = Vector3.MoveTowards(bot.transform.position, targetPoint, bot.speed * Time.deltaTime);
        // SmoothLookAt(targetPoint);
        //
        // // Nếu bot đến điểm tuần tra, dừng lại
        // if (Vector3.Distance(bot.transform.position, targetPoint) < 1f)
        // {
        //     SetShootAngle(1f);
        //     isIdle = true; // Bắt đầu trạng thái chờ
        // }
        // else
        // {
        //     SetShootAngle(0f);
        // }
        
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
    public void SetShootAngle(float _shootAngle)
    {
        animator.SetFloat(moveHash, _shootAngle);
        animator.Play("Move 0");
    }
}
