using System;
using UnityEngine;

public class ArcadeGameManager : MonoBehaviour
{
    public static ArcadeGameManager Instance { get; private set; }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    [SerializeField] private bool isGamePaused;
    [SerializeField] private bool isInsideMachineSession;

    [Header("Pause Control")]
    [SerializeField] private MonoBehaviour playerLookScript;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        isGamePaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CanPauseGame())
                TogglePauseGame();
        }
    }

    public void TogglePauseGame()
    {
        if (isGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (!CanPauseGame())
            return;

        isGamePaused = true;
        Time.timeScale = 0f;

        if (playerLookScript != null)
            playerLookScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        if (playerLookScript != null)
            playerLookScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnGameUnpaused?.Invoke(this, EventArgs.Empty);
    }

    public bool CanPauseGame()
    {
        return !isInsideMachineSession;
    }

    public void SetInsideMachineSession(bool value)
    {
        isInsideMachineSession = value;

        if (isInsideMachineSession && isGamePaused)
            ResumeGame();
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }
}