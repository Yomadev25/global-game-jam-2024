using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseState
{
    protected EnemyStateMachine _context;

    public EnemyBaseState(EnemyStateMachine ctx)
    {
        _context = ctx;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();

    protected void ChangeState(EnemyBaseState newState)
    {
        _context.CurrentState.Exit();

        newState.Enter();
        _context.CurrentState = newState;
    }
}
