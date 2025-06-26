using System.Collections;
using UnityEngine;
using static BotPlayItaStateMachine;

public class BotPlayItaDeadState : BaseState<PlayItaState>
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;
    [SerializeField] protected GameObject muzzle;
    public bool IsUserIconDeadOnBot;
    public Vector3 BotDeadPos;
    public override void EnterState()
    {
        muzzle.SetActive(false);
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        int indexSound = Random.Range(0, listSounDead.Length);
        AudioClip clipPlay = listSounDead[indexSound];
        //_source.clip = clipPlay;
        _source.PlayOneShot(clipPlay);
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        ator.SetBool("isDead", true);
        //int randomDeadStyle = Random.Range(0, 100);
        //if (randomDeadStyle % 2 == 0)
        //{
        //    ator.SetFloat("DeadStyle", 1);
        //}
        //else
        //{
        //    ator.SetFloat("DeadStyle", 0);
        //}
        StartCoroutine(HideBotOnDie());
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(2f);
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
    public override PlayItaState GetNextState()
    {
        return StateKey;

    }
}
