using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public const string MessageOnWantToGiveLaugh = "Want To Give Laugh";
    public const string MessageOnGiveLaugh = "Give Laugh";
    public const string MessageUpdateHp = "Update Hp";
    public const string MessageUpdateLaugh = "Update Laugh";

    [Header("Properties")]
    [SerializeField]
    private float _maxHp;
    [SerializeField]
    private float _hp;
    [SerializeField]
    private float _laughGauge;

    [Header("Effects")]
    [SerializeField]
    private GameObject _hitFx;
    [SerializeField]
    private GameObject _coinFx;

    private bool _isCanGiveTotem;
    private bool _isDie;

    public float maxHp => _maxHp;
    public float hp => _hp;

    private void Awake()
    {
        MessagingCenter.Subscribe<RewardManager>(this, RewardManager.MessageOnGiveReward, (sender) =>
        {
            _maxHp += 10;
        });
    }

    private void OnDestroy()
    {
        MessagingCenter.Unsubscribe<RewardManager>(this, RewardManager.MessageOnGiveReward);
    }

    void Start()
    {
        _hp = _maxHp;
        MessagingCenter.Send(this, MessageUpdateHp);
        MessagingCenter.Send(this, MessageUpdateLaugh, _laughGauge);
    }

    private void Update()
    {
        if (_isCanGiveTotem)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                float laugh = _laughGauge;
                MessagingCenter.Send(this, MessageOnGiveLaugh, laugh);
                _laughGauge = 0;

                MessagingCenter.Send(this, MessageUpdateLaugh, _laughGauge);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDie) return;

        _hp -= damage;
        CameraShake.instance.ShakeCamera(0.3f);
        Instantiate(_hitFx, transform.position + transform.up, Quaternion.identity);
        MessagingCenter.Send(this, MessageUpdateHp);

        if (_hp <= 0)
        {           
            _isDie = true;
            GameManager.instance.Gameover(GameManager.OverType.Gameover);
        }
    }
    
    public void GetLaugh(float laugh)
    {
        _laughGauge += laugh;

        if (_laughGauge >= 25f)
        {
            _laughGauge = 25f;
        }

        MessagingCenter.Send(this, MessageUpdateLaugh, _laughGauge);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Laugh"))
        {
            GetLaugh(Random.Range(3f, 5f));
            Instantiate(_coinFx, transform.position, Quaternion.identity);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Totem"))
        {
            _isCanGiveTotem = true;
            MessagingCenter.Send(this, MessageOnWantToGiveLaugh, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Totem"))
        {
            _isCanGiveTotem = false;
            MessagingCenter.Send(this, MessageOnWantToGiveLaugh, false);
        }
    }
}
