using System.Collections;
using static GameConstants;
using UnityEngine;

public class Somali_Attack : StateBase
{


    public override void EnterState()
    {
        botContext.botNetwork.RotateToPlayer();
        botContext.ChangeAnimAndType(HashAttack);
        StartCoroutine(IEChangeReloadState());

    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        StopAllCoroutines();
    }

    private IEnumerator IEChangeReloadState()
    {
        yield return HelperCoroutine.GetWait(botContext.animator.GetCurrentAnimatorStateInfo(0).length);
        botContext.stateController.ChangeState(EnemyState.Idle);
    }
}
