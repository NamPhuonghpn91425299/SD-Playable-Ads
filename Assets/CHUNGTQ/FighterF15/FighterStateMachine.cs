using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


public class FighterStateMachine : MonoBehaviour
{
    public Dictionary<FighterState, BaseState<FighterState>> StateController = new Dictionary<FighterState, BaseState<FighterState>>();
    public BaseState<FighterState> _currentState;
    private bool _isTransition;

    private FighterIdleState _idleState;
    private FighterMoveState _moveState;
    private FighterAttackState _attackState;
    private FighterDeadState _deadState;
    public enum FighterState
    {
        DefaultState,
        Idle,
        Move,
        Attack,
        Dead
    }
    private void Awake()
    {
        InitializeState();
    }
    private void InitializeState()
    {
        _idleState = GetComponent<FighterIdleState>();
        _idleState.Initialize(FighterState.Idle);

        _moveState = GetComponent<FighterMoveState>();
        _moveState.Initialize(FighterState.Move);
        _attackState = GetComponent<FighterAttackState>();
        _attackState.Initialize(FighterState.Attack);

        _deadState = GetComponent<FighterDeadState>();
        _deadState.Initialize(FighterState.Dead);

        StateController.Add(FighterState.Idle, _idleState);
        StateController.Add(FighterState.Move, _moveState);
        StateController.Add(FighterState.Attack, _attackState);
        StateController.Add(FighterState.Dead, _deadState);
    }
    void OnEnable()
    {
        this.DelayFrames(1, () =>
        {
            _currentState = StateController[FighterState.Move];
            _currentState.EnterState();
        });
    }
    void Update()
    {
        FighterState nextState = _currentState!= null? _currentState.GetNextState():FighterState.DefaultState;
        if (_currentState?.StateKey.Equals(nextState) ?? false && !_isTransition)
        {
            _currentState.UpdateState();
        }
        else if(nextState!= FighterState.DefaultState)
        {
            TransitionState(nextState);
        }
    }
    private void TransitionState(FighterState tankState)
    {
        _isTransition = true;
        _currentState.ExitState();
        _currentState = StateController[tankState];
        _currentState.EnterState();
        _isTransition = false;
    }
}
