using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BotBigHandDeadState : BaseState<BigHandState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;
    public bool IsUserIconDeadOnBot;
    public Vector3 BotDeadPos;

    public BoxCollider boxCollider;
    public override void EnterState()
    {
        _source.volume = .2f;
        BotDeadPos = this.transform.position;
        //BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        int indexSound = Random.Range(0, listSounDead.Length);
        AudioClip clipPlay = listSounDead[indexSound];

        if (botNetwork.DeadExplosion)
        {
            if(Random.Range(0,50) % 2 == 0)
                _source.PlayOneShot(clipPlay);
        }
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        ator.SetBool("isDead", true);
        StartCoroutine(HideBotOnDie());
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(2f);
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
    public override BigHandState GetNextState()
    {
        return StateKey;
    }
}