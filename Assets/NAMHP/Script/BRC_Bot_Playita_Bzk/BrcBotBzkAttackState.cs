using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperCoroutine;
using static BrcBotBzkStateMachine;
public class BrcBotBzkAttackState : BaseState<BrcBotBzkState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BotAI bot;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] shootClip;
    [SerializeField] private GameObject bulletRPG7Prefab;
    [SerializeField] private GameObject bulletRPG7;
    [SerializeField] private float bodyRotationSpeed = 5f; // Xoay ngang
    //[SerializeField] private float upperBodyRotationSpeed = 5f; // Xoay lên/xuống
    [SerializeField] private Animator animator;
    [SerializeField] protected GameObject muzzle;
    [SerializeField] private ParticleSystem muzzleParticle;
    [SerializeField] private float angleBot = 10f;
    [SerializeField] private float attackSoundChance = 0.5f;
    [SerializeField] private bool canAttack;
    [SerializeField] private int numAttacks;
    public float trajectorySpreadRadius = 1f; // Bán kính tối đa lệch điểm đến so với mục tiêu
    public Transform target;
    private int shootAngleHash = 0;
    private Coroutine attackCoroutine;
    [SerializeField] private Vector3 calculatedTargetPos;
    [Header("ROCKET ATTRIBUTES")]
    public float rocketSpeed = 50f;
    public float rocketRotationSpeed = 180f; // Tốc độ xoay khi bám đuổi/điều chỉnh hướng
    public float autoExplodeTime = 10f;
    public float explosionRadius = 5f;
    public int damage = 10;
    public float initialStraightDistance = 15f; // Khoảng cách rocket bay thẳng ban đầu
    
    private void Awake()
    {
        shootAngleHash = Animator.StringToHash("Shoot");
        if (target == null)
        {
            target = LocalPlayer.Instance.GetTranExplosion();
        }
    }
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack && !botNetwork.IsDead)
            {
                SetShootAngle(angleBot);
                yield return WaitSeconds(0.5f);  // Chờ một chút trước khi tấn công
                numAttacks = Mathf.FloorToInt(BotConfigSO.timeAttack * BotConfigSO.fireRate);  // Số lần bắn trong timeAttack

                for (int i = 0; i < numAttacks; i++)
                {
                    Vector3 targetOffset = Random.insideUnitSphere * trajectorySpreadRadius;
                    targetOffset.y = Mathf.Abs(targetOffset.y) * 0.2f; // Giảm lệch theo chiều dọc nếu muốn
                    calculatedTargetPos = target.position + targetOffset; // Điểm đến đã tính toán (lệch đi)
                    bulletRPG7.SetActive(false);
                    muzzleParticle.gameObject.SetActive(true);
                    muzzleParticle.Play();
                    audioSource.Play();  // Phát âm thanh cho mỗi phát bắn
                    var rpgRocket = ObjectPool.Instance.PopFromPool(bulletRPG7Prefab, instantiateIfNone: true);
                    rpgRocket.transform.SetPositionAndRotation(muzzle.transform.position, muzzle.transform.rotation);
                    rpgRocket.SetActive(true);
                    VolleyRocketMovement rocket = rpgRocket.GetComponent<VolleyRocketMovement>();
                    // Thiết lập thông số cho tên lửa
                    rocket.Setup(
                        target,
                        calculatedTargetPos, // Điểm đến đã tính toán (lệch đi)
                        rocketSpeed,
                        rocketRotationSpeed,
                        initialStraightDistance,
                        autoExplodeTime,
                        damage,
                        explosionRadius
                        // , explosionAttrib // Nếu dùng cấu trúc ExplosionAttribute
                    );
                    //Debug.Log($"sát thương gây ra: {totalDamage}");
                    yield return WaitSeconds(1f / BotConfigSO.fireRate);  // Chờ theo tốc độ bắn
                }

                //Debug.Log($"Tổng lượng damage bot gây ra: {totalDamage}");

                //muzzle.SetActive(false);
                animator.SetBool("isReload", true);
                yield return WaitSeconds(BotConfigSO.timeReload);  // Chờ thời gian nạp đạn
                animator.SetBool("isReload", false);
                canAttack = true;  // Sẵn sàng cho lượt tấn công tiếp theo
            }
            yield return null;  // Chờ cho tới frame kế tiếp
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
    public void SetShootAngle(float _shootAngle)
    {
        //animator.Play("Idle");
        animator.SetFloat(shootAngleHash, _shootAngle);
    }
    public override void EnterState()
    {
        animator.SetBool("isIdle", true);
        canAttack = true;
        attackCoroutine = StartCoroutine(AttackRoutine());
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
        //muzzle.SetActive(false);
        animator.Rebind();
    }

    public override BrcBotBzkState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return BrcBotBzkState.Dead;
        }
        else
        {
            if (!bot.isChangeState && !bot.canSee)
            {
                return BrcBotBzkState.Move;
            }
            else
            {
                return StateKey;
            }
        }
    }
}
