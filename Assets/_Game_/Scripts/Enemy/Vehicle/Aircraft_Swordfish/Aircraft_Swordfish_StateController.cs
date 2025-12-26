using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GameConstants;

public class Aircraft_Swordfish_StateController : StateControllerBase
{

    public Aircraft_Swordfish_Move moveState;
    public Aircraft_Swordfish_Attack attackState;
    public Vehicle_DeathExplosion deadState;
    public GameObject effectWarning;



#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (moveState == null)
            moveState = GetComponent<Aircraft_Swordfish_Move>();
        if (attackState == null)
            attackState = GetComponent<Aircraft_Swordfish_Attack>();
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
        deadState.OnInit();
        base.OnInit(_EnterState);
    }

    protected override void Update()
    {
        base.Update();
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerInstant.Instance.transform.position);
        if (distanceToPlayer <= attackState.distanceToFire)
        {
            ChangeState(EnemyState.Attack);
           
        }
       
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        ChangeState(EnemyState.Move);
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

    protected override void OnDead(bool isDead)
    {
        base.OnDead(isDead);
        botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotDeath);
        effectWarning.SetActive(false);
    }
    

}
