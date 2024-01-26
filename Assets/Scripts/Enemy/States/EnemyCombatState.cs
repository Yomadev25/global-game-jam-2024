using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatState : EnemyBaseState
{
    public EnemyCombatState(EnemyStateMachine ctx) : base(ctx) { }

    public override void Enter()
    {
        Debug.Log("Combat");
        _context.Anim.SetFloat("Speed", 0);
    }

    public override void Update()
    {
        
    }

    public override void FixedUpdate()
    {

    }

    public void OnAttacked()
    {
        ChangeState(_context.State.Chase());
    }

    public override void Exit()
    {

    }
}
