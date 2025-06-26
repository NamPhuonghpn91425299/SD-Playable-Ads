using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BotPlayItaStateMachine : MonoBehaviour
{
    public Dictionary<PlayItaState, BaseState<PlayItaState>> StateController = new Dictionary<PlayItaState, BaseState<PlayItaState>>();
    public BaseState<PlayItaState> _currentState;
    private bool _isTransition;
    private BotPlayItaMoveState _botPlayItaMoveState;
    private BotPlayItaAttackState _botPlayItaAttackState;
    private BotPlayItaDeadState _botPlayItaDeadState;

    public enum PlayItaState
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
        _botPlayItaMoveState = GetComponent<BotPlayItaMoveState>();
        _botPlayItaMoveState.Initialize(PlayItaState.Move);

        _botPlayItaAttackState = GetComponent<BotPlayItaAttackState>();
        _botPlayItaAttackState.Initialize(PlayItaState.Attack);

        _botPlayItaDeadState = GetComponent<BotPlayItaDeadState>();
        _botPlayItaDeadState.Initialize(PlayItaState.Dead);

        StateController.Add(PlayItaState.Move,_botPlayItaMoveState);
        StateController.Add(PlayItaState.Attack, _botPlayItaAttackState);
        StateController.Add(PlayItaState.Dead, _botPlayItaDeadState);

    }

    void OnEnable()
    {
        _currentState = StateController[PlayItaState.Move];
        _currentState.EnterState();
    }
    void Update()
    {
        PlayItaState nextState = _currentState.GetNextState();
        if (_currentState.StateKey.Equals(nextState) && !_isTransition)
        {
            _currentState.UpdateState();
        }
        else
        {
            TransitionState(nextState);
        }
    }
    private void TransitionState(PlayItaState tankState)
    {
        _isTransition = true;
        _currentState.ExitState();
        _currentState = StateController[tankState];
        _currentState.EnterState();
        _isTransition = false;
    }

}
