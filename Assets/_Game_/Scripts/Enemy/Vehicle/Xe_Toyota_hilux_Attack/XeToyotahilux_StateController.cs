using System;
using static GameConstants;
using UnityEngine;

public class XeToyotahilux_StateController : StateControllerBase
{
    public Vehicle_Idle idleState;
    public XeToyotahilux_Move moveState;
    public XeToyotahilux_Attack attackState;
    public Vehicle_Dead deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (idleState == null)
            idleState = GetComponent<Vehicle_Idle>();
        if (moveState == null)
            moveState = GetComponent<XeToyotahilux_Move>();
        if (attackState == null)
            attackState = GetComponent<XeToyotahilux_Attack>();
        if (deadState == null)
            deadState = GetComponent<Vehicle_Dead>();
    }
#endif

    private void Awake()
    {
        idleState.Initialize(EnemyState.Idle, botContext);
        moveState.Initialize(EnemyState.Move, botContext);
        attackState.Initialize(EnemyState.Attack, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        
        stateController.Add(EnemyState.Idle, idleState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        stateController.Add(EnemyState.Dead, deadState);
    }

    private void OnEnable()
    {
        if(moveState)
            //setup banh xe
            moveState.ResetWheelRotations();
        
        if(deadState)
            deadState.OnInit();
    }

    public override void OnInit(EnemyState _EnterState)
    {
        base.OnInit(_EnterState);
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        ChangeState(EnemyState.Move);
    }
}