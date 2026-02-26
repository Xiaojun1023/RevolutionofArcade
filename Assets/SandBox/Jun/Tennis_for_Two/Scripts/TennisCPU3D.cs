using UnityEngine;

public class TennisCPU3D : MonoBehaviour
{
    public AngleController3D angleController;
    public TennisLauncher3D launcher;
    public BallReset3D ballReset;
    public Rigidbody ballRb;

    [Header("AI difficulty")]
    public float minAngle = 25f;
    public float maxAngle = 65f;
    public float reactionDelay = 0.35f;

    [Header("Court")]
    public float cpuSideX = 0.0f;
    public float resetBuffer = 0.1f;

    [Header("Bounce behavior")]
    [Range(0f, 1f)]
    public float waitForBounceChance = 0.4f;

    [Header("Hit height window")]
    public float hitWindowHeight = 0.6f;

    bool decidedThisRally;
    bool waitForBounceThisRally;

    bool waiting;
    bool hasHitThisRally;

    void Update()
    {
        if (!ballRb) return;

        float x = ballRb.position.x;
        float y = ballRb.position.y;
        float vx = ballRb.linearVelocity.x;

        if (hasHitThisRally && x < cpuSideX - resetBuffer)
        {
            hasHitThisRally = false;
            decidedThisRally = false;
            waitForBounceThisRally = false;
            CancelPendingHit();
        }

        if (hasHitThisRally) return;

        bool movingTowardCPU = vx > 0f;
        bool onRightSide = x > cpuSideX + resetBuffer;

        if (!decidedThisRally && movingTowardCPU && onRightSide)
        {
            decidedThisRally = true;
            waitForBounceThisRally = Random.value < waitForBounceChance;
        }

        bool bouncedOnRight = ballReset && ballReset.HasBouncedOnRightSide();

        float minHitY = -99999f;
        float maxHitY = 99999f;

        if (ballReset)
        {
            minHitY = ballReset.netHeightY + ballReset.hitAboveNetMargin;
            maxHitY = minHitY + Mathf.Max(0.01f, hitWindowHeight);
        }

        bool inHitWindow = (y >= minHitY) && (y <= maxHitY);

        bool shouldPrepareHit =
            inHitWindow &&
            (
                (!waitForBounceThisRally && movingTowardCPU && onRightSide) ||
                (waitForBounceThisRally && onRightSide && bouncedOnRight)
            );

        if (!waiting && shouldPrepareHit)
        {
            waiting = true;
            Invoke(nameof(Hit), reactionDelay);
        }
    }

    void Hit()
    {
        waiting = false;

        if (!launcher || !ballReset || !ballReset.CanLaunch() || !ballRb || !angleController)
            return;

        if (ballRb.position.x <= cpuSideX)
            return;

        float y = ballRb.position.y;

        float minHitY = ballReset.netHeightY + ballReset.hitAboveNetMargin;
        float maxHitY = minHitY + Mathf.Max(0.01f, hitWindowHeight);

        if (y < minHitY || y > maxHitY)
            return;

        angleController.angle = Random.Range(minAngle, maxAngle);
        launcher.TryLaunch();

        hasHitThisRally = true;
    }

    public void CancelPendingHit()
    {
        CancelInvoke();
        waiting = false;
    }
}
