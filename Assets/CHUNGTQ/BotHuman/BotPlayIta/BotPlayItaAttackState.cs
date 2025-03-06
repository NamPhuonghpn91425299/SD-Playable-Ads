using System;
using System.Collections;
using UnityEngine;
using static BotPlayItaStateMachine;
using Random = UnityEngine.Random;
using static HelperCoroutine;

public class BotPlayItaAttackState : BaseState<PlayItaState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] protected HumanMoveBase humanMoveBase;
    [SerializeField] protected GameObject muzzle;
    [SerializeField] protected ParticleSystem muzzlePS;
    [SerializeField] protected GameObject weaponRoot;
    [SerializeField] protected Transform Mytrans;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSoundAttack;
    [SerializeField] private AudioClip[] BotVoice;
    [SerializeField] private bool canAttack;
    private bool isTakeDame;
    public override void EnterState()
    {
        int randomSay = Random.Range(0, 100);
        if (randomSay % 2 == 0)
        {
            int indexSound = Random.Range(0, listSoundAttack.Length);
            AudioClip clipPlay = listSoundAttack[indexSound];
            _source.clip = clipPlay;
        }
        //else
        //{
        //    int indexSound = Random.Range(0, BotVoice.Length);
        //    AudioClip clipPlay = BotVoice[indexSound];
        //    _source.clip = clipPlay;
        //}
        ator.SetBool("isMoveDone", true);
        canAttack = true;
        StartCoroutine(AttackRoutine());
        botNetwork.OnTakeDamage += OnTakeDame;

    }

    private IEnumerator OnPlaySoundAttack()
    {
        yield return WaitSeconds(0.5f);
    }

    private void OnTakeDame(int damage)
    {
        isTakeDame = true;
        int randomDeadStyle = Random.Range(0, 100);
        //ator.SetBool("isHit", true);
        muzzle.SetActive(false);

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
                    _source.Play();  // Phát âm thanh cho mỗi phát bắn
                    totalDamage += BotConfigSO.damage;  // Cộng sát thương gây ra vào tổng
                    EventManager.Invoke(EventName.OnTakeDamagePlayer, BotConfigSO.damage);
                    //Debug.Log($"sát thương gây ra: {totalDamage}");
                    yield return WaitSeconds(1f / BotConfigSO.fireRate);  // Chờ theo tốc độ bắn
                }

                //Debug.Log($"Tổng lượng damage bot gây ra: {totalDamage}");

                muzzle.SetActive(false);
                ator.SetBool("isReload", true);
                yield return WaitSeconds(BotConfigSO.timeReload);  // Chờ thời gian nạp đạn
                ator.SetBool("isReload", false);
                canAttack = true;  // Sẵn sàng cho lượt tấn công tiếp theo
            }
            yield return null;  // Chờ cho tới frame kế tiếp
        }
    }

    private void RotaToTarget()
    {
        Vector3 direction = LocalPlayer.Instance.GetLocalPlayer() - weaponRoot.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Mytrans.transform.rotation = rotation;
        weaponRoot.transform.rotation = rotation;

    }
    public override void UpdateState()
    {
        RotaToTarget();
    }
    public override void ExitState()
    {

        canAttack = false;
        _source.Stop();
    }
    public override PlayItaState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return PlayItaState.Dead;
        }
        else
        {
            return StateKey;
        }

    }

    private void OnEnable()
    {
        ator.Rebind();

        canAttack = false;
    }
}
