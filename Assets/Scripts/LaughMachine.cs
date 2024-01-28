using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LaughMachine : MonoBehaviour
{
    public const string MessageOnUpdateLaughMachine = "Update Laugh Machine";
    public const string MessageWantToUpgradeSpell = "Want To Upgrade Spell";
    public const string MessageWantToUpgradeCastSpeed = "Want To Upgrade Cast Speed";

    [SerializeField]
    private ParticleSystem _totemFx;
    [SerializeField]
    private ParticleSystem _totemGetLaughFx;
    [SerializeField]
    private AudioSource _totemSfx;
    [SerializeField]
    private float _currentLaugh;
    [SerializeField]
    private Sprite[] _upgradeSprites;
    [SerializeField]
    private Image _upgradeAlert;

    private bool _reached25;
    private bool _reached50;
    private bool _reached75;

    public float CurrentLaugh => _currentLaugh;

    private void Awake()
    {
        MessagingCenter.Subscribe<PlayerManager, float>(this, PlayerManager.MessageOnGiveLaugh, (sender, laugh) =>
        {
            GetLaugh(laugh);
        });
    }

    private void OnDestroy()
    {
        MessagingCenter.Unsubscribe<PlayerManager, float>(this, PlayerManager.MessageOnGiveLaugh);
    }

    private void Start()
    {
        MessagingCenter.Send(this, MessageOnUpdateLaughMachine, _currentLaugh);
    }

    private void GetLaugh(float laugh)
    {
        _currentLaugh += laugh;
        _totemGetLaughFx.Play();
        _totemSfx.Play();
        MessagingCenter.Send(this, MessageOnUpdateLaughMachine, _currentLaugh);

        if (_currentLaugh >= 100)
        {
            GameManager.instance.Gameover(GameManager.OverType.Totem);
            _totemFx.Play();
        }
        else if (_currentLaugh >= 75)
        {
            if (_reached75) return;

            _totemFx.Play();
            _upgradeAlert.sprite = _upgradeSprites[2];
            _upgradeAlert.gameObject.GetComponent<CanvasGroup>().LeanAlpha(1, 0.5f).setOnComplete(() =>
            {
                _upgradeAlert.gameObject.GetComponent<CanvasGroup>().LeanAlpha(0, 0.5f).setDelay(2f);
            });

            GameObject[] mobs = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] allies = mobs.Where(x => x.GetComponent<EnemyManager>().type == EnemyManager.Type.Ally).ToArray();
            foreach (GameObject ally in allies)
            {
                ally.GetComponent<EnemyStateMachine>().laughDamage += 0.5f;
            }

            _reached75 = true;
        }
        else if (_currentLaugh >= 50)
        {
            if (_reached50) return;

            _totemFx.Play();
            _upgradeAlert.sprite = _upgradeSprites[1];
            _upgradeAlert.gameObject.GetComponent<CanvasGroup>().LeanAlpha(1, 0.5f).setOnComplete(() =>
            {
                _upgradeAlert.gameObject.GetComponent<CanvasGroup>().LeanAlpha(0, 0.5f).setDelay(2f);
            });

            MessagingCenter.Send(this, MessageWantToUpgradeSpell);

            _reached50 = true;
        }
        else if (_currentLaugh >= 25)
        {
            if (_reached25) return;

            _totemFx.Play();
            _upgradeAlert.sprite = _upgradeSprites[0];
            _upgradeAlert.gameObject.GetComponent<CanvasGroup>().LeanAlpha(1, 0.5f).setOnComplete(() =>
            {
                _upgradeAlert.gameObject.GetComponent<CanvasGroup>().LeanAlpha(0, 0.5f).setDelay(2f);
            });

            MessagingCenter.Send(this, MessageWantToUpgradeCastSpeed);

            _reached25 = true;
        }
    }
}
