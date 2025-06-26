using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossZomChainSaw_Attack : BaseState<bossChainSawState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BossZomChainSaw thisBoss;
    [SerializeField] AnimConfig animConfig;
    [SerializeField] private Animator anim;
    [SerializeField] protected Transform Mytrans;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSoundAttack;
    [SerializeField] private AudioClip[] BotVoice;
    private bool canAttack;
    private AnimStruct animStruct;

    private Coroutine AttackCoutine;
    
    private bool attackDone;
    private bool isHit;
    
    public override void EnterState()
    {
        Init();
        _source.volume = 0.26f;
    }

    private void OnEnable()
    {
        botNetwork.OnTakeDamagePlayer += OnTakeDame;
    }

    private void OnDisable()
    {
        botNetwork.OnTakeDamagePlayer -= OnTakeDame;
    }

    private void Init()
    {
        attackDone = false;
        isHit = false;
        
        int randomAnim = Random.Range(0, animConfig.anims.Count);
        animStruct = animConfig.anims[randomAnim];
        
        botNetwork.ChangeAnim("Attack");
        anim.SetInteger("AttackStyle", animStruct.style);
        
        //AudioClip clipPlay = listSoundAttack[animStruct.style];
        //.clip = clipPlay;
        
        _source.PlayOneShot(listSoundAttack[0]);
        
        AttackCoutine = StartCoroutine(IEAttack());
    }

    private void OnTakeDame(int damage)
    { 
        thisBoss.PlusBulletToHit();
        if (thisBoss._currentState.StateKey == bossChainSawState.Attack || thisBoss._currentState.StateKey == bossChainSawState.Move)
        {
            if (thisBoss.CanHit())
            {
                StopCoroutine(AttackCoutine);
                isHit = true;
                _source.Stop();
            }
        }
    }

    private IEnumerator IEAttack()
    {
        //_source.Play();  // Phát âm thanh cho mỗi phát bắn
        RotaToTarget();
        yield return new WaitForSeconds(animStruct.timerTakeDamage);
        
        EffectUI.Instance.Play();
        
        if (!botNetwork.IsDead)
        {
            EventManager.Invoke(EventName.OnTakeDamagePlayer, BotConfigSO.damage);
        }
        
        yield return new WaitForSeconds(animStruct.timerEndAnim-animStruct.timerTakeDamage);
        attackDone = true;
    }

    private void RotaToTarget()
    {
        Vector3 direction = LocalPlayer.Instance.GetLocalPlayer() - Mytrans.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);

        Vector3 euler = rotation.eulerAngles;
        euler.x = 0f;
        Mytrans.transform.rotation = Quaternion.Euler(euler);
    }
    
    public override void UpdateState()
    {
        
    }
    
    public override void ExitState()
    {
        _source.Stop();
    }
    
    public override bossChainSawState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return bossChainSawState.Dead;
        }
        else
        {
            if (isHit)
                return bossChainSawState.Hit;
            else if (attackDone)
            {
                if (Random.Range(0, 50) % 2 == 0)
                    return bossChainSawState.Move;
                else
                    return bossChainSawState.Idle;
            }
            else
                return StateKey;
        }
    }
}