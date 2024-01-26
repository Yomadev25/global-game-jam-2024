using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaughMachine : MonoBehaviour
{
    [SerializeField]
    private float _currentLaugh;

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

    private void GetLaugh(float laugh)
    {
        _currentLaugh += laugh;

        if (_currentLaugh >= 100)
        {

        }
        else if (_currentLaugh >= 75)
        {

        }
        else if (_currentLaugh >= 50)
        {

        }
        else if (_currentLaugh >= 25)
        {

        }
    }
}
