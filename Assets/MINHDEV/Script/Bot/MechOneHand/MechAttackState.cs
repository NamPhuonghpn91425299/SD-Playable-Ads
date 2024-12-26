using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotPlayItaStateMachine;
using static MechStateMachine;

public class MechAttackState : BaseState<MechState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] protected MechMoveBase mechMoveBase;
    [SerializeField] protected BotConfigSO  botConfigSO;
    [SerializeField] protected MechAnimEvent  mechAnimEvent;
    public override void EnterState()
    {
        ator.SetBool("IsMove", false);

    }

    public override void ExitState()
    {

    }

    public override MechState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return MechState.Dead;
        }
        else
        {
            return StateKey;
        }
    }

    public override void UpdateState()
    {
        AttackState();
    }

    private void AttackState()
    {
        StartCoroutine(AttackAction());
    }    

    private IEnumerator AttackAction()
    {
        ator.SetBool("IsInAttack", true);
        yield return new WaitUntil(()=> mechAnimEvent.IsInAttack);
        ator.SetBool("IsInAttack", false);
        ator.SetBool("IsOnAttack", true);
        yield return new WaitForSeconds(5f);
        ator.SetBool("IsOnAttack", false);
        ator.SetBool("IsOutAttack", true);
    }    

}
