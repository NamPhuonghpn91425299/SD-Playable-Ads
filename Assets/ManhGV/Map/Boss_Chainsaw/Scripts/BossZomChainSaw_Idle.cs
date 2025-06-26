using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class BossZomChainSaw_Idle : BaseState<bossChainSawState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BotConfigSO botConfigSo;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;

    private bool doneIdle;
    
    public override void EnterState()
    {
        doneIdle = false;
        botNetwork.ChangeAnim("Idle");
        StartCoroutine(IEDoneState());
        _source.Play();
    }

    public IEnumerator IEDoneState()
    {
        yield return new WaitForSeconds(botConfigSo.timeReload);
        doneIdle = true;
    }
    
    public override void UpdateState()
    {
        
    }
    
    public override void ExitState()
    {
        
    }
    
    public override bossChainSawState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return bossChainSawState.Dead;
        }
        else
        {
            if (doneIdle)
                return bossChainSawState.Attack;
            else
                return StateKey;
        }
    }
}