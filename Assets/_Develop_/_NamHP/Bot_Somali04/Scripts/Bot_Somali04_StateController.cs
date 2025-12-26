using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
public class Bot_Somali04_StateController : StateControllerBase
{
    [Header("State")]
    public Default_Start startState;
    public DefaultMove moveState;
    public Bot_Somali04_Attack attackState;
    //public Default_Reload reloadState;
    public Bot_Somali04_Dead deadState;
    public Default_DeadExplosion deadExplosionState;
    public Default_DeadExplosionHelicoter deadExplosionHelicoterState;
    public GameObject explosion;
    [Header("Explosion Settings")] 
    [SerializeField] private float radiusExplosion = 5f;
    [SerializeField] private int damageExplosion = 150;
    [SerializeField] private LayerMask layerTargetExplosion;
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        startState = GetComponent<Default_Start>();
        moveState = GetComponent<DefaultMove>();
        attackState = GetComponent<Bot_Somali04_Attack>();
        //reloadState = GetComponent<Default_Reload>();
        deadState = GetComponent<Bot_Somali04_Dead>();
        deadExplosionState = GetComponent<Default_DeadExplosion>();
        deadExplosionHelicoterState = GetComponent<Default_DeadExplosionHelicoter>();
    }
#endif
    private void OnEnable()
    {
        explosion.SetActive(false);
    }

    private void Awake()
    {
        startState.Initialize(EnemyState.Start, botContext);
        moveState.Initialize(EnemyState.Move, botContext);
        attackState.Initialize(EnemyState.Attack, botContext);
        //reloadState.Initialize(EnemyState.Reload, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        deadExplosionState.Initialize(EnemyState.DeadExplosion, botContext);
        deadExplosionHelicoterState.Initialize(EnemyState.DeadExplosionHelicopter, botContext);

        stateController.Add(EnemyState.Start, startState);
        stateController.Add(EnemyState.Idle, startState);
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Attack, attackState);
        //stateController.Add(EnemyState.Reload, reloadState);
        stateController.Add(EnemyState.Dead, deadState);
        stateController.Add(EnemyState.DeadExplosion, deadExplosionState);
        stateController.Add(EnemyState.DeadExplosionHelicopter, deadExplosionHelicoterState);
    }
    
    public override void DeadExplosion()
    {
        base.DeadExplosion();
        if (!canDead)
            return;
        canDead = false;
        ChangeState(EnemyState.DeadExplosion);
    }
    public override void SetupStartState(int _typeStart)
    {
        base.SetupStartState(_typeStart);
        startState._animType = _typeStart;
    }
    public override void CallEndStart()
    {
        base.CallEndStart();
        startState.EndStart();//kết thức start
    }
    protected override void OnDead(bool isDead)
    {
        botContext.botNetwork.ACOnTakeDamage -= OnTakeDame;
        botContext.botNetwork.ACBotDead -= OnDead;
        if (!canDead)
            return;
        canDead = false;
        explosion.SetActive(true);
        ExplosionAndTakeDamageInRadius();
        if (transform.parent != null)
        {
            transform.parent = null;
            ChangeState(EnemyState.DeadExplosionHelicopter);
        }
        else
            ChangeState(EnemyState.Dead);
    }
    public void ExplosionAndTakeDamageInRadius()
    {

        Collider[] cols = Physics.OverlapSphere(transform.position, radiusExplosion, layerTargetExplosion);
        List<Transform> lstRoot = new List<Transform> ();
        
        foreach (Collider col in cols)
            if (!lstRoot.Contains(col.gameObject.transform.root))
                lstRoot.Add(col.gameObject.transform.root);
        
        foreach(var elem in lstRoot)
        {
            ITakeDamage iTakeDamage = elem.gameObject.GetComponentInParent<ITakeDamage>();
            if(iTakeDamage == null)
                iTakeDamage = elem.gameObject.GetComponent<ITakeDamage>();
            
            if (iTakeDamage != null)
            {
                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Explosion,
                    damage = damageExplosion,
                    posExplosion = transform.position,
                };
                iTakeDamage.OnTakeDamage(damageInfo);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //if (GetTransformCenter() != null)
        Gizmos.DrawWireSphere(transform.position, radiusExplosion);
    }
}
