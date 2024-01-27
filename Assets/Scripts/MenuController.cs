using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _menuHud;
    [SerializeField]
    private CanvasGroup _howToHud;
    [SerializeField]
    private Button _playButton;
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Button _howToButton;
    [SerializeField]
    private Button _backToTitleButton;
    [SerializeField]
    private Button _ggjButton;

    private void Start()
    {
        _playButton.onClick.AddListener(Play);
        _howToButton.onClick.AddListener(HowTo);
        _exitButton.onClick.AddListener(Exit);
        _backToTitleButton.onClick.AddListener(BackToTitle);
        _ggjButton.onClick.AddListener(GoToLandingPage);

        _howToHud.alpha = 0f;
        _howToHud.interactable = false;
        _howToHud.blocksRaycasts = false;

        _menuHud.alpha = 1f;
        _menuHud.interactable = true;
        _menuHud.blocksRaycasts = true;

        TransitionManager.Instance.SceneFadeOut();
    }

    private void Play()
    {
        TransitionManager.Instance.SceneFadeIn(1f, () =>
        {
            SceneManager.LoadScene("Game");
        });
    }

    private void HowTo()
    {
        _menuHud.LeanAlpha(0, 0.3f);
        _menuHud.interactable = false;
        _menuHud.blocksRaycasts = false;

        _howToHud.LeanAlpha(1f, 0.3f);
        _howToHud.interactable = true;
        _howToHud.blocksRaycasts = true;
    }

    private void BackToTitle()
    {
        _howToHud.LeanAlpha(0, 0.3f);
        _howToHud.interactable = false;
        _howToHud.blocksRaycasts = false;

        _menuHud.LeanAlpha(1f, 0.3f);
        _menuHud.interactable = true;
        _menuHud.blocksRaycasts = true;
    }

    private void GoToLandingPage()
    {
        Application.OpenURL("https://globalgamejam.org/games/2024/laughing-spell-2");
    }

    private void Exit()
    {
        Application.Quit();
    }
}
