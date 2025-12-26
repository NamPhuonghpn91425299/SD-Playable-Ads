using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
public class Bot_Somali04_Attack : StateBase
{

    public override void EnterState()
    {

        Invoke("ChangeState", .1f);
    }

    private void ChangeState()
    {
        //botContext.stateController.ChangeState(EnemyState.Dead);
        TakeDamage();
        if(GameController.Instance.CurrentGameState == GameState.InGame)
        {
            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state:"OnlyDamage"));
        }

    }

    private void TakeDamage()
    {
        var enemyBase = botContext.botNetwork;
        var dame = new DamageInfo()
        {
            damage = enemyBase.currentHealth,
            damageType = DamageType.Normal,

        };
        enemyBase.OnTakeDamage(dame);
    }
    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }
}
