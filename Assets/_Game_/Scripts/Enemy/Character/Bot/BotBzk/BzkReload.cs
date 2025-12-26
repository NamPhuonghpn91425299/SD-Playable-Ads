using static GameConstants;
using UnityEngine;
using System.Collections.Concurrent;
using System.Collections;

public class BzkReload : StateBase
{
    [SerializeField] private GameObject BangDan_StandReload;
    [SerializeField] private GameObject BangDan_SitReload;


    public override void EnterState()
    {
        BangDan_SitReload.SetActive(false);
        botContext.ChangeAnimAndType(HashReload);
        BangDan_StandReload.SetActive(true);
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        BangDan_StandReload.SetActive(false);
        BangDan_SitReload.SetActive(false);
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        botContext.stateController.ChangeState(EnemyState.Idle);
        
    }


}