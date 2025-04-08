using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BrcPlayitaStateMachine;
public class BrcPlayitaDeadState : BaseState<BrcPlayitaState>,IPoolObject
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSoundDead;
    [SerializeField] private float deathSoundChance = 0.5f;
    public override void EnterState()
    {
        int indexSound = Random.Range(0, listSoundDead.Length);
        AudioClip clipPlay = listSoundDead[indexSound];
        //_source.clip = clipPlay;
        if (Random.value <= deathSoundChance) // Random.value trả về số từ 0 -> 1
        {
            _source.PlayOneShot(clipPlay);
        }
        
        animator.SetBool("isDead", true);
        BotDeath.Instance.GetBotDeath();
        StartCoroutine(HideBotOnDie());
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(2f);
        ObjectPool.Instance.PushToPool(this, gameObject);
        gameObject.SetActive(false);

    }
    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        ResetBot();
        //_source.Stop();
    }

    public override BrcPlayitaState GetNextState()
    {
        return StateKey;
    }
    void ResetBot()
    {
        // Reset bot state when disabled
        BrcPlayitaStateMachine stateMachine = GetComponent<BrcPlayitaStateMachine>();
        stateMachine.ResetBotState();
    }

    public GameObject Prefab { get; set; }
    public void Init()
    {
        
    }

    public void OnPushToPool()
    {
        
    }
}
