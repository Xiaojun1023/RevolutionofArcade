using UnityEngine;
using System.Collections;

public class BallReset3D : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Respawn")]
    public float freezeDuration = 0.5f;

    [Header("Scoring")]
    public float netX = 0f;
    public ScoreManager scoreManager;

    [Header("Bounce rule")]
    public int maxBouncesBeforePoint = 2;
    public float groundHitDebounce = 0.12f;

    [Header("Hit rule")]
    public float netHeightY = -2.5f;
    public float hitAboveNetMargin = 0.25f;
    public bool forbidConsecutiveHits = true;

    Vector3 initialServePosition;
    bool launchLocked;

    int bounceCountOnSameSide = 0;
    bool lastBounceRightSide = false;
    float lastGroundHitTime = -999f;

    float prevX;
    bool pendingResetAfterNetCross;

    enum HitterSide { None, Left, Right }
    HitterSide lastHitter = HitterSide.None;

    bool touchedGroundSinceLastHit = false;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        initialServePosition = transform.position;

        rb.isKinematic = true;
        launchLocked = false;

        bounceCountOnSameSide = 0;
        pendingResetAfterNetCross = false;
        prevX = transform.position.x;

        lastHitter = HitterSide.None;
        touchedGroundSinceLastHit = false;
    }

    void Update()
    {
        float x = transform.position.x;

        if (pendingResetAfterNetCross && rb && !rb.isKinematic)
        {
            bool crossedNet = (prevX - netX) * (x - netX) < 0f;
            if (crossedNet)
            {
                bounceCountOnSameSide = 0;
                pendingResetAfterNetCross = false;
            }
        }

        prevX = x;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("LeftWall"))
        {
            bool isOpponentWallFromLastHit = lastHitter == HitterSide.Right;

            if (isOpponentWallFromLastHit && !touchedGroundSinceLastHit)
            {
                if (scoreManager) scoreManager.AddLeftPoint();
            }
            else
            {
                if (scoreManager) scoreManager.AddRightPoint();
            }

            StartCoroutine(RespawnFreezeAndLock());
            return;
        }

        if (collision.gameObject.CompareTag("RightWall"))
        {
            bool isOpponentWallFromLastHit = lastHitter == HitterSide.Left;

            if (isOpponentWallFromLastHit && !touchedGroundSinceLastHit)
            {
                if (scoreManager) scoreManager.AddRightPoint();
            }
            else
            {
                if (scoreManager) scoreManager.AddLeftPoint();
            }

            StartCoroutine(RespawnFreezeAndLock());
            return;
        }

        if (!collision.gameObject.CompareTag("Ground"))
            return;

        if (Time.time - lastGroundHitTime < groundHitDebounce)
            return;
        lastGroundHitTime = Time.time;

        touchedGroundSinceLastHit = true;

        bool landedRightSide = transform.position.x > netX;

        if (bounceCountOnSameSide == 0)
        {
            bounceCountOnSameSide = 1;
            lastBounceRightSide = landedRightSide;
            return;
        }

        if (landedRightSide == lastBounceRightSide)
        {
            bounceCountOnSameSide++;
        }
        else
        {
            bounceCountOnSameSide = 1;
            lastBounceRightSide = landedRightSide;
            return;
        }

        if (bounceCountOnSameSide >= maxBouncesBeforePoint)
        {
            if (scoreManager)
            {
                if (landedRightSide) scoreManager.AddLeftPoint();
                else scoreManager.AddRightPoint();
            }

            StartCoroutine(RespawnFreezeAndLock());
        }
    }

    IEnumerator RespawnFreezeAndLock()
    {
        launchLocked = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = initialServePosition;
        rb.isKinematic = true;

        bounceCountOnSameSide = 0;
        pendingResetAfterNetCross = false;
        prevX = transform.position.x;

        lastHitter = HitterSide.None;
        touchedGroundSinceLastHit = false;

        yield return new WaitForSeconds(freezeDuration);

        launchLocked = false;
    }

    public bool CanLaunch()
    {
        return !launchLocked;
    }

    public void UnfreezeForLaunch()
    {
        rb.isKinematic = false;
    }

    public void NotifyHit(bool isLeftSide)
    {
        if (bounceCountOnSameSide > 0)
            pendingResetAfterNetCross = true;

        lastHitter = isLeftSide ? HitterSide.Left : HitterSide.Right;
        touchedGroundSinceLastHit = false;
    }

    public bool HasBouncedOnRightSide()
    {
        return bounceCountOnSameSide > 0 && lastBounceRightSide;
    }

    public bool CanSideHit(bool isLeftSide, Rigidbody ballRb)
    {
        if (launchLocked) return false;
        if (!ballRb) return false;

        float minY = netHeightY + hitAboveNetMargin;
        if (ballRb.position.y < minY) return false;

        if (forbidConsecutiveHits)
        {
            HitterSide side = isLeftSide ? HitterSide.Left : HitterSide.Right;
            if (lastHitter == side) return false;
        }

        return true;
    }
}
