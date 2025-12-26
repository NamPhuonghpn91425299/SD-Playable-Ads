using System;
using static GameConstants;
using UnityEngine;

[RequireComponent(typeof(Capsule_Network),typeof(Capsule_StateController),typeof(Capsule_Idle))]
[RequireComponent(typeof(Capsule_Move),typeof(Capsule_Attack),typeof(Capsule_Dead))]
public class Capsule_StateController : StateControllerBase
{
    [HideInInspector] public Capsule_Idle _idle;
    [HideInInspector] public Capsule_Move _moveState;
    [HideInInspector] public Capsule_Attack _attackState;
    [HideInInspector] public Capsule_Dead _deadState;
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        _idle = GetComponent<Capsule_Idle>();
        _moveState = GetComponent<Capsule_Move>();
        _attackState = GetComponent<Capsule_Attack>();
        _deadState = GetComponent<Capsule_Dead>();  
    }
#endif
    private void Awake()
    {
        _idle.Initialize(EnemyState.Idle, botContext);
        _moveState.Initialize(EnemyState.Move, botContext);
        _attackState.Initialize(EnemyState.Attack, botContext);
        _deadState.Initialize(EnemyState.Dead, botContext);

        stateController.Add(EnemyState.Idle, _idle);
        stateController.Add(EnemyState.Move, _moveState);
        stateController.Add(EnemyState.Attack, _attackState);
        stateController.Add(EnemyState.Dead, _deadState);
    }

    private void Start()
    {
        OnInit(EnemyState.Idle);
    }

    // private void ZombieDeadExplosion(bool obj)
    // {
    //     if (!canDead)
    //         return;
    //     canDead = false;
    //     //ChangeState(ZomAllState.DeadExplosion);
    // }
}