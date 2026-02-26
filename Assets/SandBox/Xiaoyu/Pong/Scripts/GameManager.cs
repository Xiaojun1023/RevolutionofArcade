using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Refs")]
    public Ball ball;
    public TextMeshProUGUI scoreText;

    [Header("Score")]
    public int playerScore = 0;
    public int aiScore = 0;

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
        int dir = Random.value < 0.5f ? -1 : 1;
        ball.ResetBall(dir);
    }

    public void ScorePlayer()
    {
        playerScore++;
        UpdateUI();
        ball.ResetBall(1);
    }

    public void ScoreAI()
    {
        aiScore++;
        UpdateUI();
        ball.ResetBall(-1);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"{playerScore} : {aiScore}";
    }
}
