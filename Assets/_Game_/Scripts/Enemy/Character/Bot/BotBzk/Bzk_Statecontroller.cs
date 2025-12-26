using static GameConstants;
using UnityEngine;

public class Bzk_Statecontroller : StateControllerBase
{
    [Header("State")]
    public Default_Start startState;
    public DefaultMove moveState;
    public BotBzkAttack attackState;
    public BzkReload reloadState;
    public Default_Dead deadState;
    public Default_DeadExplosion deadExplosionState;
    public BzkIdle bzkIdle;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        startState = GetComponent<Default_Start>();
        moveState = GetComponent<DefaultMove>();
        attackState = GetComponent<BotBzkAttack>();
        reloadState = GetComponent<BzkReload>();
        deadState = GetComponent<Default_Dead>();
        deadExplosionState = GetComponent<Default_DeadExplosion>();
        bzkIdle = GetComponent<BzkIdle>();
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
        bzkIdle.Initialize(EnemyState.Idle, botContext);

        stateController.Add(EnemyState.Start, startState);
        stateController.Add(EnemyState.Idle, bzkIdle);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        stateController.Add(EnemyState.Reload, reloadState);
        stateController.Add(EnemyState.Dead, deadState);
        stateController.Add(EnemyState.DeadExplosion, deadExplosionState);
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
}