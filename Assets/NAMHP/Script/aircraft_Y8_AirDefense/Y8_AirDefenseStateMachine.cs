using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Y8_AirDefenseStateMachine : MonoBehaviour
{
    public BaseState<Y8_AirDefense> _currentY8State;
    public Dictionary<Y8_AirDefense, BaseState<Y8_AirDefense>> Y8_AirDefenseController = new Dictionary<Y8_AirDefense, BaseState<Y8_AirDefense>>();
    [SerializeField] private bool _isTransiton;
    private Y8_AirDefenseMoveState _y8DefenseMoveState;
    private Y8_AirDefenseDeadState _y8DefenseDeadState;
    private Y8_AirDefenseIdleState _y8DefenseIdleState;
    private Y8_AirDefenseGliderState _y8DefenseGliderState;
    public enum Y8_AirDefense
    {
        DefaultState,
        Idle,
        Move,
        Glider,
        Dead
    }
    private void Awake()
    {
        InitilizeState();
    }

    private void InitilizeState()
    {
        _y8DefenseIdleState = GetComponent<Y8_AirDefenseIdleState>();
        _y8DefenseIdleState.Initialize(Y8_AirDefense.Idle);

        _y8DefenseMoveState = GetComponent<Y8_AirDefenseMoveState>();
        _y8DefenseMoveState.Initialize(Y8_AirDefense.Move);

        _y8DefenseGliderState = GetComponent<Y8_AirDefenseGliderState>();
        _y8DefenseGliderState.Initialize(Y8_AirDefense.Glider);

        _y8DefenseDeadState = GetComponent <Y8_AirDefenseDeadState>();
        _y8DefenseDeadState.Initialize(Y8_AirDefense.Dead);

        Y8_AirDefenseController.Add(Y8_AirDefense.Idle, _y8DefenseIdleState);
        Y8_AirDefenseController.Add(Y8_AirDefense.Move, _y8DefenseMoveState);
        Y8_AirDefenseController.Add(Y8_AirDefense.Glider, _y8DefenseGliderState);
        Y8_AirDefenseController.Add(Y8_AirDefense.Dead, _y8DefenseDeadState);
    }

    private void OnEnable()
    {
        this.DelayFrames(1, () =>
        {
            _currentY8State = Y8_AirDefenseController[Y8_AirDefense.Move];
            _currentY8State.EnterState();
        });
    }
    private void Update()
    {
        Y8_AirDefense nextState = _currentY8State != null ? _currentY8State.GetNextState() : Y8_AirDefense.DefaultState;
        if (_currentY8State?.StateKey.Equals(nextState) ?? false && !_isTransiton)
        {
            _currentY8State.UpdateState();
        }
        else if (nextState != Y8_AirDefense.DefaultState)
        {
            TransitionState(nextState);
        }

    }
    private void TransitionState(Y8_AirDefense newState)
    {
        _isTransiton = true;
        _currentY8State.ExitState();
        _currentY8State = Y8_AirDefenseController[newState];
        _currentY8State.EnterState();
        _isTransiton = false;

    }
}
