using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleShipStateMachine;

public class BattleShipDeadState : BaseState<BattleShipState>
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
        mainPar.startSize = new ParticleSystem.MinMaxCurve(59, 61);
        ex.Explosion();
        //deadStep[0].SetActive(true);
        deadStep[1].SetActive(true);
        StartCoroutine(Hide());
    }

    public override void ExitState()
    {
    }

    public override BattleShipState GetNextState()
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
