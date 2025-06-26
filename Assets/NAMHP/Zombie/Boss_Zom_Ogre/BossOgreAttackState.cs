using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperCoroutine;
using static NUtiliti;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BossOgreAttackState : BaseState<BossOgreState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] protected HumanMoveBase humanMoveBase;
    [SerializeField] protected Transform Mytrans;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip BotVoice;
    [SerializeField] public GameObject indicatorsAttack1;
    [SerializeField] private BossOgreMoveState moveState; // Reference đến Move State
    [SerializeField] private BossWeakPointManager bossWeakPointManager; // Reference đến BossWeakPointManager
    [SerializeField] private GameObject bloodEffectPrefab; // Hiệu ứng máu
    private bool canAttack;
    private bool isTakeDame;
    private bool isShowAttack1Indicator = false; 
    public Transform playerTarget;                      // Kéo Transform của Player vào đây trong Inspector, hoặc tìm bằng code
    public float rotationSpeed = 3f;                    // Tốc độ xoay (đơn vị: radians/giây hoặc độ/giây tùy cách bạn dùng)
    public bool attackCompleted = false;
    private Coroutine attackCoroutine;
    public readonly int attack1 = Animator.StringToHash("IsAttack1");
    public readonly int attack2 = Animator.StringToHash("IsAttack2");
    public readonly int Attack1_AnimScale = Animator.StringToHash("Attack1_AnimScale");
    [SerializeField] public int attackIndex; 
    [SerializeField] private bool useFixedAttackIndex = false;
    [SerializeField] [Range(0, 1)] private int fixedAttackIndex = 0;
    [SerializeField] private bool isHit;

    public override void EnterState()
    {
        isHit = false;
        attackIndex = useFixedAttackIndex ? fixedAttackIndex : Random.Range(0, 2); // 0 hoặc 1
        //weakPointMonitor.OnAllDetectorsDestroyed += SetBossHit;
        playerTarget = LocalPlayer.Instance.GetTranformPlayer();
        EventManager.AddListener<bool>(EventName.OnRotated, OnRotated);
        bossWeakPointManager.NotifyStateChange(attackIndex);
        bossWeakPointManager.OnDetectorCleared += SetBossHit;
        botNetwork.OnTakeDamage += OnTakeDame;
        isShowAttack1Indicator = false;
        Mytrans = transform;
        attackCompleted = false;
        RotateAndAttack();
    }

    /// <summary>
    /// Được gọi khi tất cả Detector bị phá hủy.
    /// </summary>
    /// <param name="isAllDestroyed">True nếu tất cả Detector đều bị phá hủy.</param>
    private void SetBossHit(bool isAllDestroyed)
    {
        if (isAllDestroyed)
        {
            isHit = isAllDestroyed;
            bloodEffectPrefab.SetActive(false);
            ator.Play("Ogre_Hit");
            _source.PlayOneShot(BotVoice);
            Invoke(nameof(SetAttacked), 3f); // Gọi hàm SetAttacked sau s giây
            Debug.Log("All detectors destroyed, setting boss hit: " + isAllDestroyed);
        }
    }

    private void OnRotated(bool isDone)
    { 
        if (isDone && attackIndex == 0 && !isHit)
        {
            ShowAttack1Indicator();
        }
        Debug.Log("OnRotated called with isDone attackIndex1 : " + isDone);
    }
    public void ShowAttack1Indicator()
    {
        indicatorsAttack1.SetActive(true);
        ator.SetFloat(Attack1_AnimScale, 0.6f);
        Debug.Log("Show Attack 1 Indicator");

    }
    public void SetAttacked()
    {
        attackCompleted = true; // Đánh dấu tấn công đã hoàn thành
        if(isHit)
        {
            ator.Rebind();
        }
        Debug.Log("Boss Ogre attacks with damage: " + BotConfigSO.damage);
        // Thông báo cho Move State chuyển sang điểm attack tiếp theo
        if (moveState != null)
        {
            moveState.MoveToNextAttackPoint();
        }
        
        Debug.Log("Attack completed, moving to next attack point");
    }
    private void OnTakeDame(int damage)
    {
        isTakeDame = true;
    }
    private int GetAttackIndex()
    {
        if (attackIndex == 0)
        {
            Debug.Log("Boss Ogre is attacking with style 1");
            return attack1;
        }
        else
        {
            Debug.Log("Boss Ogre is attacking with style 2");
            return attack2;
        }
    }
    private void RotateAndAttack()
    {
        if(attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(LookAtAndAnimate(
            Mytrans,
            playerTarget,
            ator,
            rotationSpeed,
            GetAttackIndex(),
            canAttack
            ));
    }

    private void RotaToTarget()
    {
        if (LocalPlayer.Instance != null)
        {
            Vector3 direction = LocalPlayer.Instance.GetLocalPlayer() - Mytrans.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);

            Vector3 euler = rotation.eulerAngles;
            euler.x = 0f;
            Mytrans.transform.rotation = Quaternion.Euler(euler);
        }
    }
    
    public override void UpdateState()
    {
        // Kiểm tra nếu đã hoàn thành tấn công  
        //RotaToTarget();
    }
    
    public override void ExitState()
    {
        EventManager.RemoveListener<bool>(EventName.OnRotated, OnRotated);
        bossWeakPointManager.OnDetectorCleared -= SetBossHit;
        botNetwork.OnTakeDamage -= OnTakeDame;
        _source.Stop();
        
        // Dừng coroutine nếu đang chạy
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        
        if(attackIndex == 0)
        {
            ator.SetBool("IsAttack1", false);
        }
        else
        {
            ator.SetBool("IsAttack2", false);
        }

        isHit = false;
    }
    
    public override BossOgreState GetNextState()
    {
        if (botNetwork.DeadExplosion)
            return BossOgreState.DeadExplosion;
        else
        {
            if (botNetwork.IsDead)
            {
                return BossOgreState.Dead;
            }
            else
            {
                // Sau khi hoàn thành tấn công, quay lại Move state để đi đến điểm tiếp theo
                if (attackCompleted)
                {
                    return BossOgreState.Move;
                }
                // else if (isHit)
                // {
                //     // Nếu đã bị hit, có thể chuyển sang trạng thái khác nếu cần
                //     return BossOgreState.Idle;
                // }
                return StateKey;
            }
        }
    }
#if UNITY_EDITOR
[CustomEditor(typeof(BossOgreAttackState))]
public class BossOgreAttackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    
        BossOgreAttackState bossOgreStart = (BossOgreAttackState)target;
        if (GUILayout.Button("Play Attack 1 Animation"))
        {
            bossOgreStart.ator.SetBool("IsAttack1", true);
            bossOgreStart.ator.Play("Ogre_Attack1");
        }
        if (GUILayout.Button("Play Attack 2 Animation"))
        {
            bossOgreStart.ator.SetBool("IsAttack2", true);
            bossOgreStart.ator.Play("Ogre_Attack2");
        }
        if (GUILayout.Button("Stop Animation"))
        {
            bossOgreStart.ator.SetBool("IsAttack1", false);
            bossOgreStart.ator.SetBool("IsAttack2", false);
        }
    }
}
#endif
}


