using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum bossChainSawState
{
    Idle,
    Attack,
    Hit,
    Dead,
    Move,
    Run
}

public class BossZomChainSaw : MonoBehaviour
{
    public Dictionary<bossChainSawState, BaseState<bossChainSawState>> stateController = new Dictionary<bossChainSawState, BaseState<bossChainSawState>>();
    public BaseState<bossChainSawState> _currentState;
    private bool _isTransition;

    [Header("Hit")] 
    [SerializeField]private int countBulletToHit;
    [SerializeField]private int countBullet;
    
    BossZomChainSaw_Idle _idleState;
    BossZomChainSaw_Move _moveState;
    BossZomChainSaw_Attack _attackState;
    bossChainSawState_Run _runState;
    BossZomChainSaw_Hit _hitState;
    BossZomChainSaw_Dead _deadState;
    
    [Header("Explosion")]
    public Transform _centerExplosion;
    public float _radiusExplosion;
    public LayerMask _layerHit;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _idleState = GetComponent<BossZomChainSaw_Idle>();
        _idleState.Initialize(bossChainSawState.Idle);
        
        _runState = GetComponent<bossChainSawState_Run>();
        _runState.Initialize(bossChainSawState.Run);
        
        _moveState = GetComponent<BossZomChainSaw_Move>();
        _moveState.Initialize(bossChainSawState.Move);

        _attackState = GetComponent<BossZomChainSaw_Attack>();
        _attackState.Initialize(bossChainSawState.Attack);

        _hitState = GetComponent<BossZomChainSaw_Hit>();
        _hitState.Initialize(bossChainSawState.Hit);
        
        _deadState = GetComponent<BossZomChainSaw_Dead>();
        _deadState.Initialize(bossChainSawState.Dead);

        stateController.Add(bossChainSawState.Idle, _idleState);
        stateController.Add(bossChainSawState.Move, _moveState);
        stateController.Add(bossChainSawState.Run, _runState);
        stateController.Add(bossChainSawState.Attack, _attackState);
        stateController.Add(bossChainSawState.Hit, _hitState);
        stateController.Add(bossChainSawState.Dead, _deadState);
    }

    void OnEnable()
    {
        _currentState = stateController[bossChainSawState.Run];
        _currentState.EnterState();
    }

    void Update()
    {
        bossChainSawState nextState = _currentState.GetNextState();
        if (_currentState.StateKey.Equals(nextState) && !_isTransition)
        {
            _currentState.UpdateState();
        }
        else
        {
            TransitionState(nextState);
        }
    }

    private void TransitionState(bossChainSawState newState)
    {
        _isTransition = true;
        _currentState.ExitState();
        //stateController[newState].EnterState();
        _currentState = stateController[newState];
        _currentState.EnterState();
        _isTransition = false;
    }

    public bool CanHit()
    {
        if (countBullet >= countBulletToHit)
        {
            countBullet = 0;
            return true;
        }
        
        return false;
    }
    
    public void PlusBulletToHit()=>countBullet++;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_centerExplosion.position, _radiusExplosion);
    }
    
    public void CheckHitColliderHitExplosion()
    {
        Collider[] cols = Physics.OverlapSphere(_centerExplosion.position, _radiusExplosion, _layerHit);
        if(cols.Length != 0 )
        {
            CheckHitExplosion();
        }
    }
    public void CheckHitExplosion()
    {
        Collider[] cols = Physics.OverlapSphere(_centerExplosion.position, _radiusExplosion, _layerHit);
        List<Transform> lstRoot = new List<Transform> ();
        foreach (Collider col in cols)
        {
            if (!lstRoot.Contains(col.gameObject.transform.root))
            {
                lstRoot.Add(col.gameObject.transform.root);
            }
        }
        foreach(var elem in lstRoot)
        {
            var takeDamageController = elem.gameObject.GetComponentInParent<ITakeDamage>();
            BotNetwork botnet = elem.gameObject.GetComponentInParent<BotNetwork>();
            if (botnet != null)
            {
                botnet.posExplosion = transform.position;
            }
            
            if(takeDamageController != null)
            {
                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Gas,
                    damage = 10000,
                    name = elem.gameObject.name,
                };
                takeDamageController.TakeDamage(damageInfo);
            }
        }
    }
    
}