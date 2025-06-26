using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCraftStateMachine : MonoBehaviour
{
    public Dictionary<AirCraftState, BaseState<AirCraftState>> battleShipStatesController = new Dictionary<AirCraftState, BaseState<AirCraftState>>();
    public BaseState<AirCraftState> _currentAirCraftState;
    [SerializeField] private bool _istransition;
    AirCraftMoveAttackState _moveAttackState;
    AirCraftDeadState _deadState;

    public enum AirCraftState
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
        _moveAttackState = GetComponent<AirCraftMoveAttackState>();
        _moveAttackState.Initialize(AirCraftState.MoveAttack);

        _deadState = GetComponent<AirCraftDeadState>();
        _deadState.Initialize(AirCraftState.Dead);

        battleShipStatesController.Add(AirCraftState.MoveAttack, _moveAttackState);
        battleShipStatesController.Add(AirCraftState.Dead, _deadState);
    }


    private void OnEnable()
    {
        this.DelayFrames(1, () =>
        {
            _currentAirCraftState = battleShipStatesController[AirCraftState.MoveAttack];
            _currentAirCraftState.EnterState();
        });
    }

    private void Update()
    {
        AirCraftState nextState = _currentAirCraftState != null ? _currentAirCraftState.GetNextState() : AirCraftState.DefaultState;

        if (_currentAirCraftState?.StateKey.Equals(nextState) ?? false && !_istransition)
        {
            _currentAirCraftState.UpdateState();
        }
        else if (nextState != AirCraftState.DefaultState)
        {
            //Debug.LogError(nextState.ToString());
            TransitionState(nextState);
        }
    }




    private void TransitionState(AirCraftState newState)
    {
        _istransition = true;
        _currentAirCraftState.ExitState();
        _currentAirCraftState = battleShipStatesController[newState];
        _currentAirCraftState.EnterState();
        _istransition = false;
    }
}
