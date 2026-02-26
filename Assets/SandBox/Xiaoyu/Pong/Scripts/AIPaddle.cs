using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class AIPaddle : MonoBehaviour
{
    public Ball ball;
    public float moveSpeed = 9f;

    [Header("AI Behavior")]
    public float reactionDelay = 0.08f;
    public float trackingError = 0.4f;
    [Range(0f, 1f)] public float missChance = 0.18f;
    public float missErrorBoost = 2.2f;
    public float returnToCenterY = 0f;

    private Rigidbody2D rb;
    private Rigidbody2D ballRb;
    private Collider2D col;

    private float minY;
    private float maxY;

    private float nextReactTime = 0f;
    private bool decidedThisRally = false;
    private bool missThisRally = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        CacheBounds();
    }

    private void CacheBounds()
    {
        float camHalfH = Camera.main.orthographicSize;
        float halfPaddleH = col.bounds.extents.y;

        minY = -camHalfH + halfPaddleH;
        maxY = camHalfH - halfPaddleH;
    }

    private void Start()
    {
        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (ball == null || ballRb == null) return;

        bool ballComingToAI = ballRb.linearVelocity.x > 0.1f;

        if (ballComingToAI && !decidedThisRally && ball.transform.position.x < 0f)
        {
            decidedThisRally = true;
            missThisRally = (Random.value < missChance);
        }
        if (!ballComingToAI)
        {
            decidedThisRally = false;
            missThisRally = false;
        }

        if (ballComingToAI)
        {
            if (Time.time >= nextReactTime)
            {
                nextReactTime = Time.time + reactionDelay;

                float err = trackingError * (missThisRally ? missErrorBoost : 1f);
                float targetY = ball.transform.position.y + Random.Range(-err, err);
                MoveTowardY(targetY);
            }
        }
        else
        {
            MoveTowardY(returnToCenterY);
        }
    }

    private void MoveTowardY(float targetY)
    {
        Vector2 pos = rb.position;
        float clampedTargetY = Mathf.Clamp(targetY, minY, maxY);

        float newY = Mathf.MoveTowards(pos.y, clampedTargetY, moveSpeed * Time.fixedDeltaTime);
        newY = Mathf.Clamp(newY, minY, maxY);

        rb.MovePosition(new Vector2(pos.x, newY));
    }
}
