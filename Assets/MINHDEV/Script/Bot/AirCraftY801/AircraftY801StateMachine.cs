using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotPlayItaStateMachine;

public class AircraftY801StateMachine : MonoBehaviour
{
    [SerializeField] private BotNetwork botNetwork;
    private Dictionary<AirForceState, BaseState<AirForceState>> StateController = new Dictionary<AirForceState, BaseState<AirForceState>>();
    public BaseState<AirForceState> _currentState;
    private bool _isTransition;

    private AircraftY801FlyState _flyState;
    private AircraftY801SpwanState _spwanState;
    private AircaftY801MoveBehindState _moveBehindState;
    public enum AirForceState
    {
        Fly,
        SpawnBot,
        MoveBehindPos,
        Dead
    }
    private void Awake()
    {
        InitializeState();
    }
    private void InitializeState()
    {
        _flyState = GetComponent<AircraftY801FlyState>();
        _flyState.Initialize(AirForceState.Fly);
        _spwanState = GetComponent<AircraftY801SpwanState>();
        _spwanState.Initialize(AirForceState.SpawnBot);
        _moveBehindState = GetComponent<AircaftY801MoveBehindState>();
        _moveBehindState.Initialize(AirForceState.MoveBehindPos);

        StateController.Add(AirForceState.Fly, _flyState);
        StateController.Add(AirForceState.SpawnBot, _spwanState);
        StateController.Add(AirForceState.MoveBehindPos, _moveBehindState);
    }
    void OnEnable()
    {
        _currentState = StateController[AirForceState.Fly];
        _currentState.EnterState();
        botNetwork.OnTakeDamage += OnTakeDame;
    }
    private void OnTakeDame(int dame)
    {
        Debug.Log(dame);
    }
    void Update()
    {
        AirForceState nextState = _currentState.GetNextState();
        if (_currentState.StateKey.Equals(nextState) && !_isTransition)
        {
            _currentState.UpdateState();
        }
        else
        {
            TransitionState(nextState);
        }
    }

    private void TransitionState(AirForceState State)
    {
        _isTransition = true;
        _currentState.ExitState();
        _currentState = StateController[State];
        _currentState.EnterState();
        _isTransition = false;
    }

}
