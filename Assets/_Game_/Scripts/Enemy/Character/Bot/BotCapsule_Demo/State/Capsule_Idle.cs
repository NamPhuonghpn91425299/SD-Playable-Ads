using System.Collections;
using static GameConstants;
using UnityEngine;

public class Capsule_Idle : StateBase
{
    public override void EnterState()
    {
        print("Enter Idle State");
        StartCoroutine(IEAttack());
    }

    public override void UpdateState()
    {
        print("Update Idle State");
    }

    public override void ExitState()
    {
        print("Exit Idle State");
    }

    IEnumerator IEAttack()
    {
        botContext.ChangeAnimAndType(HashIdle);
        yield return HelperCoroutine.GetWait(1.5f);
        botContext.stateController.ChangeState(EnemyState.Move);
    }
}