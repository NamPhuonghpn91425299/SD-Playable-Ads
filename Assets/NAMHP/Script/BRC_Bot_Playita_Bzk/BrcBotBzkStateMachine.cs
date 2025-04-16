using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrcBotBzkStateMachine : MonoBehaviour
{
    public enum BrcBotBzkState
    {
        Move,
        Attack,
        Dead
    }
    public Dictionary<BrcBotBzkState, BaseState<BrcBotBzkState>> stateControllers = new Dictionary<BrcBotBzkState, BaseState<BrcBotBzkState>>();
    public BaseState<BrcBotBzkState> currentState;
    private bool _isTransition;
    private BrcBotBzkAttackState _brcBotBzkAttackState;
    private BrcBotBzkMoveState _brcBotBzkMoveState;
    private BrcBotBzkDeadState _brcBotBzkDeadState;


    private void Init()
    {
        _brcBotBzkAttackState = GetComponent<BrcBotBzkAttackState>();
        _brcBotBzkMoveState = GetComponent<BrcBotBzkMoveState>();
        _brcBotBzkDeadState = GetComponent<BrcBotBzkDeadState>();
        
        _brcBotBzkAttackState.Initialize(BrcBotBzkState.Attack);
        _brcBotBzkMoveState.Initialize(BrcBotBzkState.Move);
        _brcBotBzkDeadState.Initialize(BrcBotBzkState.Dead);
        
        stateControllers.Add(BrcBotBzkState.Attack, _brcBotBzkAttackState);
        stateControllers.Add(BrcBotBzkState.Move, _brcBotBzkMoveState);
        stateControllers.Add(BrcBotBzkState.Dead, _brcBotBzkDeadState);
        
    }
    public void ResetBotState()
    {
        _isTransition = false;  // Đảm bảo không bị kẹt ở trạng thái chuyển đổi
        currentState = stateControllers[BrcBotBzkState.Move]; // Đặt lại trạng thái về Move
        currentState.EnterState(); // Kích hoạt lại trạng thái Move
    }
    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        ResetBotState();
    }
    
    private void OnDisable()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        BrcBotBzkState nextState = currentState.GetNextState();
        if (currentState.StateKey.Equals(nextState) && !_isTransition)
        {
            currentState.UpdateState();
        }
        else
        {
            TransitionState(nextState);
        }
        
    }

    private void TransitionState(BrcBotBzkState nextState)
    {
        _isTransition = true;
        currentState.ExitState();
        currentState = stateControllers[nextState];
        currentState.EnterState();
        _isTransition = false;
    }
}
