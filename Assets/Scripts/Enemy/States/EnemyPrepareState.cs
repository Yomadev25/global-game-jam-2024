using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPrepareState : EnemyBaseState
{
    Vector3 direction;
    float moveSpeed = 0.5f;

    Transform target;

    public EnemyPrepareState(EnemyStateMachine ctx) : base(ctx) { }

    public override void Enter()
    {
        int randomDir = Random.Range(0, 2);
        direction = randomDir == 1 ? Vector3.right : Vector3.left;

        _context.Anim.SetFloat("Speed", moveSpeed);
    }

    public override void Update()
    {
        target = _context.GetCombatTarget();
        if (target != null)
        {
            Vector3 dir = (target.position - _context.transform.position).normalized;
            Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir;
            Vector3 movedir = Vector3.zero;
            Vector3 finalDirection = (pDir * direction.normalized.x);

            movedir += finalDirection * moveSpeed * Time.deltaTime;
            _context.NavMesh.Move(movedir);
            _context.transform.LookAt(new Vector3(target.position.x, _context.transform.position.y, target.position.z));

            //_context.transform.rotation = Quaternion.LookRotation(finalDirection);
        }

        CheckChangeState();
    }

    public override void FixedUpdate()
    {

    }

    private void CheckChangeState()
    {
        if (_context.IsReadyToCombat)
        {
            ChangeState(_context.State.Combat());
        }
        
        if (target == null)
        {
            ChangeState(_context.State.Chase());
        }
    }

    public override void Exit()
    {

    }
}
