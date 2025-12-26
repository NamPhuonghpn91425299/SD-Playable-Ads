using static GameConstants;
using UnityEngine;
using Assets._Develop_.ThanhNT.Scripts.Observer;

public class Default_Dead : StateBase
{
    public override void EnterState()
    {

        botContext.ChangeAnimAndType(HashDead);
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotDeath);
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
        botContext.botNetwork.OnDespawn(3f);
    }
}