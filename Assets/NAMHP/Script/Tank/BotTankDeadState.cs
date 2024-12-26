using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotTankStateMachine;
public class BotTankDeadState : BaseState<TankState>
{
    public bool IsUserIconDeadOnBot;
    public Vector3 BotDeadPos;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected GameObject[] deadStep;
    [SerializeField] protected GameObject model;
    [SerializeField] private int currentHealth;
    [SerializeField] private GameObject explosionPrb;
    //[SerializeField] private AnimationClip _expolosion;
    //private Animation animation;

    public override void EnterState()
    {
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        model.SetActive(false);
        deadStep[1].SetActive(true);
        var explosion = ObjectPool.Instance.PopFromPool(explosionPrb, instantiateIfNone: true);
        explosion.transform.position = BotDeadPos;
        StartCoroutine(HideBotOnDie());
        //animation.AddClip(_expolosion,"ex");
        //animation.Play();
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(5f);
        //gameObject.SetActive(false);

    }
    public override void ExitState()
    {
        //ObjectPool.Instance.PushToPool(explosionPrb.GetComponent<IPoolObject>(),gameObject);
    }

    public override void UpdateState()
    {

    }

    public override TankState GetNextState()
    {
        return StateKey;
    }
}
