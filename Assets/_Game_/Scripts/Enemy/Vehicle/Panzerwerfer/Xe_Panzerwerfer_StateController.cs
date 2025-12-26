using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class Xe_Panzerwerfer_StateController : StateControllerBase
{
    public Panzerwerfer_Idle idleState;
    public Panzerwerfer_Move moveState;
    public Panzerwerfer_Attack attackState;
    public Panzerwerfer_Death deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (idleState == null)
            idleState = GetComponent<Panzerwerfer_Idle>();
        if (moveState == null)
            moveState = GetComponent<Panzerwerfer_Move>();
        if (attackState == null)
            attackState = GetComponent<Panzerwerfer_Attack>();
        if (deadState == null)
            deadState = GetComponent<Panzerwerfer_Death>();
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

    public override void OnInit(EnemyState _EnterState)
    {
        deadState.OnInit();
        base.OnInit(_EnterState);
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        ChangeState(EnemyState.Move);
    }
}
