using System;
using static GameConstants;
using UnityEngine;

public class TankStugIV_StateController : StateControllerBase
{
    [Header("State")]
    public Vehicle_Idle idleState;
    public TankStugIV_Attack attackState;
    public TankStugIV_Move moveState;
    public Vehicle_Dead deadState;
    
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        idleState = GetComponent<Vehicle_Idle>();
        attackState = GetComponent<TankStugIV_Attack>();
        moveState = GetComponent<TankStugIV_Move>();
        deadState = GetComponent<Vehicle_Dead>();
    }
    #endif

    private void Awake()
    {
        idleState.Initialize(EnemyState.Idle,botContext);
        moveState.Initialize(EnemyState.Move,botContext);
        attackState.Initialize(EnemyState.Attack,botContext);
        deadState.Initialize(EnemyState.Dead,botContext);
        
        stateController.Add(EnemyState.Idle, idleState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        stateController.Add(EnemyState.Dead, deadState);
    }
    
    public override void OnInit(EnemyState _EnterState)
    {
        deadState.OnInit();
        moveState.GetPoint();
        base.OnInit(_EnterState);
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        ChangeState(EnemyState.Move);
    }
}