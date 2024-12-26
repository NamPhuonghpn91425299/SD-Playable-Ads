using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FighterStateMachine;

public class FighterDeadState : BaseState<FighterState>
{
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] GameObject step1;
    [SerializeField] F15TrackingMovement movement;
    [SerializeField] BotAirDead botAirDead;
    public override void EnterState()
    {
        BotDeath.Instance.GetBotDeath();
        botNetwork.Path.IsUse = false;
        movement.enabled = false;
        botAirDead.OnBotDead();
        step1.SetActive(true);
    }
    public override void UpdateState()
    {
        
    }
    public override void ExitState()
    {

    }
    public override FighterState GetNextState()
    {
        return StateKey;

    }
    
}

