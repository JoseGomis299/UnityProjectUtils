using System.Collections;
using System.Collections.Generic;
using ProjectUtils.StateMachine;
using UnityEngine;

public class EnemyRoaming : State
{
    private Vector2 _movementDirection;
    public override void OnEnterState(EnemyStateManager enemyStateManager)
    {
    }
    public override void OnUpdateState(EnemyStateManager enemyStateManager)
    {
    }

    public override void OnEndState(EnemyStateManager enemyStateManager)
    {
    }

    public override void OnCollisionEnter(EnemyStateManager enemyStateManager, Collision2D collision)
    {
    }
}
