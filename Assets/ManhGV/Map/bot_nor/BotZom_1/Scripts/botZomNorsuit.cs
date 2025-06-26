using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum botZomState
    {
        Idle,
        Move,
        Attack,
        Dead,
        Start,
        DeadExplosion
    }

    public class botZomNorsuit : MonoBehaviour
    {
        public Dictionary<botZomState, BaseState<botZomState>> stateController = new Dictionary<botZomState, BaseState<botZomState>>();

        public BaseState<botZomState> _currentState;
        private bool _isTransition;
        [SerializeField] private BotNetwork botNetwork;
        [SerializeField] private bool haveStart;


        botZomNorsuitMoveState _moveState;
        botZomNorsuitAttackState _attackState;
        botZomNorsuitDeadState _deadState;
        botZomNorsuitDeadExplosion _deadExplosionState;
        public botZomNorsuitStart _startState;


        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _moveState = GetComponent<botZomNorsuitMoveState>();
            _moveState.Initialize(botZomState.Move);

            _attackState = GetComponent<botZomNorsuitAttackState>();
            _attackState.Initialize(botZomState.Attack);

            _deadState = GetComponent<botZomNorsuitDeadState>();
            _deadState.Initialize(botZomState.Dead);

            _deadExplosionState = GetComponent<botZomNorsuitDeadExplosion>();
            _deadExplosionState.Initialize(botZomState.DeadExplosion);

            stateController.Add(botZomState.Move, _moveState);
            stateController.Add(botZomState.Attack, _attackState);
            stateController.Add(botZomState.Dead, _deadState);
            stateController.Add(botZomState.DeadExplosion, _deadExplosionState);

            if (haveStart)
            {
                _startState = GetComponent<botZomNorsuitStart>();
                _startState.Initialize(botZomState.Start);

                stateController.Add(botZomState.Start, _startState);
            }
        }

        void OnEnable()
        { 
            botNetwork.OnTakeDamagePlayer += OnTakeDamePlayer;
            if (haveStart)
                _currentState = stateController[botZomState.Start];
            else
                _currentState = stateController[botZomState.Move];

            _currentState.EnterState();
        }

        private void OnTakeDamePlayer(int damage)
        {
            if (_currentState.StateKey == botZomState.Start)
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
            botZomState nextState = _currentState.GetNextState();
            if (_currentState.StateKey.Equals(nextState) && !_isTransition)
            {
                _currentState.UpdateState();
            }
            else
            {
                TransitionState(nextState);
            }
        }

        private void TransitionState(botZomState tankState)
        {
            _isTransition = true;
            _currentState.ExitState();
            _currentState = stateController[tankState];
            _currentState.EnterState();
            _isTransition = false;
        }
    }
