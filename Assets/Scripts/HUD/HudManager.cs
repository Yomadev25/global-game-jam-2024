using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yoma.ThirdPerson;

public class HudManager : MonoBehaviour
{
    [Header("Gameplay HUD")]
    [SerializeField]
    private CanvasGroup _gameplayHud;
    [SerializeField]
    private Image _hpFill;
    [SerializeField]
    private Image _laughFill;
    [SerializeField]
    private CanvasGroup _chargeBar;
    [SerializeField]
    private Image _chargeFill;
    [SerializeField]
    private Color _chargeDisableColor;
    [SerializeField]
    private Color _chargeEnableColor;
    [SerializeField]
    private Image _laughMachineFill;
    [SerializeField]
    private TMP_Text _alliesText;
    [SerializeField]
    private GameObject _fillLaughAlert;
    [SerializeField]
    private GameObject _targetAlert;

    [Header("Result HUD")]
    [SerializeField]
    private CanvasGroup _resultHud;
    [SerializeField]
    private Sprite[] _resultSprites;
    [SerializeField]
    private Image _resultImage;
    [SerializeField]
    private Button _menuButton;
    [SerializeField]
    private GameObject _alliesTotal;
    [SerializeField]
    private GameObject _timeTotal;
    [SerializeField]
    private GameObject _laughTotal;

    private void Awake()
    {
        MessagingCenter.Subscribe<GameManager, GameManager.OverType>(this, GameManager.MessageOnGameover, (sender, type) =>
        {
            OnGameover(type);
        });

        MessagingCenter.Subscribe<GameManager, int>(this, GameManager.MessageUpdateAlliesCount, (sender, count) =>
        {
            UpdateAlliesCount(count);
        });

        MessagingCenter.Subscribe<PlayerManager>(this, PlayerManager.MessageUpdateHp, (sender) =>
        {
            UpdateHpBar(sender.hp, sender.maxHp);
        });

        MessagingCenter.Subscribe<PlayerManager, float>(this, PlayerManager.MessageUpdateLaugh, (sender, laugh) =>
        {
            UpdateLaughBar(laugh);
        });

        MessagingCenter.Subscribe<PlayerController, float>(this, PlayerController.MessageUpdateCharge, (sender, charge) =>
        {
            if (charge > 0)
            {
                _chargeBar.LeanAlpha(1, 0.2f);
            }
            else
            {
                _chargeBar.LeanAlpha(0, 0.2f);
            }
            UpdateChargeFill(charge);
        });

        MessagingCenter.Subscribe<LaughMachine, float>(this, LaughMachine.MessageOnUpdateLaughMachine, (sender, laugh) =>
        {
            UpdateLaughMachine(laugh);
        });

        MessagingCenter.Subscribe<PlayerManager, bool>(this, PlayerManager.MessageOnWantToGiveLaugh, (sender, active) =>
        {
            OnCloseToTotem(active);
        });

        MessagingCenter.Subscribe<PlayerController>(this, PlayerController.MessageCannotFindTarget, (sender) =>
        {
            StopCoroutine(TargetAlert());
            StartCoroutine(TargetAlert());
        });
    }

    private void OnDestroy()
    {
        MessagingCenter.Unsubscribe<GameManager, GameManager.OverType>(this, GameManager.MessageOnGameover);
        MessagingCenter.Unsubscribe<GameManager, int>(this, GameManager.MessageUpdateAlliesCount);
        MessagingCenter.Unsubscribe<PlayerManager>(this, PlayerManager.MessageUpdateHp);
        MessagingCenter.Unsubscribe<PlayerManager, float>(this, PlayerManager.MessageUpdateLaugh);
        MessagingCenter.Unsubscribe<PlayerController, float>(this, PlayerController.MessageUpdateCharge);
        MessagingCenter.Unsubscribe<LaughMachine, float>(this, LaughMachine.MessageOnUpdateLaughMachine);
        MessagingCenter.Unsubscribe<PlayerManager, bool>(this, PlayerManager.MessageOnWantToGiveLaugh);
        MessagingCenter.Unsubscribe<PlayerController>(this, PlayerController.MessageCannotFindTarget);
    }

    private void Start()
    {
        _resultHud.alpha = 0f;
        _resultHud.interactable = false;
        _resultHud.blocksRaycasts = false;

        _gameplayHud.alpha = 1f;
        _gameplayHud.interactable = true;
        _gameplayHud.blocksRaycasts = true;

        _menuButton.onClick.AddListener(BackToMenu);
    }

    private void UpdateHpBar(float hp, float maxHp)
    {
        _hpFill.fillAmount = hp / maxHp;
    }

    private void UpdateLaughBar(float laugh)
    {
        _laughFill.fillAmount = laugh / 25;
    }

    private void UpdateChargeFill(float charge)
    {
        _chargeFill.color = charge >= 50? _chargeEnableColor : _chargeDisableColor;
        _chargeFill.fillAmount = charge / 100;
    }

    private void UpdateLaughMachine(float laugh)
    {
        _laughMachineFill.fillAmount = laugh / 100;
    }

    private void UpdateAlliesCount(int allies)
    {
        _alliesText.text = allies.ToString();
    }

    private void OnCloseToTotem(bool active)
    {
        if (active)
        {
            _fillLaughAlert.transform.LeanScale(Vector3.one, 0.2f).setEaseOutSine();
        }
        else
        {
            _fillLaughAlert.transform.LeanScale(Vector3.zero, 0.2f).setEaseInSine();
        }
    }

    private void OnGameover(GameManager.OverType type)
    {
        _gameplayHud.LeanAlpha(0f, 0.3f);
        _gameplayHud.interactable = false;
        _gameplayHud.blocksRaycasts = false;

        _resultHud.LeanAlpha(1f, 0.3f);
        _resultHud.interactable = true;
        _resultHud.blocksRaycasts = true;

        _resultImage.sprite = _resultSprites[(int)type];
        TimeSpan duration =  DateTime.Now - GameManager.instance.startTime;

        TMP_Text laugh = _laughTotal.transform.GetChild(0).GetComponent<TMP_Text>();
        laugh.text = FindAnyObjectByType<LaughMachine>().CurrentLaugh.ToString() + " / 100";

        if (type == GameManager.OverType.Allies || type == GameManager.OverType.Totem)
        {
            _alliesTotal.SetActive(false);
            _timeTotal.SetActive(true);

            TMP_Text time = _timeTotal.transform.GetChild(0).GetComponent<TMP_Text>();
            time.text = $"{duration.Minutes.ToString("00")}:{duration.Seconds.ToString("00")}";

            if (type == GameManager.OverType.Allies)
            {
                laugh.text = "INCREDIBLE!!";
            }
        }
        else if (type== GameManager.OverType.Gameover)
        {
            _alliesTotal.SetActive(true);
            _timeTotal.SetActive(false);

            TMP_Text allies = _alliesTotal.transform.GetChild(0).GetComponent<TMP_Text>();
            allies.text = _alliesText.text;
        }
    }

    private void BackToMenu()
    {
        TransitionManager.Instance.SceneFadeIn(1f, () =>
        {
            SceneManager.LoadScene("Menu");
        });
    }

    IEnumerator TargetAlert()
    {
        _targetAlert.SetActive(true);
        yield return new WaitForSeconds(1f);
        _targetAlert.SetActive(false);
    }
}
