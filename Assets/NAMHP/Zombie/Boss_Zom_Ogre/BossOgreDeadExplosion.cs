using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOgreDeadExplosion : BaseState<BossOgreState>
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] protected Animator anim;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;
    public Vector3 BotDeadPos;
    public Vector3 posGas;
    private Transform TF;
    [SerializeField] private GameObject botDeathEffect;

    public override void EnterState()
    {
        _source.volume = .1f;
        AchievementEvaluator.instance.ResetKillData();
        AchievementEvaluator.instance._medalsUI.OnGetMedal(4);
        TF = transform;
        botNetwork.OnBotDead.Invoke();
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        AudioClip clipPlay = listSounDead[0];
        _source.PlayOneShot(clipPlay);
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        
        StartCoroutine(HideBotOnDie());
        posGas = botNetwork.posExplosion;

        anim.SetBool("isDead", true);
        //PrintExplosionDirection(TF, posGas);
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
        gameObject.SetActive(false);
        
    }
    
    void PrintExplosionDirection(Transform target, Vector3 explosionCenter)
    {
        Vector3 direction = (target.position - explosionCenter).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0)
            {
                anim.SetTrigger("E_Phai");
                Debug.Log("Phải");
            }
            else
            {
                anim.SetTrigger("E_Trai");
                Debug.Log("Trái");
            }
        }
        else
        {
            if (direction.z > 0)
            {
                anim.SetTrigger("E_Truoc");
                Debug.Log("Trước");
            }
            else
            {
                anim.SetTrigger("E_Sau");
                Debug.Log("Sau");
            }
        }
    }
    
    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        _source.Stop();
    }

    public override BossOgreState GetNextState()
    {
        return StateKey;
    }
}