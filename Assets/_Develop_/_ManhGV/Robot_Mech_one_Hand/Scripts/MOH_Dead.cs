using static GameConstants;
using UnityEngine;

public class MOH_Dead : StateBase
{
    public ParticleSystem vfxExplosion;
    
    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashDead);
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
        vfxExplosion.Play();
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotDeath);
        botContext.botNetwork.OnDespawn(4f);
    }
}