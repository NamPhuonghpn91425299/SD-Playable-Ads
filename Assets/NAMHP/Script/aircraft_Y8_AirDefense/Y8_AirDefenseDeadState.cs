using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Y8_AirDefenseStateMachine;
public class Y8_AirDefenseDeadState : BaseState<Y8_AirDefense>
{
    [SerializeField] private GameObject _deadStep;
    public override void EnterState()
    {
        BotDeath.Instance.GetBotDeath();
        _deadStep.SetActive(true);
    }

    public override void ExitState()
    {
    
    }


    public override Y8_AirDefense GetNextState()
    {
         return StateKey;
    }

    public override void UpdateState()
    {

    }

}
