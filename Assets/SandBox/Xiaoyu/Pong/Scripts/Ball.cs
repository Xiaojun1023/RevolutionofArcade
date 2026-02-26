using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class Ball : MonoBehaviour
{
    public float speed = 8f;
    public float paddleBounceFactor = 1.2f;
    public float maxYDirection = 0.85f;

    public AudioClip hitSfx;
    [Range(0f, 1f)] public float paddleHitVolume = 1f;
    [Range(0f, 1f)] public float wallHitVolume = 0.8f;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Vector2 startPos;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    public void ResetBall(int directionX)
    {
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;

        float y = Random.Range(-0.3f, 0.3f);
        Vector2 dir = new Vector2(Mathf.Sign(directionX), y).normalized;
        rb.linearVelocity = dir * speed;
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hitSfx != null && audioSource != null)
        {
            if (collision.collider.CompareTag("Paddle"))
                audioSource.PlayOneShot(hitSfx, paddleHitVolume);
            else if (collision.collider.CompareTag("Wall"))
                audioSource.PlayOneShot(hitSfx, wallHitVolume);
        }

        if (!collision.collider.CompareTag("Paddle")) 
            return;

        Transform paddle = collision.collider.transform;

        float x = Mathf.Sign(transform.position.x - paddle.position.x);
        if (Mathf.Approximately(x, 0f)) x = 1f;

        float paddleHeight = collision.collider.bounds.size.y;
        float offset = (transform.position.y - paddle.position.y) / (paddleHeight * 0.5f);
        offset = Mathf.Clamp(offset, -1f, 1f);

        float y = offset * paddleBounceFactor;
        Vector2 dir = new Vector2(x, Mathf.Clamp(y, -maxYDirection, maxYDirection)).normalized;
        rb.linearVelocity = dir * speed;

        float pushOut = collision.collider.bounds.extents.x + GetComponent<Collider2D>().bounds.extents.x + 0.01f;
        transform.position = new Vector3(paddle.position.x + x * pushOut, transform.position.y, transform.position.z);
    }
}
