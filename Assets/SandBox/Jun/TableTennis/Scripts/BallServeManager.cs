using UnityEngine;
using TMPro;

public class BallServeManager : MonoBehaviour
{
    public BallController ball;

    public float outX = 2.2f;
    public float outY = -1.0f;

    public float startDelay = 0.8f;
    public float respawnDelay = 0.7f;

    public float respawnSpeedScale = 0.9f;
    public float respawnHoverHeight = 0.25f;

    public bool useTableTennisServeRule = true;

    [Header("Score UI")]
    public TMP_Text leftNameText;
    public TMP_Text scoreText;
    public TMP_Text rightNameText;
    public ScorePulse scorePulse;

    public string leftLabel = "PLAYER";
    public string rightLabel = "AI";

    int leftScore;
    int rightScore;

    int serveDir = 1;
    bool waiting;

    void Start()
    {
        if (ball == null) return;

        UpdateUI();

        ball.Freeze(true);
        Invoke(nameof(ServeStart), startDelay);
    }

    void Update()
    {
        if (ball == null) return;
        if (waiting) return;

        Vector3 p = ball.transform.position;

        bool outByX = p.x > outX || p.x < -outX;
        bool outByY = p.y < outY;

        if (!outByX && !outByY) return;

        if (ball.WaitingForOpponentTableBounce && !ball.OpponentTableBounceConfirmed)
        {
            if (ball.LastHitFromLeft)
                rightScore++;
            else
                leftScore++;
        }
        else
        {
            if (p.x > outX)
                leftScore++;
            else if (p.x < -outX)
                rightScore++;
            else if (p.y < outY)
            {
                if (ball.LastHitFromLeft)
                    rightScore++;
                else
                    leftScore++;
            }
        }

        UpdateUI();

        if (scorePulse != null)
            scorePulse.PlayPulse();

        UpdateServeDir();
        BeginRespawn();
    }

    void UpdateServeDir()
    {
        if (!useTableTennisServeRule)
        {
            serveDir = -serveDir;
            return;
        }

        int total = leftScore + rightScore;
        bool deuce = leftScore >= 10 && rightScore >= 10;
        int interval = deuce ? 1 : 2;

        int block = total / interval;
        serveDir = (block % 2 == 0) ? 1 : -1;
    }

    void BeginRespawn()
    {
        waiting = true;

        Vector3 p = new Vector3(0f, ball.GroundY + respawnHoverHeight, 0f);
        ball.SetPosition(p);
        ball.Freeze(true);

        Invoke(nameof(ServeRespawn), respawnDelay);
    }

    void ServeStart()
    {
        ServeInternal(1f);
    }

    void ServeRespawn()
    {
        ServeInternal(respawnSpeedScale);
        waiting = false;
    }

    void ServeInternal(float speedScale)
    {
        Vector3 p = ball.transform.position;
        p.x = 0f;
        p.y = ball.GroundY + respawnHoverHeight;
        p.z = 0f;
        ball.SetPosition(p);

        ball.Serve(serveDir, speedScale);
    }

    void UpdateUI()
    {
        if (leftNameText != null)
            leftNameText.text = leftLabel;

        if (scoreText != null)
            scoreText.text = leftScore.ToString("00") + " : " + rightScore.ToString("00");

        if (rightNameText != null)
            rightNameText.text = rightLabel;
    }
}