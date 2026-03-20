using UnityEngine;
using TMPro;

public class BallServeManager : MonoBehaviour
{
    public BallController ball;

    [Header("Out Bounds")]
    public float outX = 2.2f;
    public float outY = -1.0f;

    [Header("Serve Timing")]
    public float startDelay = 0.8f;
    public float respawnDelay = 0.7f;
    public float respawnSpeedScale = 0.9f;
    public float respawnHoverHeight = 0.25f;

    [Header("Rules")]
    public bool useTableTennisServeRule = true;
    public int pointsToWin = 11;
    public int winBy = 2;

    [Header("Side Mapping")]
    public bool playerIsLeft = true;

    [Header("Score UI")]
    public TMP_Text leftNameText;
    public TMP_Text scoreText;
    public TMP_Text rightNameText;
    public ScorePulse scorePulse;

    public string leftLabel = "PLAYER";
    public string rightLabel = "AI";

    [Header("End Match")]
    public MatchResultUI matchResultUI;
    public MonoBehaviour playerLookScript;
    public bool freezeTimeOnMatchEnd = false;

    int playerScore;
    int aiScore;

    int serveDir = 1;
    bool waiting;
    bool matchEnded;

    void Start()
    {
        if (ball == null) return;

        UpdateUI();

        ball.Freeze(true);

        if (!matchEnded)
            Invoke(nameof(ServeStart), startDelay);
    }

    void Update()
    {
        if (ball == null) return;
        if (waiting) return;
        if (matchEnded) return;

        Vector3 p = ball.transform.position;

        bool outByX = p.x > outX || p.x < -outX;
        bool outByY = p.y < outY;

        if (!outByX && !outByY) return;

        AwardPointFromCurrentBallState();

        UpdateUI();

        if (scorePulse != null)
            scorePulse.PlayPulse();

        if (TryHandleMatchEnd())
            return;

        UpdateServeDir();
        BeginRespawn();
    }

    void AwardPointFromCurrentBallState()
    {
        Vector3 p = ball.transform.position;

        if (ball.WaitingForOpponentTableBounce && !ball.OpponentTableBounceConfirmed)
        {
            if (ball.LastHitFromLeft)
                AwardPointToRightSide();
            else
                AwardPointToLeftSide();

            return;
        }

        if (p.x > outX)
        {
            AwardPointToLeftSide();
            return;
        }

        if (p.x < -outX)
        {
            AwardPointToRightSide();
            return;
        }

        if (p.y < outY)
        {
            if (ball.LastHitFromLeft)
                AwardPointToRightSide();
            else
                AwardPointToLeftSide();
        }
    }

    void AwardPointToLeftSide()
    {
        if (playerIsLeft)
            playerScore++;
        else
            aiScore++;
    }

    void AwardPointToRightSide()
    {
        if (playerIsLeft)
            aiScore++;
        else
            playerScore++;
    }

    bool TryHandleMatchEnd()
    {
        if (!HasWinner())
            return false;

        matchEnded = true;
        waiting = true;

        CancelInvoke();

        if (ball != null)
            ball.Freeze(true);

        bool playerWon = playerScore > aiScore;

        if (matchResultUI != null)
        {
            if (playerWon)
                matchResultUI.ShowWin();
            else
                matchResultUI.ShowLose();
        }

        if (playerLookScript != null)
            playerLookScript.enabled = false;

        if (freezeTimeOnMatchEnd)
            Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        return true;
    }

    bool HasWinner()
    {
        int maxScore = Mathf.Max(playerScore, aiScore);
        int diff = Mathf.Abs(playerScore - aiScore);

        return maxScore >= pointsToWin && diff >= winBy;
    }

    void UpdateServeDir()
    {
        if (!useTableTennisServeRule)
        {
            serveDir = -serveDir;
            return;
        }

        int total = playerScore + aiScore;
        bool deuce = playerScore >= 10 && aiScore >= 10;
        int interval = deuce ? 1 : 2;

        int block = total / interval;
        serveDir = (block % 2 == 0) ? 1 : -1;
    }

    void BeginRespawn()
    {
        if (matchEnded) return;

        waiting = true;

        Vector3 p = new Vector3(0f, ball.GroundY + respawnHoverHeight, 0f);
        ball.SetPosition(p);
        ball.Freeze(true);

        Invoke(nameof(ServeRespawn), respawnDelay);
    }

    void ServeStart()
    {
        if (matchEnded) return;
        ServeInternal(1f);
    }

    void ServeRespawn()
    {
        if (matchEnded) return;

        ServeInternal(respawnSpeedScale);
        waiting = false;
    }

    void ServeInternal(float speedScale)
    {
        if (matchEnded) return;

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
            leftNameText.text = playerIsLeft ? "PLAYER" : "AI";

        if (rightNameText != null)
            rightNameText.text = playerIsLeft ? "AI" : "PLAYER";

        int leftScore = playerIsLeft ? playerScore : aiScore;
        int rightScore = playerIsLeft ? aiScore : playerScore;

        if (scoreText != null)
            scoreText.text = leftScore.ToString("00") + " : " + rightScore.ToString("00");
    }

    public void ResetMatch()
    {
        CancelInvoke();

        playerScore = 0;
        aiScore = 0;
        serveDir = 1;
        waiting = false;
        matchEnded = false;

        UpdateUI();

        if (matchResultUI != null)
            matchResultUI.HideImmediate();

        if (playerLookScript != null)
            playerLookScript.enabled = true;

        if (freezeTimeOnMatchEnd)
            Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (ball != null)
        {
            Vector3 p = new Vector3(0f, ball.GroundY + respawnHoverHeight, 0f);
            ball.SetPosition(p);
            ball.Freeze(true);
        }

        Invoke(nameof(ServeStart), startDelay);
    }
}