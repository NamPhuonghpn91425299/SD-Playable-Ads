using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleShipStateMachine : MonoBehaviour
{
    public Dictionary<BattleShipState, BaseState<BattleShipState>> battleShipStatesController = new Dictionary<BattleShipState, BaseState<BattleShipState>>();
    public BaseState<BattleShipState> _currentBattleShipState;
    [SerializeField] private bool _istransition;
    BattleShipMoveAttackState _moveAttackState;
    BattleShipDeadState _deadState;

    public enum BattleShipState
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
        _moveAttackState = GetComponent<BattleShipMoveAttackState>();
        _moveAttackState.Initialize(BattleShipState.MoveAttack);

        _deadState = GetComponent<BattleShipDeadState>();
        _deadState.Initialize(BattleShipState.Dead);

        battleShipStatesController.Add(BattleShipState.MoveAttack, _moveAttackState);
        battleShipStatesController.Add(BattleShipState.Dead, _deadState);
    }


    private void OnEnable()
    {
        this.DelayFrames(1, () =>
        {
            _currentBattleShipState = battleShipStatesController[BattleShipState.MoveAttack];
            _currentBattleShipState.EnterState();
        });
    }

    private void Update()
    {
        BattleShipState nextState = _currentBattleShipState != null ? _currentBattleShipState.GetNextState() : BattleShipState.DefaultState;

        if (_currentBattleShipState?.StateKey.Equals(nextState) ?? false && !_istransition)
        {
            _currentBattleShipState.UpdateState();
        }
        else if (nextState != BattleShipState.DefaultState)
        {
            //Debug.LogError(nextState.ToString());
            TransitionState(nextState);
        }
    }




    private void TransitionState(BattleShipState newState)
    {
        _istransition = true;
        _currentBattleShipState.ExitState();
        _currentBattleShipState = battleShipStatesController[newState];
        _currentBattleShipState.EnterState();
        _istransition = false;
    }
}
