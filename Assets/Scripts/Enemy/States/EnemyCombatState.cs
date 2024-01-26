using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyCombatState : EnemyBaseState
{
    public EnemyCombatState(EnemyStateMachine ctx) : base(ctx) { }

    public override async void Enter()
    {
        Debug.Log("Combat");
        _context.Anim.SetFloat("Speed", 0);

        if (_context.enemyManager.type == EnemyManager.Type.Enemy)
        {
            _context.Anim.SetTrigger("Enemy Attack");
        }
        else if (_context.enemyManager.type == EnemyManager.Type.Ally)
        {
            _context.Anim.SetTrigger("Ally Attack");
        }
        Collider[] targetInViewRadius = Physics.OverlapSphere(_context.transform.position, _context.combatRadius, _context.targetLayer);

        foreach (Collider collider in targetInViewRadius)
        {
            if (_context.enemyManager.type == EnemyManager.Type.Enemy)
            {
                PlayerManager player = collider.GetComponent<PlayerManager>();
                if (player != null)
                {
                    player.TakeDamage(1f);
                }

                EnemyManager enemy = collider.GetComponent<EnemyManager>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1f);
                }
            }
            else if (_context.enemyManager.type == EnemyManager.Type.Ally)
            {
                EnemyManager enemy = collider.GetComponent<EnemyManager>();
                if (enemy != null)
                {
                    enemy.TakeLaughDamage(1f);
                }
            }

            await Task.Delay(2000);

            OnAttacked();
        }
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
        _context.ResetCombatCooldown();
    }
}
