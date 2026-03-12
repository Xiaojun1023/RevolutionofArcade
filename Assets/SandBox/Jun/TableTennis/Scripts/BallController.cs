using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Table / Ball")]
    public float tableY = 0.82f;
    public float ballRadius = 0.1f;
    public float netX = 0f;
    public float netTopY = 0.98f;
    public float netClearance = 0.02f;
    public float zMin = -0.7f;
    public float zMax = 0.7f;
    public float gravity = 9.8f;

    [Header("Table Bounds For Bounce")]
    public float tableMinX = -1.4f;
    public float tableMaxX = 1.4f;
    public float tableMinZ = -0.8f;
    public float tableMaxZ = 0.8f;
    public float bounceBoundsPadding = 0.02f;

    [Header("Bounce")]
    public float bounce = 0.7f;
    public float stickYVel = 0.8f;

    [Header("Hit")]
    public float hitCooldown = 0.06f;
    public float pushOut = 0.06f;

    [Header("Rally")]
    public float extraLift = 0f;
    public float rallyLandingMinFromNet = 0.5f;
    public float rallyLandingMaxFromNet = 0.7f;
    public float rallyFlightTime = 0.5f;
    public float safeZPadding = 0.18f;

    [Header("Serve")]
    public float serveLandingMinFromNet = 0.95f;
    public float serveLandingMaxFromNet = 1.25f;
    public float serveFlightTime = 0.45f;
    public float serveNetClearance = 0.01f;
    public float serveExtraLift = -0.2f;
    public float serveSafeZPadding = 0.12f;

    [Header("Top / Back Spin")]
    public float spinInputScale = 0.35f;
    public float maxSpin = 30f;
    public float topspinDownForce = 10f;
    public float spinDecay = 0.2f;
    public float bounceSpinForwardBoost = 0.8f;
    public float bounceSpinVerticalBoost = 0.2f;

    [Header("Side Spin")]
    public float sideSpinInputScale = 0.22f;
    public float maxSideSpin = 10f;
    public float sideSpinCurveForce = 2.2f;
    public float sideSpinDecay = 0.35f;
    public float bounceSideSpinBoost = 0.02f;
    public float sideTargetZInfluence = 0.06f;

    Vector3 vel;
    float nextHitTime;
    bool frozen;

    float spinY;
    float spinZ;

    Vector3 lastPos;

    // ===== Scoring / rally state =====
    public bool LastHitFromLeft { get; private set; }
    public bool WaitingForOpponentTableBounce { get; private set; }
    public bool OpponentTableBounceConfirmed { get; private set; }

    public float GroundY => tableY + ballRadius;
    public Vector3 Velocity => vel;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        if (frozen)
        {
            lastPos = transform.position;
            return;
        }

        float dt = Time.deltaTime;

        Vector3 p0 = transform.position;
        Vector3 velStep = vel;

        velStep.y -= gravity * dt;

        if (Mathf.Abs(spinY) > 0.0001f)
        {
            velStep.y -= spinY * topspinDownForce * dt;
            spinY = Mathf.MoveTowards(spinY, 0f, spinDecay * dt);
        }

        if (Mathf.Abs(spinZ) > 0.0001f)
        {
            velStep.z += spinZ * sideSpinCurveForce * dt;
            spinZ = Mathf.MoveTowards(spinZ, 0f, sideSpinDecay * dt);
        }

        Vector3 p1 = p0 + velStep * dt;

        Vector3 delta = p1 - p0;
        float dist = delta.magnitude;

        if (dist > 0.00001f)
        {
            RaycastHit hit;
            if (Physics.SphereCast(p0, ballRadius, delta.normalized, out hit, dist, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.CompareTag("Paddle"))
                {
                    OnTriggerEnter(hit.collider);
                    lastPos = transform.position;
                    return;
                }
            }
        }

        vel = velStep;
        Vector3 p = p1;

        float gy = GroundY;

        if (IsOverTable(p.x, p.z) && p.y <= gy && vel.y <= 0f)
        {
            p.y = gy;

            // First legal bounce check: did this shot land on opponent side?
            if (WaitingForOpponentTableBounce && !OpponentTableBounceConfirmed)
            {
                bool landedOnOpponentSide =
                    (LastHitFromLeft && p.x > netX) ||
                    (!LastHitFromLeft && p.x < netX);

                if (landedOnOpponentSide)
                {
                    OpponentTableBounceConfirmed = true;
                    WaitingForOpponentTableBounce = false;
                }
            }

            float spinSign = Mathf.Sign(vel.x);
            vel.x += spinSign * spinY * bounceSpinForwardBoost;
            vel.y += spinY * bounceSpinVerticalBoost;
            vel.z += spinZ * bounceSideSpinBoost;

            if (Mathf.Abs(vel.y) < stickYVel) vel.y = 0f;
            else vel.y = -vel.y * bounce;
        }

        transform.position = p;
        lastPos = transform.position;
    }

    bool IsOverTable(float x, float z)
    {
        return x >= tableMinX - bounceBoundsPadding &&
               x <= tableMaxX + bounceBoundsPadding &&
               z >= tableMinZ - bounceBoundsPadding &&
               z <= tableMaxZ + bounceBoundsPadding;
    }

    public void Freeze(bool value)
    {
        frozen = value;
        if (frozen) vel = Vector3.zero;
    }

    public void SetPosition(Vector3 p)
    {
        transform.position = p;
    }

    public void ResetRallyTracking()
    {
        WaitingForOpponentTableBounce = false;
        OpponentTableBounceConfirmed = false;
    }

    public void Serve(int dirX, float serveSpeedScale)
    {
        float dx = dirX >= 0 ? 1f : -1f;

        Vector3 p = transform.position;

        float landDist = Random.Range(serveLandingMinFromNet, serveLandingMaxFromNet);
        float landingX = netX + dx * landDist;

        float safeMinZ = zMin + serveSafeZPadding;
        float safeMaxZ = zMax - serveSafeZPadding;
        float targetZ = Random.Range(safeMinZ, safeMaxZ);

        float T = Mathf.Max(serveFlightTime, 0.15f) / Mathf.Max(serveSpeedScale, 0.01f);

        float vx = (landingX - p.x) / T;
        float vz = (targetZ - p.z) / T;

        float y0 = p.y;
        float yT = GroundY;
        float vy = (yT - y0 + 0.5f * gravity * T * T) / T + serveExtraLift;

        Vector3 v0 = new Vector3(vx, vy, vz);

        if (!ClearsNetCustom(p, v0, T, serveNetClearance))
        {
            float boost = (netTopY + serveNetClearance) - NetYAt(p, v0);
            if (boost > 0f) vy += boost * 1.2f;
        }

        vel = new Vector3(vx, vy, vz);
        spinY = 0f;
        spinZ = 0f;
        frozen = false;
        nextHitTime = 0f;

        ResetRallyTracking();
    }

    void OnTriggerEnter(Collider other)
    {
        if (frozen) return;
        if (!other.CompareTag("Paddle")) return;
        if (Time.time < nextHitTime) return;

        nextHitTime = Time.time + hitCooldown;

        float paddleX = other.transform.position.x;
        float dirX = paddleX < 0f ? 1f : -1f;
        float dx = dirX >= 0 ? 1f : -1f;

        Vector3 p = transform.position;
        p.x = paddleX + dx * pushOut;
        if (p.y < GroundY) p.y = GroundY;
        transform.position = p;

        Vector3 paddleVelocity = GetPaddleVelocity(other);

        spinY = Mathf.Clamp(paddleVelocity.y * spinInputScale, -maxSpin, maxSpin);
        spinZ = Mathf.Clamp(-paddleVelocity.z * sideSpinInputScale, -maxSideSpin, maxSideSpin);

        // Track who hit last, and require next bounce to be on opponent side
        LastHitFromLeft = paddleX < 0f;
        WaitingForOpponentTableBounce = true;
        OpponentTableBounceConfirmed = false;

        float landDist = Random.Range(rallyLandingMinFromNet, rallyLandingMaxFromNet);
        float landingX = netX + dx * landDist;

        float safeMinZ = zMin + safeZPadding;
        float safeMaxZ = zMax - safeZPadding;
        float baseTargetZ = Random.Range(safeMinZ, safeMaxZ);
        float spinOffsetZ = Mathf.Clamp(spinZ * sideTargetZInfluence, -safeZPadding * 0.6f, safeZPadding * 0.6f);
        float targetZ = Mathf.Clamp(baseTargetZ + spinOffsetZ, safeMinZ, safeMaxZ);

        float T = Mathf.Max(rallyFlightTime, 0.15f);

        float vx = (landingX - p.x) / T;
        float vz = (targetZ - p.z) / T;

        float y0 = p.y;
        float yT = GroundY;
        float vy = (yT - y0 + 0.5f * gravity * T * T) / T + extraLift;

        Vector3 v0 = new Vector3(vx, vy, vz);

        if (!ClearsNet(p, v0, T))
        {
            float boost = (netTopY + netClearance) - NetYAt(p, v0);
            if (boost > 0f) vy += boost * 2.2f;
        }

        vel = new Vector3(vx, vy, vz);
    }

    Vector3 GetPaddleVelocity(Collider other)
    {
        MousePaddle_NoDepth playerPaddle = other.GetComponent<MousePaddle_NoDepth>();
        if (playerPaddle != null) return playerPaddle.Velocity;

        PaddleAI_NoDepth aiPaddle = other.GetComponent<PaddleAI_NoDepth>();
        if (aiPaddle != null) return aiPaddle.Velocity;

        return Vector3.zero;
    }

    bool ClearsNet(Vector3 p0, Vector3 v0, float T)
    {
        float vx = v0.x;
        if (Mathf.Abs(vx) < 0.0001f) return true;

        float t = (netX - p0.x) / vx;
        if (t <= 0f || t >= T) return true;

        float y = p0.y + v0.y * t - 0.5f * gravity * t * t;
        return y >= netTopY + netClearance;
    }

    bool ClearsNetCustom(Vector3 p0, Vector3 v0, float T, float clearance)
    {
        float vx = v0.x;
        if (Mathf.Abs(vx) < 0.0001f) return true;

        float t = (netX - p0.x) / vx;
        if (t <= 0f || t >= T) return true;

        float y = p0.y + v0.y * t - 0.5f * gravity * t * t;
        return y >= netTopY + clearance;
    }

    float NetYAt(Vector3 p0, Vector3 v0)
    {
        float vx = v0.x;
        if (Mathf.Abs(vx) < 0.0001f) return p0.y;

        float t = (netX - p0.x) / vx;
        float y = p0.y + v0.y * t - 0.5f * gravity * t * t;
        return y;
    }
}