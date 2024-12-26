using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Y8_AirDefenseStateMachine;
public class Y8_AirDefenseIdleState : BaseState<Y8_AirDefense>
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private BotConfigSO configSO;
    [SerializeField] private float lastTimer;
    public override void EnterState()
    {
        lastTimer = Time.time;
    }

    public override void ExitState()
    {
        
    }

    public override Y8_AirDefense GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return Y8_AirDefense.Dead;
        }
        else
        {
            if (Time.time >= lastTimer + configSO.timeReload)
            {
                return Y8_AirDefense.Glider;
            }
            return StateKey;
        }
    }

    public override void UpdateState()
    {
       
    }


}
