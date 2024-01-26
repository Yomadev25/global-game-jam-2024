using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public const string MessageOnPlayerSelected = "On Player Selected";

    public enum Type
    {
        Enemy,
        Ally
    }

    [Header("Enemy Profile")]
    [SerializeField]
    private EnemyStateMachine _enemyStateMachine;

    [Header("Properties")]
    [SerializeField]
    private float _maxHp;
    [SerializeField]
    private float _hp;
    [SerializeField]
    private Type _type;

    [Header("References")]
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private GameObject _laughPrefab;
    [SerializeField]
    private GameObject[] _enemyForm;
    [SerializeField]
    private GameObject[] _allyForm;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onTakeDamage;
    [SerializeField]
    private UnityEvent onHeal;

    bool isDie;

    #region PUBLIC VARIABLES
    public EnemyStateMachine stateMachine => _enemyStateMachine;
    #endregion

    private void Start()
    {
        _hp = _maxHp;
    }

    private void Update()
    {
        if (_hp <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDie) return;

        _hp -= damage;
        _anim.SetTrigger("Hit");

        onTakeDamage?.Invoke();
    }

    public void Heal(float heal)
    {
        if (isDie) return;

        _hp += heal;
        onHeal?.Invoke();
    }

    private void Die()
    {
        if (isDie) return;
        isDie = true;

        if (_type == Type.Enemy)
        {
            Instantiate(_laughPrefab, transform.position, Quaternion.identity);

            foreach (GameObject enemy in _enemyForm)
            {
                enemy.SetActive(false);
            }
            foreach (GameObject ally in _allyForm)
            {
                ally.SetActive(true);
            }

            _type = Type.Ally;
        }
        else if (_type == Type.Ally)
        {
            foreach (GameObject enemy in _enemyForm)
            {
                enemy.SetActive(true);
            }
            foreach (GameObject ally in _allyForm)
            {
                ally.SetActive(false);
            }

            _type = Type.Enemy;
        }

        _hp = _maxHp;
        isDie = false;
    }

    private void OnMouseDown()
    {
        PlayerManager player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        if (player != null)
        {
            if (Vector3.Distance(player.transform.position, this.transform.position) < 10)
            {
                MessagingCenter.Send(this, MessageOnPlayerSelected);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magic"))
        {          
            if (_type == Type.Enemy)
            {
                TakeDamage(1f);              
            }
            else if (_type == Type.Ally)
            {
                Heal(1f);
            }

            Destroy(other.gameObject);
        }
    }
}
