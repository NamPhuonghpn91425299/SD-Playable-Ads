using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static BrcPlayitaStateMachine;
using static HelperCoroutine;
public class BrcPlayitaAttackState : BaseState<BrcPlayitaState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BotAI bot;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] shootClip;
    [SerializeField] private float bodyRotationSpeed = 5f; // Xoay ngang
    //[SerializeField] private float upperBodyRotationSpeed = 5f; // Xoay lên/xuống
    [SerializeField] private Transform upperBody;
    [SerializeField] private Animator animator;
    [SerializeField] protected GameObject muzzle;
    [SerializeField] private float angleBot = 20f;
    [SerializeField] private float attackSoundChance = 0.5f;
    [SerializeField] private bool canAttack;
    private int shootAngleHash = 0;
    private Coroutine attackCoroutine;
    private void Awake()
    {
        shootAngleHash = Animator.StringToHash("Shoot");
    }
    public override void EnterState()
    {
        Debug.Log("Bot bắt đầu tấn công!");
        animator.SetBool("isIdle", true);
        canAttack = true;
        attackCoroutine = StartCoroutine(AttackRoutine());
    }
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack && !botNetwork.IsDead)
            {

                SetShootAngle(angleBot);
                yield return WaitSeconds(0.5f);  // Chờ một chút trước khi tấn công
                float totalDamage = 0f;  // Biến để lưu tổng sát thương gây ra
                int numAttacks = Mathf.FloorToInt(BotConfigSO.timeAttack * BotConfigSO.fireRate);  // Số lần bắn trong timeAttack
                muzzle.GetComponent<ParticleSystem>().Play();
                muzzle.SetActive(true);
                for (int i = 0; i < numAttacks; i++)
                {
                    RandomClip();  // Phát âm thanh ngẫu nhiên
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
    public void SetShootAngle(float _shootAngle)
    {
        //animator.Play("Idle");
        animator.SetFloat(shootAngleHash, _shootAngle);
    }
    private void RandomClip()
    {
        if (Random.value <= attackSoundChance)
        {
            int randomIndex = Random.Range(0, shootClip.Length);
            audioSource.PlayOneShot(shootClip[randomIndex]);
        }
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

    public override void UpdateState()
    {

        RotateBody();
    }

    public override void ExitState()
    {
        StopCoroutine(attackCoroutine);
        animator.SetBool("isIdle", false);
        animator.SetBool("isReload", false);
        canAttack = false;
        muzzle.SetActive(false);
        animator.Rebind();
    }

    public override BrcPlayitaState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return BrcPlayitaState.Dead;
        }
        else
        {
            if (!bot.isChangeState && !bot.canSee)
            {
                return BrcPlayitaState.Move;
            }
            else
            {
                return StateKey;
            }
        }
    }
}
