using UnityEngine;

public class RacketFollowerX : MonoBehaviour
{
    public Rigidbody ballRb;
    public float netX = 0f;
    public bool isLeftRacket = true;

    public float followSpeed = 30f;
    public float xMin = -6f;
    public float xMax = 6f;
    public float deadZone = 0.02f;

    public float sideBuffer = 0.02f;

    float fixedY;
    float fixedZ;

    bool lockedAfterHit;
    bool hasLeftOwnSideSinceHit;

    void Awake()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        lockedAfterHit = false;
        hasLeftOwnSideSinceHit = false;
    }

    void Update()
    {
        if (!ballRb) return;

        float ballX = ballRb.position.x;
        bool ballOnLeft = ballX < netX - sideBuffer;
        bool ballOnRight = ballX > netX + sideBuffer;

        bool ballOnOwnSide = isLeftRacket ? ballOnLeft : ballOnRight;
        bool ballOnOtherSide = isLeftRacket ? ballOnRight : ballOnLeft;

        if (lockedAfterHit)
        {
            if (!hasLeftOwnSideSinceHit && ballOnOtherSide)
                hasLeftOwnSideSinceHit = true;

            if (hasLeftOwnSideSinceHit && ballOnOwnSide)
            {
                lockedAfterHit = false;
                hasLeftOwnSideSinceHit = false;
            }

            return;
        }

        if (!ballOnOwnSide) return;

        float targetX = Mathf.Clamp(ballRb.position.x, xMin, xMax);
        float currentX = transform.position.x;

        if (Mathf.Abs(targetX - currentX) < deadZone) return;

        float newX = Mathf.MoveTowards(currentX, targetX, followSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, fixedY, fixedZ);
    }

    public void LockAfterHit()
    {
        lockedAfterHit = true;
        hasLeftOwnSideSinceHit = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ballRb) return;
        if (collision.rigidbody && collision.rigidbody == ballRb)
            LockAfterHit();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ballRb) return;
        Rigidbody r = other.attachedRigidbody;
        if (r && r == ballRb)
            LockAfterHit();
    }
}
