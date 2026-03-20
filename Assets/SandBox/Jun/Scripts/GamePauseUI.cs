using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    public static GamePauseUI Instance { get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    void Awake()
    {
        Instance = this;

        resumeButton.onClick.AddListener(() =>
        {
            if (ArcadeGameManager.Instance != null)
                ArcadeGameManager.Instance.TogglePauseGame();
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    void Start()
    {
        if (ArcadeGameManager.Instance != null)
        {
            ArcadeGameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
            ArcadeGameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        }

        Hide();
    }

    void OnDestroy()
    {
        if (ArcadeGameManager.Instance != null)
        {
            ArcadeGameManager.Instance.OnGamePaused -= GameManager_OnGamePaused;
            ArcadeGameManager.Instance.OnGameUnpaused -= GameManager_OnGameUnpaused;
        }
    }

    void GameManager_OnGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    void GameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        resumeButton.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}