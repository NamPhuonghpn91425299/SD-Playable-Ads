using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperCoroutine;
public class BossOgreHitState : BaseState<BossOgreState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private BossOgreAttackState _attackState;
    [SerializeField] private BossOgreMoveState moveState; // Reference đến Move State
    [SerializeField] private float timerState = 3f;
    private int attackIndex;
    private bool isHitDone = false;
    
    public override void EnterState()
    {
        isHitDone = false;
        attackIndex = _attackState.attackIndex;
        if (attackIndex == 0)
        {
            ator.SetBool("IsHit1",true);
        }
        else if (attackIndex == 1)
        {
            ator.SetBool("IsHit2",true);
        }
        StartCoroutine(DelayHit());
    }

    private IEnumerator DelayHit()
    {
        yield return WaitSeconds(timerState);
        if (moveState != null)
        {
            moveState.MoveToNextAttackPoint();
        }
        isHitDone = true;
        ator.SetBool("IsMoveDone", true);
    }
    public override void UpdateState()
    {
        if (attackIndex == 0)
        {
            ator.SetBool("IsHit1",false);
        }
        else if (attackIndex == 1)
        {
            ator.SetBool("IsHit2",false);
        }
    }

    public override void ExitState()
    {
        isHitDone = false;
        //ator.Rebind();
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
                if (isHitDone)
                {
                    return BossOgreState.Move;
                }
                return StateKey;
            }
        }
    }
}
