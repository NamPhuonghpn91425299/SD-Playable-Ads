using System;
using static GameConstants;
using UnityEngine;

public class XeM113_StateController : StateControllerBase
{
    [Header("State")] 
    public Vehicle_Idle idleState;
    public XeM113_Move moveState;
    public XeM113_DropTroops dropTroopsState;
    public Vehicle_Dead deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        idleState = GetComponent<Vehicle_Idle>();
        moveState = GetComponent<XeM113_Move>();
        dropTroopsState = GetComponent<XeM113_DropTroops>();
        deadState = GetComponent<Vehicle_Dead>();
    }
#endif

    private void Awake()
    {
        idleState.Initialize(EnemyState.Idle,botContext);
        moveState.Initialize(EnemyState.Move,botContext);
        dropTroopsState.Initialize(EnemyState.DropTroops,botContext);
        deadState.Initialize(EnemyState.Dead,botContext);
        
        stateController.Add(EnemyState.Idle, idleState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.DropTroops, dropTroopsState);
        stateController.Add(EnemyState.Dead, deadState);
    }

    public override void CallEndStart()
    {
        base.CallEndStart();
        ChangeState(EnemyState.Move);
    }

    private void OnEnable()
    {
        deadState.OnInit();
    }
}