using System.Collections;
using static GameConstants;
using UnityEngine;

public class Capsule_Dead : StateBase
{
    public override void EnterState()
    {
        print("Enter Dead State");
        StartCoroutine(IEAttack());
    }

    public override void UpdateState()
    {
        print("Update Dead State");
    }

    public override void ExitState()
    {
        print("Exit Dead State");
    }

    IEnumerator IEAttack()
    {
        botContext.ChangeAnimAndType(HashDead);
        yield return HelperCoroutine.GetWait(1.5f);
        botContext.stateController.ChangeState(EnemyState.Idle);
    }
}