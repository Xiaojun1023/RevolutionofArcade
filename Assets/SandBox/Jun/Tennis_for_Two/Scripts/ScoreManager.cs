using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score")]
    public int leftScore;
    public int rightScore;
    public int winScore = 10;

    [Header("UI")]
    public TMP_Text scoreText;
    public MatchResultUI resultUI;

    [Header("Exit")]
    public KeyCode exitKey = KeyCode.E;
    public ArcadeMachineSession machineSession;

    [Header("Freeze")]
    public TennisLauncher3D launcher;
    public BallReset3D ballReset;
    public MonoBehaviour aimController;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pointSfx;
    [Range(0f, 1f)] public float pointSfxVolume = 1f;

    public AudioClip playerWinSfx;
    public AudioClip aiWinSfx;
    [Range(0f, 1f)] public float winSfxVolume = 1f;

    bool gameEnded = false;

    void Start()
    {
        Refresh();

        if (resultUI != null)
            resultUI.HideImmediate();

        if (launcher != null)
            launcher.enabled = true;

        if (ballReset != null)
            ballReset.enabled = true;

        if (aimController != null)
            aimController.enabled = true;
    }

    void Update()
    {
        if (!gameEnded) return;

        if (Input.GetKeyDown(exitKey))
        {
            if (machineSession != null)
                machineSession.EndSession();
        }
    }

    public void AddLeftPoint()
    {
        if (gameEnded) return;

        leftScore++;
        Refresh();
        PlaySfx(pointSfx, pointSfxVolume);
        CheckWin();
    }

    public void AddRightPoint()
    {
        if (gameEnded) return;

        rightScore++;
        Refresh();
        PlaySfx(pointSfx, pointSfxVolume);
        CheckWin();
    }

    void Refresh()
    {
        if (scoreText != null)
            scoreText.text = $"{leftScore} : {rightScore}";
    }

    void CheckWin()
    {
        if (leftScore >= winScore)
        {
            EndGame(true);
        }
        else if (rightScore >= winScore)
        {
            EndGame(false);
        }
    }

    void EndGame(bool playerWon)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (launcher != null)
            launcher.enabled = false;

        if (ballReset != null)
            ballReset.enabled = false;

        if (aimController != null)
            aimController.enabled = false;

        if (resultUI != null)
        {
            if (playerWon)
                resultUI.ShowWin();
            else
                resultUI.ShowLose();
        }

        if (playerWon)
            PlaySfx(playerWinSfx, winSfxVolume);
        else
            PlaySfx(aiWinSfx, winSfxVolume);
    }

    void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip, volume);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }
}