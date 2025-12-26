using static GameConstants;
using UnityEngine;

public class Default_Reload : StateBase
{
    [SerializeField] private Default_Attack _defaultAttack;
    [SerializeField] private GameObject BangDan_StandReload;
    [SerializeField] private GameObject BangDan_SitReload;
    
    public override void EnterState()
    {
        BangDan_StandReload.SetActive(false);
        BangDan_SitReload.SetActive(false);
        botContext.ChangeAnimAndType(HashReload);
        if (_defaultAttack.animType == 0)
            BangDan_StandReload.SetActive(true);
        else
            BangDan_SitReload.SetActive(true);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
        
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        botContext.stateController.ChangeState(EnemyState.Attack);
    }
}