using System;
using static GameConstants;
using UnityEngine;
using UnityEngine.Serialization;

public class MechOnehand_StateController : StateControllerBase
{
    public MOH_Start startState;
    public MOH_Move moveState;
    public MOH_Attack attackState;
    public MOH_Stun stunState;
    public MOH_Shield shieldState;
    public MOH_Dead deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        startState ??= GetComponent<MOH_Start>();
        moveState ??= GetComponent<MOH_Move>();
        attackState ??= GetComponent<MOH_Attack>();
        stunState ??= GetComponent<MOH_Stun>();
        shieldState ??= GetComponent<MOH_Shield>();
        deadState ??= GetComponent<MOH_Dead>();
    }
#endif
    
    private void Awake()
    {
        startState.Initialize(EnemyState.Start,botContext);
        moveState.Initialize(EnemyState.Move,botContext);
        attackState.Initialize(EnemyState.Attack,botContext);
        stunState.Initialize(EnemyState.Stun,botContext);
        shieldState.Initialize(EnemyState.Shield,botContext);
        deadState.Initialize(EnemyState.Dead,botContext);
        
        stateController.Add(EnemyState.Start,startState);
        stateController.Add(EnemyState.Move,moveState);
        stateController.Add(EnemyState.Attack,attackState);
        stateController.Add(EnemyState.Stun,stunState);
        stateController.Add(EnemyState.Shield,shieldState);
        stateController.Add(EnemyState.Dead,deadState);
    }

    public override void OnInit(EnemyState _EnterState)
    {
        moveState.GetAssignPath();
        base.OnInit(_EnterState);
    }

    public override void ChangeState(EnemyState newAllEnemyState)
    {
        base.ChangeState(newAllEnemyState);
        print("Change State: " + newAllEnemyState);
    }
}
