using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotAI : MonoBehaviour
{
    [SerializeField] private BotNetwork botNetwork; 
    public Transform player;
    
    public float detectionRange = 50f;
    public float speed = 3f;
    //[SerializeField] private float upperBodyRotationSpeed = 5f; // Xoay lên/xuống
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    private IBotState currentState;
    private BotPatrolState patrolState;
    private BotAttackState attackState;
    private BotIdleState idleState;

    // Thêm thời gian cần duy trì trạng thái trước khi chuyển
    [SerializeField] private float seenTimer = 0f;
    [SerializeField] private float unseenTimer = 0f;
    public float stateChangeDelay = 0.2f; // Ví dụ: 0.2 giây
    public bool canSee;
    public bool isChangeState;
    private void Awake()
    {
        botNetwork = GetComponent<BotNetwork>();
        if (player == null)
        {
            player = LocalPlayer.Instance.GetTranformPlayer();
        }

    }

    private void OnEnable()
    {
        
    }

    void Start()
    {
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        canSee = CanSeePlayer();

        // Cập nhật bộ đếm thời gian
        if (canSee)
        {
            seenTimer += Time.deltaTime;
            unseenTimer = 0f;
        }
        else
        {
            unseenTimer += Time.deltaTime;
            seenTimer = 0f;
        }

        // Chỉ chuyển state khi điều kiện ổn định trong stateChangeDelay
        if (seenTimer >= stateChangeDelay && !isChangeState)
        {
            Debug.Log("Chuyển sang trạng thái tấn công");
            isChangeState = true;
        }
        else if (unseenTimer >= stateChangeDelay && isChangeState)
        {
            Debug.Log("Chuyển sang trạng thái tuần tra");
            isChangeState = false;
        }
        
    }
    

    // public void SetState(IBotState newState)
    // {
    //     if (newState == null || newState == currentState) return;
    //
    //     currentState = newState;
    //     if (newState is BotIdleState idle)
    //         idle.EnterState();
    //     else if (newState is BotPatrolState patrol)
    //         patrol.EnterState();
    //     else if (newState is BotAttackState attack)
    //         attack.EnterState();
    // }

    private bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, distanceToPlayer, obstacleMask | playerMask))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.blue, 0.1f);
            if (((1 << hit.transform.gameObject.layer) & playerMask) != 0)
                return true;
            return false;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (!player) return;

        Gizmos.color = (currentState is BotAttackState) ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawLine(transform.position, player.position);
    }
}
