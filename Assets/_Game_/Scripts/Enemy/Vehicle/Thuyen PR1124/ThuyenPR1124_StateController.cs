using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class ThuyenPR1124_StateController : StateControllerBase
{
    [Header("State")]
    public Vehicle_Idle idleState;
    public ThuyenPR112_Move moveState;
    public ThuyenPR1124_Attack attackState;
    public Vehicle_DeathExplosion deadState;
    public GameObject effectWarning;


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (idleState == null)
            idleState = GetComponent<Vehicle_Idle>();
        if (moveState == null)
            moveState = GetComponent<ThuyenPR112_Move>();
        if (attackState == null)
            attackState = GetComponent<ThuyenPR1124_Attack>();
        if (deadState == null)
            deadState = GetComponent<Vehicle_DeathExplosion>();
    }
#endif

    private void Awake()
    {
        moveState.Initialize(EnemyState.Move, botContext);
        attackState.Initialize(EnemyState.Attack, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);


        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        stateController.Add(EnemyState.Dead, deadState);
    }

    public override void OnInit(EnemyState _EnterState)
    {
        moveState.OnInitState();
        base.OnInit(_EnterState);

    }

    protected override void OnDead(bool isDead)
    {
        base.OnDead(isDead);
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotDeath);
        effectWarning.SetActive(false);

    }
    
     protected override void OnTakeDame(DamageInfo _damageInfo)
    {
        base.OnTakeDame(_damageInfo);
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.GetHit);
        if (botContext.botNetwork.currentHealth <= botContext.botNetwork.MaxHealth / 2)
        {
            effectWarning.SetActive(true);
        }
    }
}
