using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AirCraftStateMachine;
using static HelicopterStateMachine;

public class HelicopterDeadState : BaseState<HelicopterState>
{
    public Vector3 BotDeadPos;
    [SerializeField] protected BotNetwork botNetwork;
    [SerializeField] protected GameObject[] deadStep;
    [SerializeField] protected GameObject model;
    [SerializeField] private GameObject explosionPrb;
    public override void EnterState()
    {
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        model.SetActive(false);
        var obj = ObjectPool.Instance.PopFromPool(explosionPrb, instantiateIfNone: true);
        obj.transform.SetPositionAndRotation(transform.position, explosionPrb.transform.rotation);
        ExplosionRocket ex = obj.GetComponent<ExplosionRocket>();
        var mainPar = ex.ExplosionEffect.main;
        mainPar.startSize = new ParticleSystem.MinMaxCurve(13, 15);
        ex.Explosion();
        deadStep[0].SetActive(true);
        StartCoroutine(Hide());
    }

    public override void ExitState()
    {
    }

    public override HelicopterState GetNextState()
    {
        return StateKey;
    }

    public override void UpdateState()
    {

    }
    IEnumerator Hide()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }
}
