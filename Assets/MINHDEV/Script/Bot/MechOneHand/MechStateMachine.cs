using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotPlayItaStateMachine;

public class MechStateMachine : MonoBehaviour
{
    public Dictionary<MechState, BaseState<MechState>> StateController = new Dictionary<MechState, BaseState<MechState>>();
    public BaseState<MechState> _currentState;

    private MechMoveState _mechMoveState;
    private MechAttackState _mechAttackState;
    private MechDeadState _mechDeadState;

    private bool _isTransition;

    public enum MechState
    {
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
        _mechMoveState = GetComponent<MechMoveState>();
        _mechMoveState.Initialize(MechState.Move);

        _mechAttackState = GetComponent<MechAttackState>();
        _mechAttackState.Initialize(MechState.Attack);

        _mechDeadState = GetComponent<MechDeadState>();
        _mechDeadState.Initialize(MechState.Dead);

        StateController.Add(MechState.Move, _mechMoveState);
        StateController.Add(MechState.Attack, _mechAttackState);
        StateController.Add(MechState.Dead, _mechDeadState);

    }

    void OnEnable()
    {
        _currentState = StateController[MechState.Move];
        _currentState.EnterState();
    }

    void Update()
    {
        MechState nextState = _currentState.GetNextState();
        if (_currentState.StateKey.Equals(nextState) && !_isTransition)
        {
            _currentState.UpdateState();
        }
        else
        {
            TransitionState(nextState);
        }
    }

    private void TransitionState(MechState mechState)
    {
        _isTransition = true;
        _currentState.ExitState();
        _currentState = StateController[mechState];
        _currentState.EnterState();
        _isTransition = false;
    }
}
