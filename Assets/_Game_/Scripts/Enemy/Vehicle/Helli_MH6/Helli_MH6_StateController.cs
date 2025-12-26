using System;
using DG.Tweening;
using static GameConstants;
using UnityEngine;

public class Helli_MH6_StateController : StateControllerBase
{
    [Header("State")] public Helli_MH6_Idle idleState;
    public Helli_MH6_Move moveState;
    public Helli_MH6_DropTroops dropTroopsState;
    public Vehicle_Dead deadState;

    [Header("Cho cánh quạt máy bay vào đây!")] [SerializeField]
    Transform[] rotorBlades;

    [SerializeField] private float[] rotateSpeed;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        idleState = GetComponent<Helli_MH6_Idle>();
        moveState = GetComponent<Helli_MH6_Move>();
        dropTroopsState = GetComponent<Helli_MH6_DropTroops>();
        deadState = GetComponent<Vehicle_Dead>();
    }
#endif

    private void Awake()
    {
        idleState.Initialize(EnemyState.Idle, botContext);
        moveState.Initialize(EnemyState.Move, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        dropTroopsState.Initialize(EnemyState.DropTroops, botContext);

        stateController.Add(EnemyState.Idle, idleState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.DropTroops, dropTroopsState);
        stateController.Add(EnemyState.Dead, deadState);
    }

    public override void OnInit(EnemyState _EnterState)
    {
        deadState.ResetExplosionParts();
        moveState.OnInitState();
        moveState.GetPoint();
        base.OnInit(_EnterState);
        dropTroopsState.SpawnBots();    
    }

    private void FixedUpdate()
    {
        RotateRotorBlades();
    }

    public void RotateRotorBlades()
    {
        rotorBlades[0].Rotate(Vector3.up,  rotateSpeed[0]);
        rotorBlades[1].Rotate(Vector3.right, rotateSpeed[1]);
    }

    protected override void OnDead(bool isDead)
    {
        dropTroopsState.CallbotEqualsNull_IfCanoDead();
        if(moveState.currentPathTween!=null)
            moveState.currentPathTween.Kill();
        if(moveState.speedControlTween!=null)
            moveState.speedControlTween.Kill();
        base.OnDead(isDead);
    }
}