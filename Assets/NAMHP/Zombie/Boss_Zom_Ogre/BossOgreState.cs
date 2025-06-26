using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum BossOgreState
    {
        Idle,
        Move,
        Attack,
        Dead,
        Start,
        DeadExplosion
    }

    public class BossOgreStateMachine : MonoBehaviour
    {
        public Dictionary<BossOgreState, BaseState<BossOgreState>> stateController = new Dictionary<BossOgreState, BaseState<BossOgreState>>();

        public BaseState<BossOgreState> _currentState;
        private bool _isTransition;
        [SerializeField] private BotNetwork botNetwork;
        BossOgreMoveState _moveState;
        BossOgreAttackState _attackState;
        BossOgreDeadState _deadState;
        BossOgreDeadExplosion _deadExplosionState;
        BossOgreStart _startState;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _moveState = GetComponent<BossOgreMoveState>();
            _moveState.Initialize(BossOgreState.Move);

            _attackState = GetComponent<BossOgreAttackState>();
            _attackState.Initialize(BossOgreState.Attack);

            _deadState = GetComponent<BossOgreDeadState>();
            _deadState.Initialize(BossOgreState.Dead);

            _deadExplosionState = GetComponent<BossOgreDeadExplosion>();
            _deadExplosionState.Initialize(BossOgreState.DeadExplosion);
            
            stateController.Add(BossOgreState.Move, _moveState);
            stateController.Add(BossOgreState.Attack, _attackState);
            stateController.Add(BossOgreState.Dead, _deadState);
            stateController.Add(BossOgreState.DeadExplosion, _deadExplosionState);
            
            _startState = GetComponent<BossOgreStart>();
            _startState.Initialize(BossOgreState.Start);
            stateController.Add(BossOgreState.Start, _startState);
        }

        void OnEnable()
        { 
            botNetwork.OnTakeDamagePlayer += OnTakeDamePlayer;
            _currentState = stateController[BossOgreState.Move];
            _currentState.EnterState();
        }

        private void OnTakeDamePlayer(int damage)
        {
            if (_currentState.StateKey == BossOgreState.Start)
            {
                if (!BotManager.Instance.AllZBEatAttack)
                {
                    BotManager.Instance.AllZBEatAttack = true;
                    foreach (botZomNorsuit VARIABLE in BotManager.Instance._botzomNorAddBangTay)
                    {
                        VARIABLE._startState.OntakeDame();
                    }
                }
            }
            //print(" [dautao] " + damage);
        }
        
        void OnDisable()
        {
            botNetwork.OnTakeDamagePlayer -= OnTakeDamePlayer;
        }
        
        void Update()
        {
            BossOgreState nextState = _currentState.GetNextState();
            if (_currentState.StateKey.Equals(nextState) && !_isTransition)
            {
                _currentState.UpdateState();
            }
            else
            {
                TransitionState(nextState);
            }
        }

        private void TransitionState(BossOgreState tankState)
        {
            _isTransition = true;
            _currentState.ExitState();
            _currentState = stateController[tankState];
            _currentState.EnterState();
            _isTransition = false;
        }
    }
