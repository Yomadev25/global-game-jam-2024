using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyManager), typeof(NavMeshAgent))]
public class EnemyStateMachine : MonoBehaviour
{
    public enum IdleBehavior
    {
        IDLE,
        PATROL
    }

    [Header("Field Of View")]
    [SerializeField]
    private float _viewRadius;
    [SerializeField]
    private float _chaseRadius;
    [SerializeField]
    private float _combatRadius;
    [SerializeField]
    private float _viewAngle;
    [SerializeField]
    private LayerMask _targetLayer;
    [SerializeField]
    private LayerMask _obstacleLayer;

    [Header("Idle Behaviours")]
    [SerializeField]
    private IdleBehavior _idleBehavior;
    [SerializeField]
    private Transform[] _patrolSequence;
    [SerializeField]
    private float _patrolDelay;
    [SerializeField]
    private int _currentPatrol;
    [SerializeField]
    private float _delayPerCombo;
    private float _currentCooldown;

    [Header("References")]
    [SerializeField]
    private NavMeshAgent _navMesh;
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private AnimatorOverrideController _defaultAnim;
    [SerializeField]
    private AnimatorOverrideController _overrideAnim;
    [SerializeField]
    private EnemyManager _enemyManager;


    #region PUBLIC REFERENCES
    public EnemyBaseState CurrentState { get; set; }
    public EnemyStateFactory State { get; set; }

    public IdleBehavior idleBehavior => _idleBehavior;
    public Transform[] patrolSequence => _patrolSequence;
    public float patrolDelay => _patrolDelay;
    public int currentPatrol => _currentPatrol;

    public NavMeshAgent NavMesh => _navMesh;
    public Animator Anim => _anim;
    public AnimatorOverrideController defaultAnim => _defaultAnim;
    public AnimatorOverrideController overrideAnim => _overrideAnim;
    public EnemyManager enemyManager => _enemyManager;
    public float viewRadius => _viewRadius;
    public LayerMask targetLayer => _targetLayer;
    public float combatRadius => _combatRadius;

    public bool IsReadyToCombat { get; set; }
    public int ComboCount { get; set; }
    #endregion


    private void Start()
    {
        State = new EnemyStateFactory(this);
        CurrentState = State.Idle();
        CurrentState.Enter();
    }

    private void Update()
    {
        if (CurrentState != null && !_enemyManager.isCursed)
        {
            CurrentState.Update();
        }

        CombatCooldownHandler();
    }

    private void FixedUpdate()
    {
        if (CurrentState != null && !_enemyManager.isCursed)
        {
            CurrentState.FixedUpdate();
        }
    }

    public void ChangePatrolPoint()
    {
        _currentPatrol++;

        if (_currentPatrol > _patrolSequence.Length - 1)
        {
            _currentPatrol = 0;
        }
    }

    #region ENEMY VISUALIZATION
    public Transform GetVisibleTarget()
    {
        Collider[] targetInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius, _targetLayer);

        foreach (Collider collider in targetInViewRadius)
        {
            Transform target = collider.transform;

            Vector3 dirToTarget = (target.position - transform.position).normalized;

            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            float elevationAngle = Vector3.Angle(Vector3.up, dirToTarget);

            if (angleToTarget < _viewAngle / 2 && (elevationAngle > 80 && elevationAngle < 110))
            {
                return target;
            }
        }

        return null;
    }

    public Transform GetChasedTarget()
    {
        Collider[] targetInChaseRadius = Physics.OverlapSphere(transform.position, _chaseRadius, _targetLayer);

        foreach (Collider collider in targetInChaseRadius)
        {
            Transform player = collider.transform;

            if (Vector3.Distance(player.transform.position, this.transform.position) > _chaseRadius * 0.6f)
            {
                return null;
            }

            return collider.transform;
        }

        return null;
    }

    public Transform GetCombatTarget()
    {
        Collider[] targetInCombatRadius = Physics.OverlapSphere(transform.position, _combatRadius, _targetLayer);

        foreach (Collider collider in targetInCombatRadius)
        {
            return collider.transform;
        }

        return null;
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private bool IsInLineOfSight(Transform target)
    {
        Vector3 middlePoint = (transform.position + target.position) / 2;

        Collider[] obstacles = Physics.OverlapSphere(middlePoint, Vector3.Distance(transform.position, target.position) / 2, _obstacleLayer);

        foreach (Collider obstacle in obstacles)
        {
            if (obstacle.transform != target)
            {
                return true;
            }
        }


        return false;
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    public void ChangeTargetLayer(LayerMask layer)
    {
        _targetLayer = layer;
    }
    #endregion

    private void CombatCooldownHandler()
    {
        if (_currentCooldown <= 0 && !IsReadyToCombat)
        {
            IsReadyToCombat = true;
        }
        else
        {
            _currentCooldown -= Time.deltaTime;
        }
    }

    public void ResetCombatCooldown()
    {
        _currentCooldown = _delayPerCombo;
        IsReadyToCombat = false;
    }

    #region DEBUGING
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Vector3 viewAngleA = DirFromAngle(-_viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(_viewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * _viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * _viewRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _chaseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _combatRadius);
    }
    #endregion
}

public class EnemyStateFactory
{
    EnemyStateMachine _context;

    public EnemyStateFactory(EnemyStateMachine currentContext)
    {
        _context = currentContext;
    }

    public EnemyIdleState Idle()
    {
        return new EnemyIdleState(_context);
    }

    public EnemyChaseState Chase()
    {
        return new EnemyChaseState(_context);
    }

    public EnemyPrepareState Prepare() //Prepare to combat
    {
        return new EnemyPrepareState(_context);
    }

    public EnemyCombatState Combat()
    {
        return new EnemyCombatState(_context);
    }
}
