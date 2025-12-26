using System.Collections;
using static GameConstants;
using System.Collections.Generic;
using UnityEngine;

public class Capsule_Attack : StateBase
{
    public override void EnterState()
    {
        print("Enter Attack State");
        //TODO: Set trigger attack animation, sound, effects, etc or use coroutine to handle attack logic
        StartCoroutine(IEAttack());
    }

    public override void UpdateState()
    {
        print("Update Attack State");
    }

    public override void ExitState()
    {
        print("Exit Attack State");
    }

    IEnumerator IEAttack()
    {
        botContext.ChangeAnimAndType(HashAttack);// set attack animation = trigger
        yield return HelperCoroutine.GetWait(1.5f);
        botContext.stateController.ChangeState(EnemyState.Dead);// Change to dead state after attack
    }
}