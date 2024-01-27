using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaughMachine : MonoBehaviour
{
    public const string MessageOnUpdateLaughMachine = "Update Laugh Machine";

    [SerializeField]
    private ParticleSystem _totemFx;
    [SerializeField]
    private float _currentLaugh;
    private float _currentReached;

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

        if (_currentLaugh >= 100)
        {
            GameManager.instance.Gameover(GameManager.OverType.Totem);
            _totemFx.Play();
        }
        else if (_currentLaugh >= 75)
        {
            _totemFx.Play();
            _currentReached = 75;
        }
        else if (_currentLaugh >= 50)
        {
            _totemFx.Play();
            _currentReached = 50;
        }
        else if (_currentLaugh >= 25)
        {
            _totemFx.Play();
            _currentReached = 25;
        }

        MessagingCenter.Send(this, MessageOnUpdateLaughMachine, _currentLaugh);
    }
}
