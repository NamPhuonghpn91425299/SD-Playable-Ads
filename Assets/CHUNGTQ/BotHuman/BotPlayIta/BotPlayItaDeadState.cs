using System.Collections;
using UnityEngine;
using static BotPlayItaStateMachine;

public class BotPlayItaDeadState : BaseState<PlayItaState>, IPoolObject
{
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected Animator ator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;
    [SerializeField] protected GameObject muzzle;
    public bool IsUserIconDeadOnBot;
    public Vector3 BotDeadPos;
    [SerializeField] bool iscanMove = false;
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
        //iscanMove = true;
        ObjectPool.Instance.PushToPool(this, gameObject);
        //gameObject.SetActive(false);

    }

    public override void UpdateState()
    {

    }
    public override void ExitState()
    {
        iscanMove = false;
        _source.Stop();
        // _source.clip = null; // Reset clip về null để đảm bảo không tái sử dụng
    }
    public override PlayItaState GetNextState()
    {
        // if (iscanMove)
        // {
        //     return PlayItaState.Move;
        // }
        //else
        {
            return StateKey;
        }
    }

    public GameObject Prefab { get; set; }
    public void Init()
    {

    }

    public void OnPushToPool()
    {
        // Reset bot state when pushed to pool
        BotPlayItaStateMachine stateMachine = GetComponent<BotPlayItaStateMachine>();
        stateMachine.ResetBotState();
    }
}
