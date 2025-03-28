using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour
{
    public Transform player;
    public Transform upperBody;
    public Transform[] patrolPoints;
    public float detectionRange = 50f;
    public float attackRange = 30f;
    public float speed = 3f;
    [SerializeField] private float upperBodyRotationSpeed = 5f; // Xoay lên/xuống
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    private IBotState currentState;
    private BotPatrolState patrolState;
    private BotAttackState attackState;
    private BotIdleState idleState;
    
    public BotAttackState AttackState => attackState;
    public BotPatrolState PatrolState => patrolState;
    public BotIdleState IdleState => idleState;

    void Start()
    {
        // Kiểm tra và lấy các state, báo lỗi nếu không tìm thấy
        patrolState = GetComponent<BotPatrolState>();
        attackState = GetComponent<BotAttackState>();
        idleState = GetComponent<BotIdleState>();

        if (patrolState == null || attackState == null || idleState == null)
        {
            Debug.LogError("Một hoặc nhiều state chưa được gắn vào GameObject!");
            return;
        }

        currentState = patrolState; // Mặc định là patrol
        //StartCoroutine(CheckPlayerVisibilityRoutine()); // Kiểm tra định kỳ
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }

        // Kiểm tra tầm nhìn mỗi frame thay vì coroutine
        bool canSee = CanSeePlayer();
        if (canSee && currentState != attackState)
        {
            SetState(attackState);
        }
        else if (!canSee && currentState != patrolState)
        {
            ResetUpperBodyRotation();
            SetState(patrolState);
        }

        if (currentState == patrolState)
        {
            ResetUpperBodyRotation();
        }

    }
    /// <summary>
    /// Đưa upperBody về trạng thái thẳng đứng (x = 0)
    /// </summary>
    void ResetUpperBodyRotation()
    {
        if (upperBody != null)
        {
            //Debug.Log("Reset upper body rotation");
            Vector3 eulerAngles = upperBody.localEulerAngles;
            eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, 0, Time.deltaTime * upperBodyRotationSpeed);
            upperBody.localRotation = Quaternion.Euler(eulerAngles);
        }
    }
    public void SetState(IBotState newState)
    {
        if (newState == null || newState == currentState) return;
        currentState = newState;

        // Gọi EnterState() cho tất cả các state nếu có
        if (newState is BotIdleState idle)
        {
            idle.EnterState();
        }
        else if (newState is BotPatrolState patrol)
        {
            patrol.EnterState();
        }
        else if (newState is BotAttackState attack)
        {
            attack.EnterState();
        }
    }

    IEnumerator CheckPlayerVisibilityRoutine()
    {
        while (true)
        {
            bool canSee = CanSeePlayer();
            
            if (canSee && currentState != attackState)
            {
                SetState(attackState);
            }
            else if (!canSee && currentState != patrolState)
            {
                SetState(patrolState);
            }

            yield return new WaitForSeconds(0.1f); // Kiểm tra mỗi 200ms
        }
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // Đặt raycast cao hơn để tránh va chạm mặt đất
        RaycastHit hit;

        // Bắn raycast kiểm tra xem có bị chắn bởi địa hình hay không
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, distanceToPlayer, obstacleMask | playerMask))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.blue, 0.1f);

            // Nếu hit phải Player -> thấy Player -> return true
            if (((1 << hit.transform.gameObject.layer) & playerMask) != 0)
            {
                return true;
            }

            // Nếu không phải Player mà là vật cản (địa hình) -> return false
            return false;
        }

        return false; // Nếu không có va chạm nào xảy ra, coi như không thấy Player
    }



    void OnDrawGizmos()
    {
        if (!player) return;

        Gizmos.color = (currentState is BotAttackState) ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawLine(transform.position, player.position);
    }
}