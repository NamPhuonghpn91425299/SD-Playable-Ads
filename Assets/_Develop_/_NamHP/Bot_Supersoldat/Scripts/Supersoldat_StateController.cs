using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
public class Supersoldat_StateController : StateControllerBase
{
    [Header("State")]
    public Default_Start default_Start;
    public DefaultMove default_Move;
    public Supersoldat_Attack_State supersoldat_Attack;
    public Supersoldat_Attack_Rocket_State supersoldat_Attack_Rocket;
    public Default_Dead default_Dead;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        default_Start = GetComponent<Default_Start>();
        default_Move = GetComponent<DefaultMove>();
        supersoldat_Attack = GetComponent<Supersoldat_Attack_State>();
        supersoldat_Attack_Rocket = GetComponent<Supersoldat_Attack_Rocket_State>();
        default_Dead = GetComponent<Default_Dead>();
    }
#endif

    void Awake()
    {
        default_Start.Initialize(EnemyState.Start, botContext);
        default_Move.Initialize(EnemyState.Move, botContext);
        supersoldat_Attack.Initialize(EnemyState.Attack, botContext);
        supersoldat_Attack_Rocket.Initialize(EnemyState.Idle, botContext);
        default_Dead.Initialize(EnemyState.Dead, botContext);

        stateController.Add(EnemyState.Start, default_Start);
        stateController.Add(EnemyState.Move, default_Move);
        stateController.Add(EnemyState.Attack, supersoldat_Attack);
        stateController.Add(EnemyState.Idle, supersoldat_Attack_Rocket);
        stateController.Add(EnemyState.Dead, default_Dead);
    }
    public override void SetupStartState(int _typeStart)
    {
        base.SetupStartState(_typeStart);
        default_Start._animType = _typeStart;
    }

    public override void DeadExplosion()
    {
        base.DeadExplosion();
        if (!canDead)
            return;
        canDead = false;
        ChangeState(EnemyState.Dead);
    }

    protected override void OnDead(bool isDead)
    {
        botContext.botNetwork.ACOnTakeDamage -= OnTakeDame;
        botContext.botNetwork.ACBotDead -= OnDead;
        if (!canDead)
            return;
        canDead = false;
        if (transform.parent != null)
        {
            transform.parent = null;
            ChangeState(EnemyState.Dead);
        }
        else
            ChangeState(EnemyState.Dead);
    }

}
