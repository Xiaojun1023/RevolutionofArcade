using UnityEngine;

public class TennisLauncher3D : MonoBehaviour
{
    public Rigidbody ballRb;
    public AngleController3D angleController;
    public BallReset3D ballReset;
    public TrailRenderer trail;
    public RacketFollowerX racketFollower;

    [Header("Launch")]
    public float launchSpeed = 10f;
    public bool isLeft = true;
    public KeyCode hitKey = KeyCode.Space;

    [Header("Audio")]
    public AudioSource hitAudio;

    void Awake()
    {
        if (!hitAudio)
            hitAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(hitKey))
            TryLaunch();
    }

    public void TryLaunch()
    {
        if (!ballRb || !angleController || !ballReset) return;
        if (!ballReset.CanLaunch()) return;
        if (!ballReset.CanSideHit(isLeft, ballRb)) return;

        if (isLeft && ballRb.position.x > -2f)
            return;

        ballReset.UnfreezeForLaunch();
        ballReset.NotifyHit(isLeft);

        if (trail)
            trail.Clear();

        float rad = angleController.angle * Mathf.Deg2Rad;
        float dir = isLeft ? 1f : -1f;

        Vector3 v = new Vector3(
            Mathf.Cos(rad) * launchSpeed * dir,
            Mathf.Sin(rad) * launchSpeed,
            0f
        );

        ballRb.linearVelocity = v;

        if (racketFollower) 
            racketFollower.LockAfterHit();

        if (hitAudio)
            hitAudio.Play();
    }
}