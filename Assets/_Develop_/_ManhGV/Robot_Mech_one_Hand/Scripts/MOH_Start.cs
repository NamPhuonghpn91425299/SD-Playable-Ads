using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
using UnityEngine;

public class MOH_Start : StateBase
{
    public ParticleSystem vfxExplosionJump;
    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashStart);
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

    public override void TriggerCenterAnimation()
    {
        base.TriggerCenterAnimation();
        vfxExplosionJump.Play();
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotDeath);
        EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = .9f,vibrato = 15,randomness = 45}));
    }
}