using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossZomChainSaw_Dead : BaseState<bossChainSawState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] private BossZomChainSaw bossZomChainSaw;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioSource _sourceDefault;
    [SerializeField] private AudioClip[] listSounDead;
    [SerializeField] private ParticleSystem vfx_explorionHed;
    public GameObject[] DeadToSetActiveFalse;
    public Vector3 BotDeadPos;
    
    public override void EnterState()
    {
        _source.Stop();
        _sourceDefault.Stop();
        
        AchievementEvaluator.instance.ResetKillData();
        AchievementEvaluator.instance._medalsUI.OnGetMedal(4);
        
        vfx_explorionHed.Play();
        bossZomChainSaw.CheckHitColliderHitExplosion();
        foreach (GameObject VARIABLE in DeadToSetActiveFalse)
            VARIABLE.SetActive(false);
        
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        // AudioClip clipPlay = listSounDead[0];
        // _source.PlayOneShot(clipPlay);
        
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        
        botNetwork.ChangeAnim("Death");
        
        StartCoroutine(HideBotOnDie());
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }

    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        
    }
    public override bossChainSawState GetNextState()
    {
        return StateKey;
    }
}