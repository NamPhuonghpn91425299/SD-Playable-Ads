using static GameConstants;
using UnityEngine;

public class MOH_Stun : StateBase
{
    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashStun);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        botContext.stateController.ChangeState(EnemyState.Move);
    }
}