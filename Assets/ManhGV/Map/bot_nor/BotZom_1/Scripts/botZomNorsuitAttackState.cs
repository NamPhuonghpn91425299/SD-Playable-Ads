using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class botZomNorsuitAttackState : BaseState<botZomState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] protected HumanMoveBase humanMoveBase;
    [SerializeField] protected ParticleSystem muzzlePS;
    [SerializeField] protected Transform Mytrans;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSoundAttack;
    [SerializeField] private AudioClip[] BotVoice;
    private bool canAttack;
    private bool isTakeDame;
    // private int attackIndex;
    // private float timeAttack;
    
    public override void EnterState()
    {
        int randomSay = Random.Range(0, 100);
        if(randomSay % 2 == 0)
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
        // attackIndex = Random.Range(0, 3);
        // timeAttack = attackIndex == 0 ? 2.28f: attackIndex == 1 ? 1.28f : 2.12f;
        // ator.SetInteger("AttackCombo",attackIndex);
        ator.SetBool("isMoveDone", true);
        canAttack = true;
        StartCoroutine(AttackRoutine());
        botNetwork.OnTakeDamage += OnTakeDame;
       
    }

    private IEnumerator OnPlaySoundAttack()
    {
        yield return new WaitForSeconds(0.5f);
    }

    private void OnTakeDame(int damage)
    {
        isTakeDame = true;
        //ator.SetBool("isHit", true);

    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack && !botNetwork.IsDead)
            {

                _source.Play();  // Phát âm thanh cho mỗi phát bắn
                yield return new WaitForSeconds(1f);  // Chờ 
                
                EventManager.Invoke(EventName.OnTakeDamagePlayer, BotConfigSO.damage);
                EffectUI.Instance.Play();
                
                yield return new WaitForSeconds(1f);  // Chờ 

                //Debug.Log($"Tổng lượng damage bot gây ra: {totalDamage}");

                ator.SetBool("isReload", true);

                yield return new WaitForSeconds(BotConfigSO.timeReload);  // Chờ thời gian nạp đạn
                ator.SetBool("isReload", false);
                canAttack = true;  // Sẵn sàng cho lượt tấn công tiếp theo
            }
            yield return null;  // Chờ cho tới frame kế tiếp
        }
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
        RotaToTarget();
    }
    public override void ExitState()
    {
        _source.Stop();
    }
    public override botZomState GetNextState()
    {
        if (botNetwork.DeadExplosion)
            return botZomState.DeadExplosion;
        else
        {
            if (botNetwork.IsDead)
            {
                return botZomState.Dead;
            }
            else
            {
                return StateKey;
            }
        }
    }
}