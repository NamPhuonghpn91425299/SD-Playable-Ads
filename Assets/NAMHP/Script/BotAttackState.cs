using System.Collections;
using UnityEngine;
using static HelperCoroutine;
public class BotAttackState : MonoBehaviour, IBotState
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BotAI bot;
    [SerializeField] private float bodyRotationSpeed = 5f; // Xoay ngang
    [SerializeField] private float upperBodyRotationSpeed = 5f; // Xoay lên/xuống
    [SerializeField] private Transform upperBody;
    [SerializeField] private Animator animator;
    [SerializeField] protected GameObject muzzle;
    [SerializeField] private float angleBot = 20f;
    [SerializeField] private bool canAttack;
    private int shootAngleHash = 0;
    private void Awake()
    {
        shootAngleHash = Animator.StringToHash("Shoot");
    }
    public void EnterState()
    {
        Debug.Log("Bot bắt đầu tấn công!");
        animator.SetBool("isIdle", true);
        canAttack = true;
        StartCoroutine(AttackRoutine());
    }
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack && !botNetwork.IsDead)
            {
                float totalDamage = 0f;  // Biến để lưu tổng sát thương gây ra
                int numAttacks = Mathf.FloorToInt(BotConfigSO.timeAttack * BotConfigSO.fireRate);  // Số lần bắn trong timeAttack

                muzzle.SetActive(true);
                muzzle.GetComponent<ParticleSystem>().Play();

                for (int i = 0; i < numAttacks; i++)
                {
                    SetShootAngle(angleBot);
                    //_source.Play();  // Phát âm thanh cho mỗi phát bắn
                    totalDamage += BotConfigSO.damage;  // Cộng sát thương gây ra vào tổng
                    EventManager.Invoke(EventName.OnTakeDamagePlayer, BotConfigSO.damage);
                    //Debug.Log($"sát thương gây ra: {totalDamage}");
                    yield return WaitSeconds(1f / BotConfigSO.fireRate);  // Chờ theo tốc độ bắn
                }

                //Debug.Log($"Tổng lượng damage bot gây ra: {totalDamage}");

                muzzle.SetActive(false);
                animator.SetBool("isReload", true);
                yield return WaitSeconds(BotConfigSO.timeReload);  // Chờ thời gian nạp đạn
                animator.SetBool("isReload", false);
                canAttack = true;  // Sẵn sàng cho lượt tấn công tiếp theo
            }
            yield return null;  // Chờ cho tới frame kế tiếp
        }
    }
    public void UpdateState()
    {
        if (bot == null || bot.player == null) return;

        RotateBody();
        RotateUpperBody();

    }

    /// <summary>
    /// Xoay toàn thân về phía player (chỉ xoay ngang - Y)
    /// </summary>
    void RotateBody()
    {
        Vector3 direction = bot.player.position - bot.transform.position;
        direction.y = 0; // Giữ nguyên chiều cao

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            bot.transform.rotation = Quaternion.Slerp(bot.transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
        }
    }

    /// <summary>
    /// Xoay thân trên để nhắm lên/xuống đúng hướng
    /// </summary>
    void RotateUpperBody()
    {
        if (upperBody == null) return;

        Vector3 targetDirection = bot.player.position - upperBody.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Chỉ xoay theo trục X (nghiêng lên/xuống)
        Vector3 eulerAngles = targetRotation.eulerAngles;
        eulerAngles.y = upperBody.eulerAngles.y; // Giữ nguyên xoay ngang
        upperBody.rotation = Quaternion.Slerp(upperBody.rotation, Quaternion.Euler(eulerAngles), Time.deltaTime * upperBodyRotationSpeed);
    }
    public void SetShootAngle(float _shootAngle)
    {
        animator.Play("Idle");
        animator.SetFloat(shootAngleHash, _shootAngle);
    }
    /// <summary>
    /// Khi bot rời trạng thái tấn công, đưa góc X của upperBody về 0
    /// </summary>
    public void ExitState()
    {
        if (upperBody != null)
        {
            Vector3 eulerAngles = upperBody.localEulerAngles;
            eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, 0, Time.deltaTime * upperBodyRotationSpeed);
            upperBody.localRotation = Quaternion.Euler(eulerAngles);
        }
    }
}
