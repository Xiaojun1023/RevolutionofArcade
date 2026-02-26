using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerPaddle : MonoBehaviour
{
    public float moveSpeed = 10f;

    private Rigidbody2D rb;
    private Collider2D col;

    private float minY;
    private float maxY;

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

    private void FixedUpdate()
    {
        float move = 0f;
        if (Input.GetKey(KeyCode.W)) move = 1f;
        else if (Input.GetKey(KeyCode.S)) move = -1f;

        Vector2 pos = rb.position;
        float newY = pos.y + move * moveSpeed * Time.fixedDeltaTime;
        newY = Mathf.Clamp(newY, minY, maxY);

        rb.MovePosition(new Vector2(pos.x, newY));
    }
}
