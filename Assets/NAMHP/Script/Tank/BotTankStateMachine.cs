using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotTankStateMachine : MonoBehaviour
{
    public Dictionary<TankState, BaseState<TankState>> tankStatesController = new Dictionary<TankState, BaseState<TankState>>();
    public BaseState<TankState> _currentTankState;
    [SerializeField] private bool _istransition;
    private BotTankAttackState _botTankAttackState;
    private BotTankMoveState _botTankMoveState;
    private BotTankDeadState _botTankDeadState;
    private BotTankMoveToAttackState _botTankMoveToAttackState;
    public enum TankState
    {
        DefaultState,
        Move,
        MoveToAttack,
        Acttack,
        Dead
    }

    private void Awake()
    {
        InitializeState();
    }
    private void InitializeState()
    {
        _botTankMoveState = GetComponent<BotTankMoveState>();
        _botTankMoveState.Initialize(TankState.Move);

        _botTankMoveToAttackState = GetComponent<BotTankMoveToAttackState>();
        _botTankMoveToAttackState.Initialize(TankState.MoveToAttack);

        _botTankAttackState = GetComponent<BotTankAttackState>();
        _botTankAttackState.Initialize(TankState.Acttack);
        
        _botTankDeadState = GetComponent<BotTankDeadState>();
        _botTankDeadState.Initialize(TankState.Dead);
        
        tankStatesController.Add(TankState.Move, _botTankMoveState);
        tankStatesController.Add(TankState.MoveToAttack, _botTankMoveToAttackState);
        tankStatesController.Add(TankState.Acttack, _botTankAttackState);
        tankStatesController.Add(TankState.Dead, _botTankDeadState);
    }


    private void OnEnable()
    {
        this.DelayFrames(1, () =>
        {
            _currentTankState = tankStatesController[TankState.Move];
            _currentTankState.EnterState();
        });
    }

    private void Update()
    {
        TankState nextState = _currentTankState != null ? _currentTankState.GetNextState() : TankState.DefaultState;

        if (_currentTankState?.StateKey.Equals(nextState) ?? false && !_istransition)
        {
            _currentTankState.UpdateState();
        }
        else if (nextState != TankState.DefaultState)
        {
            //Debug.LogError(nextState.ToString());
            TransitionState(nextState); 
        }
    }




    private void TransitionState(TankState newState)
    {
        _istransition = true; 
        _currentTankState.ExitState();
        _currentTankState = tankStatesController[newState];
        _currentTankState.EnterState();
        _istransition = false;
    }
}
