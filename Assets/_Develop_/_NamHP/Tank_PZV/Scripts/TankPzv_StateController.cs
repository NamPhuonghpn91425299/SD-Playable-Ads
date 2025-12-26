using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
public class TankPzv_StateController: StateControllerBase
{
    [Header("State")]
    public TankPzv_Attack stateAttack;
    public TankPzv_Move stateMove;
    public Vehicle_Dead stateDead;
    
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        stateAttack = GetComponent<TankPzv_Attack>();
        stateMove = GetComponent<TankPzv_Move>();
        stateDead = GetComponent<Vehicle_Dead>();
    }
    #endif
    private void Awake()
    {
        stateAttack.Initialize(EnemyState.Attack, botContext);
        stateMove.Initialize(EnemyState.Move, botContext);
        stateDead.Initialize(EnemyState.Dead, botContext);
        
        stateController.Add(EnemyState.Attack, stateAttack);
        stateController.Add(EnemyState.Move, stateMove);
        stateController.Add(EnemyState.Dead, stateDead);
    }

    public override void OnInit(EnemyState _EnterState)
    {
        stateDead.OnInit();
        stateMove.GetPoint();
        base.OnInit(_EnterState);
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        ChangeState(EnemyState.Move);
    }
}
