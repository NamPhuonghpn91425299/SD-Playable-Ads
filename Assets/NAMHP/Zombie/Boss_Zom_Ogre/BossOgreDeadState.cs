using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BossOgreDeadState : BaseState<BossOgreState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;
    [SerializeField] private GameObject botDeathEffect;
    public bool IsUserIconDeadOnBot;
    public Vector3 BotDeadPos;
    
    public override void EnterState()
    {
        _source.volume = .2f;
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        int indexSound = Random.Range(0, listSounDead.Length);
        AudioClip clipPlay = listSounDead[indexSound];
        AchievementEvaluator.instance.ResetKillData();
        AchievementEvaluator.instance._medalsUI.OnGetMedal(4);
        if (botNetwork.DeadExplosion)
        {
            if(Random.Range(0,50) % 2 == 0)
                _source.PlayOneShot(clipPlay);
        }
        else
            _source.PlayOneShot(clipPlay);
        
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        ator.SetBool("isDead", true);

        StartCoroutine(HideBotOnDie());
    }
    private void SpawnBotDeathEffect()
    {
        if (botDeathEffect != null)
        {
            GameObject effectInstance = ObjectPool.Instance.PopFromPool(botDeathEffect, instantiateIfNone: true);
            effectInstance.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            effectInstance.SetActive(true); 
        }
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(2f);
        SpawnBotDeathEffect();
        _source.volume = .3f;
        gameObject.SetActive(false);
    }

    public override void UpdateState()
    {

    }
    public override void ExitState()
    {
        _source.Stop();
        // _source.clip = null; // Reset clip về null để đảm bảo không tái sử dụng
    }
    public override BossOgreState GetNextState()
    {
        return StateKey;
    }
}