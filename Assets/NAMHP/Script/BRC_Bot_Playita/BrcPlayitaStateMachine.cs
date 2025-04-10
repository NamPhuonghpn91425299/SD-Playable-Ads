using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrcPlayitaStateMachine : MonoBehaviour
{
    public Dictionary<BrcPlayitaState, BaseState<BrcPlayitaState>> stateControllers = new Dictionary<BrcPlayitaState, BaseState<BrcPlayitaState>>();
    public BaseState<BrcPlayitaState> currentState;
    private bool _isTransition;
    private BrcPlayitaAttackState _brcPlayItaAttackState;
    private BrcPlayitaMoveState _brcPlayItaMoveState;
    private BrcPlayitaDeadState _brcPlayItaDeadState;
    
    public enum BrcPlayitaState
    {
        Move,
        Attack,
        Dead
    }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        ResetBotState();
    }

    private void Init()
    {
        _brcPlayItaMoveState = GetComponent<BrcPlayitaMoveState>();
        _brcPlayItaMoveState.Initialize(BrcPlayitaState.Move);
        _brcPlayItaAttackState = GetComponent<BrcPlayitaAttackState>();
        _brcPlayItaAttackState.Initialize(BrcPlayitaState.Attack);
        _brcPlayItaDeadState = GetComponent<BrcPlayitaDeadState>();
        _brcPlayItaDeadState.Initialize(BrcPlayitaState.Dead);
        
        stateControllers.Add(BrcPlayitaState.Move, _brcPlayItaMoveState);
        stateControllers.Add(BrcPlayitaState.Attack, _brcPlayItaAttackState);
        stateControllers.Add(BrcPlayitaState.Dead, _brcPlayItaDeadState);
    }
    public void ResetBotState()
    {
        _isTransition = false;  // Đảm bảo không bị kẹt ở trạng thái chuyển đổi
        currentState = stateControllers[BrcPlayitaState.Move]; // Đặt lại trạng thái về Move
        currentState.EnterState(); // Kích hoạt lại trạng thái Move
    }

    private void Update()
    {
        BrcPlayitaState nextState = currentState.GetNextState();
        if(currentState.StateKey.Equals(nextState) && !_isTransition)
        {
            currentState.UpdateState();
        }
        else
        {
            TransitionState(nextState);
        }

    }
    private void TransitionState(BrcPlayitaState nextState)
    {
        _isTransition = true;
        currentState.ExitState();
        currentState = stateControllers[nextState];
        currentState.EnterState();
        _isTransition = false;
    }
}
