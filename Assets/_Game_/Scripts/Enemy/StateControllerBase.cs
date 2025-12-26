using System;
using static GameConstants;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class StateControllerBase: MonoBehaviour
{
    public bool OnInitEqualStart = false; // Nếu true thì trạng thái khởi tạo sẽ vào trạng thái bắt đầu không thì vẫn vào move
    public bool canDead = true;
    public Dictionary<EnemyState, StateBase> stateController = new Dictionary<EnemyState, StateBase>();
    public StateBase _currentState;
    [Space(20)]
#if UNITY_EDITOR
    public bool autoFindBotContext = true;
    protected virtual void OnValidate()
    {
        if (!autoFindBotContext) return;
        botContext.botNetwork = GetComponent<EnemyBase>();
        botContext.animator = GetComponentInChildren<Animator>();
        botContext.audioPlayable = GetComponent<AudioPlayable>();
        botContext.botIdentity = GetComponent<BotIdentity>();
            
        botContext.stateController = this;
        if (botContext.botNetwork == null)
            Debug.LogError("Null botNet trong botcontext kìa: " + gameObject.name);
        if (botContext.animator == null)
            Debug.LogError("Null animator trong botcontext kìa: " + gameObject.name);
        if (botContext.audioPlayable == null)
            Debug.LogError("Null audioPlayable trong botcontext kìa: " + gameObject.name);
        if (botContext.botIdentity == null)
            Debug.LogError("Null botindentity trong botcontext kìa: " + gameObject.name);
            
        if (botContext.stateController == null)
            autoFindBotContext = false;
    }
#endif
    //[SerializeField] 
    public BotContext botContext;
    protected bool _isTransition;

    // void Start()
    // {
    //     botContext.audioPlayable = GetComponent<IAudioPlayable>();
        
    // }


    /// <summary>
    /// Khởi tạo trạng thái của bot. 
    /// </summary>
    /// <param name="_EnterState">Trạng thái bắt đầu của bot</param>
    public virtual void OnInit(EnemyState _EnterState)
    {
        if (botContext.botNetwork.isBoss)
        {
            EventManager.Instance.Publish(new BossSpawnEvent(true));
        }
        canDead = true;
        botContext.botNetwork.ACBotDead += OnDead;
        botContext.botNetwork.ACOnTakeDamage += OnTakeDame;
        _currentState = stateController[_EnterState];
        _currentState.EnterState();
    }
    
    protected virtual void Update()
    {
        if (_currentState == null)
        {
//            print("Null current state");
            return;
        }
        
        if (_isTransition)
        {
  //          print("Đang đổi trạng thái");
            return;
        }
        _currentState.UpdateState();
    }
    
    protected virtual  void OnTakeDame(DamageInfo _damageInfo)
    {
        
    }
    
    protected virtual void OnDead(bool isDead)
    {
        botContext.botNetwork.ACOnTakeDamage -= OnTakeDame;
        botContext.botNetwork.ACBotDead -= OnDead;
        if (!canDead)
            return;
        canDead = false;
        ChangeState(EnemyState.Dead);
    }
    
    #region State Controller
    public virtual void ChangeState(EnemyState newAllEnemyState)
    {
//        print("Đổi trạng thái sang: " + newAllState);
        if (_currentState == null || _currentState.StateKey.Equals(newAllEnemyState) || _isTransition) 
            return;

        TransitionState(newAllEnemyState);
    }
    private void TransitionState(EnemyState newAllEnemyState)
    {
        _isTransition = true;
        _currentState.ExitState();
        _currentState = stateController[newAllEnemyState];
        _currentState.EnterState();
        _isTransition = false;
    }
    #endregion

    protected virtual void OnDisable()
    {
        _currentState = null;
    }

    /// <summary>
    /// Điều chỉnh trang thái bắt đầu của bot
    /// </summary>
    /// <param name="_typeStart">0: 1: 2:</param>
    public virtual void SetupStartState(int _typeStart)
    {
        
    }

    public virtual void CallEndStart()
    {
        
    }

    public virtual void DeadExplosion()
    {
        
    }
}