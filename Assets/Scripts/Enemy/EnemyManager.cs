using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public const string MessageOnPlayerSelected = "On Player Selected";
    public const string MessageOnPlayerDeselected = "On Player Deselected";

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

    [Header("Effects")]
    [SerializeField]
    private GameObject _laughHitFx;
    [SerializeField]
    private GameObject _trasformFx;
    [SerializeField]
    private GameObject _hitFx;

    [Header("HUD")]
    [SerializeField]
    private Image _hpFill;

    [Header("References")]
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private GameObject _laughPrefab;
    [SerializeField]
    private GameObject[] _enemyForm;
    [SerializeField]
    private GameObject[] _allyForm;
    [SerializeField]
    private LayerMask _enemyLayer;
    [SerializeField]
    private LayerMask _playerLayer;
    [SerializeField]
    private Color _playerColor;
    [SerializeField]
    private Color _enemyColor;
    [SerializeField]
    private Outline _outline;

    [Header("Audio")]
    [SerializeField]
    private AudioSource _allyLaughSfx;
    [SerializeField]
    private AudioSource _enemyLaughSfx;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onTakeDamage;
    [SerializeField]
    private UnityEvent onHeal;

    bool isDie;
    public bool isCursed;
    public Type type => _type;

    #region PUBLIC VARIABLES
    public EnemyStateMachine stateMachine => _enemyStateMachine;
    #endregion

    private void Start()
    {
        _hp = _maxHp;
        _hpFill.fillAmount = _hp / _maxHp;

        if (_type == Type.Ally)
        {
            foreach (GameObject enemy in _enemyForm)
            {
                enemy.SetActive(false);
            }
            foreach (GameObject ally in _allyForm)
            {
                ally.SetActive(true);
            }

            _enemyStateMachine.ChangeTargetLayer(_enemyLayer);
            gameObject.layer = LayerMask.NameToLayer("Player");
            _hpFill.color = _playerColor;
        }
        else if (_type == Type.Enemy)
        {
            foreach (GameObject enemy in _enemyForm)
            {
                enemy.SetActive(true);
            }
            foreach (GameObject ally in _allyForm)
            {
                ally.SetActive(false);
            }

            _enemyStateMachine.ChangeTargetLayer(_playerLayer);
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            _hpFill.color = _enemyColor;
        }
    }

    private void Update()
    {
        if (_hp <= 0)
        {
            Die();
        }
    }

    public void TakeLaughDamage(float damage)
    {
        if (isDie) return;

        _hp -= damage;
        _hpFill.fillAmount = _hp / _maxHp;

        onTakeDamage?.Invoke();

        StopCoroutine(LaughCurse());
        StartCoroutine(LaughCurse());

        Instantiate(_laughHitFx, transform.position + transform.up, Quaternion.identity);
        _enemyLaughSfx.Play();
        _enemyStateMachine.DetectAttack();
    }

    public void TakeDamage(float damage)
    {
        if (isDie) return;
        _hp -= damage;
        _hpFill.fillAmount = _hp / _maxHp;

        Instantiate(_hitFx, transform.position + transform.up, Quaternion.identity);
    }

    IEnumerator LaughCurse()
    {
        isCursed = true;
        _anim.SetTrigger("Laugh");
        yield return new WaitForSeconds(2f);
        isCursed = false;
    }

    public void Heal(float heal)
    {
        if (isDie) return;

        _hp += heal;
        _hpFill.fillAmount = _hp / _maxHp;
        if (_hp >= _maxHp) _hp = _maxHp;

        onHeal?.Invoke();
        StopCoroutine(LaughCurse());
        StartCoroutine(LaughCurse());

        Instantiate(_laughHitFx, transform.position + transform.up, Quaternion.identity);
        _allyLaughSfx.Play();
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
            _enemyStateMachine.ChangeTargetLayer(_enemyLayer);
            gameObject.layer = LayerMask.NameToLayer("Player");

            _type = Type.Ally;
            _hpFill.color = _playerColor;

            _enemyStateMachine.SetBehavior(EnemyStateMachine.IdleBehavior.PATROL);
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
            _enemyStateMachine.ChangeTargetLayer(_playerLayer);
            gameObject.layer = LayerMask.NameToLayer("Enemy");

            _type = Type.Enemy;
            _hpFill.color = _enemyColor;
        }

        Instantiate(_trasformFx, transform.position, Quaternion.identity);
        _enemyStateMachine.CurrentState = _enemyStateMachine.State.Idle();
        _enemyStateMachine.CurrentState.Enter();
        _hp = _maxHp;
        _hpFill.fillAmount = _hp / _maxHp;
        SetOutline(false);
        GameManager.instance.AlliesCount();

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

    private void OnMouseEnter()
    {
        PlayerManager player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        if (player != null)
        {
            if (Vector3.Distance(player.transform.position, this.transform.position) < 10)
            {
                _outline.enabled = true;
                _outline.OutlineColor = Color.white;
            }
        }
    }

    private void OnMouseExit()
    {
        if (_outline.enabled)
        {
            _outline.enabled = false;
        }
    }

    public void SetOutline(bool active)
    {
        _outline.enabled = active;
        if (active)
        {
            if(_type == Type.Enemy)
            {
                _outline.OutlineColor = new Color(1, 0, 0.2f);
            }
            else if (_type == Type.Ally)
            {
                _outline.OutlineColor = new Color(0, 1, 0.4f);
            }
        }
        else
        {
            _outline.OutlineColor = Color.white;
            MessagingCenter.Send(this, MessageOnPlayerDeselected);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magic"))
        {          
            if (_type == Type.Enemy)
            {
                TakeLaughDamage(other.transform.localScale.x * 1.3f);
            }
            else if (_type == Type.Ally)
            {
                Heal(other.transform.localScale.x * 0.8f);
            }           
            Destroy(other.gameObject);
        }
    }
}
