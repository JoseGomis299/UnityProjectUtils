using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.StateMachine
{
    public abstract class State
    {
        public abstract void OnEnterState(EnemyStateManager enemyStateManager);

        public abstract void OnUpdateState(EnemyStateManager enemyStateManager);

        public abstract void OnEndState(EnemyStateManager enemyStateManager);

        public abstract void OnCollisionEnter(EnemyStateManager enemyStateManager, Collision2D collision);
    }
}