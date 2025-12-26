using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechRonin_Dead_State : StateBase
{
    public override void EnterState()
    {
        botContext.SetAnimation(AnimationParameters.Dead);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }
}
