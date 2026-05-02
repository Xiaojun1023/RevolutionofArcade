using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Refs")]
    public Ball ball;
    public TextMeshProUGUI scoreText;
    public MatchResultUI resultUI;
    public ArcadeMachineSession machineSession;

    [Header("Paddles")]
    public MonoBehaviour playerPaddle;
    public MonoBehaviour aiPaddle;

    [Header("Score")]
    public int playerScore = 0;
    public int aiScore = 0;
    public int winScore = 10;

    [Header("Exit")]
    public KeyCode exitKey = KeyCode.E;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pointSfx;
    [Range(0f, 1f)] public float pointSfxVolume = 1f;

    public AudioClip playerWinSfx;
    public AudioClip aiWinSfx;
    [Range(0f, 1f)] public float winSfxVolume = 1f;

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();

        if (resultUI != null)
            resultUI.HideImmediate();

        int dir = Random.value < 0.5f ? -1 : 1;
        if (ball != null)
            ball.ResetBall(dir);
    }

    private void Update()
    {
        if (!gameEnded) return;

        if (Input.GetKeyDown(exitKey))
        {
            if (machineSession != null)
                machineSession.EndSession();
        }
    }

    public void ScorePlayer()
    {
        if (gameEnded) return;

        playerScore++;
        UpdateUI();
        PlaySfx(pointSfx, pointSfxVolume);

        if (playerScore >= winScore)
        {
            EndGame(true);
            return;
        }

        if (ball != null)
            ball.ResetBall(1);
    }

    public void ScoreAI()
    {
        if (gameEnded) return;

        aiScore++;
        UpdateUI();
        PlaySfx(pointSfx, pointSfxVolume);

        if (aiScore >= winScore)
        {
            EndGame(false);
            return;
        }

        if (ball != null)
            ball.ResetBall(-1);
    }

    private void EndGame(bool playerWon)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (ball != null)
            ball.enabled = false;

        if (playerPaddle != null)
            playerPaddle.enabled = false;

        if (aiPaddle != null)
            aiPaddle.enabled = false;

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

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"{playerScore} : {aiScore}";
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip, volume);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }
}