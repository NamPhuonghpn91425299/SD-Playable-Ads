using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBigHandHitState : BaseState<BigHandState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    private float timerState = 4.25f;
    float timer;
    
    public override void EnterState()
    {
        ator.SetBool("Hit",true);
        timer = 0f;
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    public override BigHandState GetNextState()
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
