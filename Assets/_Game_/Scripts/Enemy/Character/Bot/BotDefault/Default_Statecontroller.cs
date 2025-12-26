using System;
using static GameConstants;
using UnityEngine;

public class Default_Statecontroller : StateControllerBase
{
    [Header("State")]
    public Default_Start startState;
    public DefaultMove moveState;
    public Default_Attack attackState;
    public Default_Reload reloadState;
    public Default_Dead deadState;
    public Default_DeadExplosion deadExplosionState;
    public Default_DeadExplosionHelicoter deadExplosionHelicoterState;
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        startState = GetComponent<Default_Start>();
        moveState = GetComponent<DefaultMove>();
        attackState = GetComponent<Default_Attack>();
        reloadState = GetComponent<Default_Reload>();
        deadState = GetComponent<Default_Dead>();
        deadExplosionState = GetComponent<Default_DeadExplosion>();
        deadExplosionHelicoterState = GetComponent<Default_DeadExplosionHelicoter>();
    }
#endif
    private void Awake()
    {
        startState.Initialize(EnemyState.Start, botContext);
        moveState.Initialize(EnemyState.Move, botContext);
        attackState.Initialize(EnemyState.Attack, botContext);
        reloadState.Initialize(EnemyState.Reload, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        deadExplosionState.Initialize(EnemyState.DeadExplosion, botContext);
        deadExplosionHelicoterState.Initialize(EnemyState.DeadExplosionHelicopter, botContext);

        stateController.Add(EnemyState.Start, startState);
        stateController.Add(EnemyState.Idle, startState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        stateController.Add(EnemyState.Reload, reloadState);
        stateController.Add(EnemyState.Dead, deadState);
        stateController.Add(EnemyState.DeadExplosion, deadExplosionState);
        stateController.Add(EnemyState.DeadExplosionHelicopter, deadExplosionHelicoterState);
    }

    public override void DeadExplosion()
    {
        base.DeadExplosion();
        if (!canDead)
            return;
        canDead = false;
        ChangeState(EnemyState.DeadExplosion);
    }

    public override void SetupStartState(int _typeStart)
    {
        base.SetupStartState(_typeStart);
        startState._animType = _typeStart;
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        startState.EndStart();//kết thức start
    }

    protected override void OnDead(bool isDead)
    {
        botContext.botNetwork.ACOnTakeDamage -= OnTakeDame;
        botContext.botNetwork.ACBotDead -= OnDead;
        if (!canDead)
            return;
        canDead = false;
        if (transform.parent != null)
        {
            transform.parent = null;
            ChangeState(EnemyState.DeadExplosionHelicopter);
        }
        else
            ChangeState(EnemyState.Dead);
    }
}