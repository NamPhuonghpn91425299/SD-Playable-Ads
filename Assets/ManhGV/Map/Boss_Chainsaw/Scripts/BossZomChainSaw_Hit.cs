using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossZomChainSaw_Hit : BaseState<bossChainSawState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip audioClip;
    private bool DoneHit;
    
    public override void EnterState()
    {
        _source.volume = 0.26f;
        
        DoneHit = false;
        botNetwork.ChangeAnim("OnHit");
        StartCoroutine(IEDoneState());
        _source.PlayOneShot(audioClip);
    }

    private IEnumerator IEDoneState()
    {
        yield return new WaitForSeconds(3.01f);
        DoneHit = true;
    }

    public override void UpdateState()
    {
        print("Hit");
    }

    public override void ExitState()
    {
        _source.Stop();
    }

    public override bossChainSawState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return bossChainSawState.Dead;
        }
        else
        {
            if(DoneHit)
                return bossChainSawState.Attack;
            else
                return StateKey;
        }
    }
}