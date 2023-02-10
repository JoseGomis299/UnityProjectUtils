using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.StateMachine
{
    public class EnemyStateManager : MonoBehaviour
    {
        private State _currentState;
        public readonly EnemyRoaming roamingState = new EnemyRoaming();
        public readonly EnemyAttacking attackingState = new EnemyAttacking();
        public readonly EnemySecondPhase secondPhaseState = new EnemySecondPhase();
        public readonly EnemyDie dieState = new EnemyDie();


        public bool isBoss = false;
        private float _health;

        private void Start()
        {
            _currentState = roamingState;

            _currentState.OnEnterState(this);
        }

        private void Update()
        {
            _currentState.OnUpdateState(this);
        }

        public void SetState(State state)
        {
            if (state == _currentState) return;

            _currentState = state;
            state.OnEnterState(this);
        }

        public void ChangeHealth(float increment)
        {
            _health += increment;
            if (_health <= 0) SetState(dieState);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _currentState.OnCollisionEnter(this, col);
        }
    }
}
