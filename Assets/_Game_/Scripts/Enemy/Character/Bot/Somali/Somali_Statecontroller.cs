using static GameConstants;
using UnityEngine;

public class Somali_Statecontroller : StateControllerBase
{
    [Header("State")]
    public Default_Start startState;
    public DefaultMove moveState;
    public Somali_Attack attackState;
    public Default_Dead deadState;
    public Default_DeadExplosion deadExplosionState;
    public Somali_Idle idleState;


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        startState = GetComponent<Default_Start>();
        moveState = GetComponent<DefaultMove>();
        attackState = GetComponent<Somali_Attack>();
        deadState = GetComponent<Default_Dead>();
        deadExplosionState = GetComponent<Default_DeadExplosion>();
        idleState = GetComponent<Somali_Idle>();

    }
#endif
    private void Awake()
    {
        startState.Initialize(EnemyState.Start, botContext);
        moveState.Initialize(EnemyState.Move, botContext);
        attackState.Initialize(EnemyState.Attack, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        deadExplosionState.Initialize(EnemyState.DeadExplosion, botContext);
        idleState.Initialize(EnemyState.Idle, botContext);


        stateController.Add(EnemyState.Start, startState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        stateController.Add(EnemyState.Dead, deadState);
        stateController.Add(EnemyState.DeadExplosion, deadExplosionState);
        stateController.Add(EnemyState.Idle, idleState);
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