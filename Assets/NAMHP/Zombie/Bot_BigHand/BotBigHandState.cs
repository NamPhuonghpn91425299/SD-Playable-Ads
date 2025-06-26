using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum BigHandState
    {
        Idle,
        Move,
        Attack,
        Dead,       
        Start,
        DeadExplosion
    }

    public class BotBigHandState : MonoBehaviour
    {
        public Dictionary<BigHandState, BaseState<BigHandState>> stateController = new Dictionary<BigHandState, BaseState<BigHandState>>();

        public BaseState<BigHandState> _currentState;
        private bool _isTransition;
        [SerializeField] private BotNetwork botNetwork;
        [SerializeField] private bool haveStart;


        BotBigHandMoveState _moveState;
        BotBigHandAttackState _attackState;
        BotBigHandDeadState _deadState;
        BotBigHandDeadExplosion _deadExplosionState;
        public BotBigHandStart _startState;


        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _moveState = GetComponent<BotBigHandMoveState>();
            _moveState.Initialize(BigHandState.Move);

            _attackState = GetComponent<BotBigHandAttackState>();
            _attackState.Initialize(BigHandState.Attack);

            _deadState = GetComponent<BotBigHandDeadState>();
            _deadState.Initialize(BigHandState.Dead);

            _deadExplosionState = GetComponent<BotBigHandDeadExplosion>();
            _deadExplosionState.Initialize(BigHandState.DeadExplosion);

            stateController.Add(BigHandState.Move, _moveState);
            stateController.Add(BigHandState.Attack, _attackState);
            stateController.Add(BigHandState.Dead, _deadState);
            stateController.Add(BigHandState.DeadExplosion, _deadExplosionState);

            if (haveStart)
            {
                _startState = GetComponent<BotBigHandStart>();
                _startState.Initialize(BigHandState.Start);

                stateController.Add(BigHandState.Start, _startState);
            }
        }

        void OnEnable()
        { 
            botNetwork.OnTakeDamagePlayer += OnTakeDamePlayer;
            if (haveStart)
                _currentState = stateController[BigHandState.Start];
            else
                _currentState = stateController[BigHandState.Move];

            _currentState.EnterState();
        }

        private void OnTakeDamePlayer(int damage)
        {
            if (_currentState.StateKey == BigHandState.Start)
            {
                _startState.OntakeDame();
                if (!BotManager.Instance.AllZBEatAttack)
                {
                    BotManager.Instance.AllZBEatAttack = true;
                    foreach (botZomNorsuit VARIABLE in BotManager.Instance._botzomNorAddBangTay)
                    {
                        VARIABLE._startState.OntakeDame();
                    }
                }
            }
            print(" [dautao] " + damage);
        }
        
        void OnDisable()
        {
            botNetwork.OnTakeDamagePlayer -= OnTakeDamePlayer;
        }
        
        void Update()
        {
            BigHandState nextState = _currentState.GetNextState();
            if (_currentState.StateKey.Equals(nextState) && !_isTransition)
            {
                _currentState.UpdateState();
            }
            else
            {
                TransitionState(nextState);
            }
        }

        private void TransitionState(BigHandState tankState)
        {
            _isTransition = true;
            _currentState.ExitState();
            _currentState = stateController[tankState];
            _currentState.EnterState();
            _isTransition = false;
        }
    }
