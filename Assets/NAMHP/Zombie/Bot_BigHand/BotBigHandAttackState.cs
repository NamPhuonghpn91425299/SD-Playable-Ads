using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBigHandAttackState : BaseState<BigHandState>
{
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] protected HumanMoveBase humanMoveBase;
    [SerializeField] protected ParticleSystem muzzlePS;
    [SerializeField] protected Transform Mytrans;
    [SerializeField] private float maxDelay = 4f;
    [SerializeField] private float minDelay = 1f;
    private bool canAttack;
    private bool isTakeDame;
    
    
    public override void EnterState()
    {
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
                float delay = Random.Range(minDelay, maxDelay);
                yield return new WaitForSeconds(delay);  // Chờ 
                
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

    }
    public override BigHandState GetNextState()
    {
        if (botNetwork.DeadExplosion)
            return BigHandState.DeadExplosion;
        else
        {
            if (botNetwork.IsDead)
            {
                return BigHandState.Dead;
            }
            else
            {
                return StateKey;
            }
        }
    }
}