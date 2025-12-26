using System;
using static GameConstants;
using UnityEngine;

public class ThuyenHiggins_StateController : StateControllerBase
{
    [Header("State")]
    public Vehicle_Idle idleState;
    public ThuyenHiggins_Move moveState;
    public ThuyenHiggins_DropVehicle dropVehicleState;
    public ThuyenHiggins_Dead deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (idleState == null)
            idleState = GetComponent<Vehicle_Idle>();
        if (moveState == null)
            moveState = GetComponent<ThuyenHiggins_Move>();
        if (dropVehicleState == null)
            dropVehicleState = GetComponent<ThuyenHiggins_DropVehicle>();
        if (deadState == null)
            deadState = GetComponent<ThuyenHiggins_Dead>();
    }
#endif

    private void Awake()
    {
        moveState.Initialize(EnemyState.Move,botContext);
        dropVehicleState.Initialize(EnemyState.DropTroops, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.DropTroops, dropVehicleState);
        stateController.Add(EnemyState.Dead, deadState);
    }

    public override void OnInit(EnemyState _EnterState)
    {
        moveState.OnInitState();
        base.OnInit(_EnterState);
        dropVehicleState.SpawnVehical();
    }

    protected override void OnDead(bool isDead)
    {
        base.OnDead(isDead);
        dropVehicleState.DeadAllEndVehical();//nếu xe chưa được đưa ra khỏi thuyền thì sẽ bị hủy -> xe nổ
    }
}