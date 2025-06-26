using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterStateMachine : MonoBehaviour
{
    public Dictionary<HelicopterState, BaseState<HelicopterState>> battleShipStatesController = new Dictionary<HelicopterState, BaseState<HelicopterState>>();
    public BaseState<HelicopterState> _currentHelicopterState;
    [SerializeField] private bool _istransition;
    HelicopterMoveAttackState _moveAttackState;
    HelicopterDeadState _deadState;

    public enum HelicopterState
    {
        DefaultState,
        MoveAttack,
        Dead
    }

    private void Awake()
    {
        InitializeState();
    }
    private void InitializeState()
    {
        _moveAttackState = GetComponent<HelicopterMoveAttackState>();
        _moveAttackState.Initialize(HelicopterState.MoveAttack);

        _deadState = GetComponent<HelicopterDeadState>();
        _deadState.Initialize(HelicopterState.Dead);

        battleShipStatesController.Add(HelicopterState.MoveAttack, _moveAttackState);
        battleShipStatesController.Add(HelicopterState.Dead, _deadState);
    }


    private void OnEnable()
    {
        this.DelayFrames(1, () =>
        {
            _currentHelicopterState = battleShipStatesController[HelicopterState.MoveAttack];
            _currentHelicopterState.EnterState();
        });
    }

    private void Update()
    {
        HelicopterState nextState = _currentHelicopterState != null ? _currentHelicopterState.GetNextState() : HelicopterState.DefaultState;

        if (_currentHelicopterState?.StateKey.Equals(nextState) ?? false && !_istransition)
        {
            _currentHelicopterState.UpdateState();
        }
        else if (nextState != HelicopterState.DefaultState)
        {
            //Debug.LogError(nextState.ToString());
            TransitionState(nextState);
        }
    }




    private void TransitionState(HelicopterState newState)
    {
        _istransition = true;
        _currentHelicopterState.ExitState();
        _currentHelicopterState = battleShipStatesController[newState];
        _currentHelicopterState.EnterState();
        _istransition = false;
    }
}
