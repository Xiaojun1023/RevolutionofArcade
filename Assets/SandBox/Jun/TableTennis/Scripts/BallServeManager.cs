using UnityEngine;
using TMPro;

public class BallServeManager : MonoBehaviour
{
    public BallController ball;

    public float outX = 2.2f;

    public float startDelay = 0.8f;
    public float respawnDelay = 0.7f;

    public float respawnSpeedScale = 0.9f;

    public float respawnHoverHeight = 0.25f;

    public bool useTableTennisServeRule = true;

    public TMP_Text scoreBoardText;
    public string leftLabel = "PLAYER";
    public string rightLabel = "CPU";

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

        float x = ball.transform.position.x;

        if (x > outX)
        {
            leftScore++;
            UpdateUI();
            UpdateServeDir();
            BeginRespawn();
        }
        else if (x < -outX)
        {
            rightScore++;
            UpdateUI();
            UpdateServeDir();
            BeginRespawn();
        }
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
        if (scoreBoardText != null)
            scoreBoardText.text = leftLabel + " " + leftScore + "  -  " + rightScore + " " + rightLabel;
    }
}