using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankWanze_StateController : StateControllerBase
{
    public TankWanze_Attack attackState;
    public Tank_Move moveState;
    public Vehicle_Dead deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        attackState ??= GetComponent<TankWanze_Attack>();
        moveState ??= GetComponentInParent<Tank_Move>();
        deadState ??= GetComponentInParent<Vehicle_Dead>();
    }
#endif

    private void Awake()
    {
        moveState.Initialize(GameConstants.EnemyState.Move,botContext);
        attackState.Initialize(GameConstants.EnemyState.Attack, botContext);
        deadState.Initialize(GameConstants.EnemyState.Dead,botContext);
        
        stateController.Add(GameConstants.EnemyState.Move,moveState);
        stateController.Add(GameConstants.EnemyState.Attack, attackState);
        stateController.Add(GameConstants.EnemyState.Dead,deadState);
    }

    public override void OnInit(GameConstants.EnemyState _EnterState)
    {
        deadState.OnInit();
        moveState.GetPoint();
        base.OnInit(_EnterState);
    }
}