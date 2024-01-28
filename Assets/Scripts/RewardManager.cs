using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : Singleton<RewardManager>
{
    public const string MessageOnGiveReward = "Give Reward";

    public GameObject alert;
    public bool isLose;

    private void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            if (isLose)
            {
                alert.SetActive(true);
            }
        }
        else if (scene.name == "Game")
        {
            if (isLose)
            {
                MessagingCenter.Send(this, MessageOnGiveReward);
                isLose = false;
            }
        }
    }
}
