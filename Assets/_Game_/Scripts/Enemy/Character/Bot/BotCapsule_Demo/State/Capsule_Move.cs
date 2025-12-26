using System.Collections;
using static GameConstants;
using UnityEngine;

public class Capsule_Move : StateBase
{
    public override void EnterState()
    {
        print("Enter Move State");
        StartCoroutine(IEAttack());
    }

    public override void UpdateState()
    {
        print("Update Move State");
    }

    public override void ExitState()
    {
        print("Exit Move State");
    }

    IEnumerator IEAttack()
    {
        botContext.ChangeAnimAndType(HashMove);
        yield return HelperCoroutine.GetWait(1.5f);
        botContext.stateController.ChangeState(EnemyState.Attack);
    }
}