using UnityEngine;

public class BallController : MonoBehaviour
{
    public float leftPaddleX = -1.35f;
    public float rightPaddleX = 1.35f;

    public float tableY = 0.82f;
    public float ballRadius = 0.1f;

    public float netX = 0f;
    public float netTopY = 0.98f;
    public float netClearance = 0.02f;

    public float zMin = -0.7f;
    public float zMax = 0.7f;

    public float gravity = 9.8f;

    public float bounce = 0.7f;
    public float stickYVel = 0.8f;

    public float hitCooldown = 0.06f;
    public float pushOut = 0.06f;

    public float extraLift = 0f;

    public float rallyLandingMinFromNet = 0.4f;
    public float rallyLandingMaxFromNet = 0.7f;
    public float rallyFlightTime = 0.5f;

    public float serveLandingMinFromNet = 0.95f;
    public float serveLandingMaxFromNet = 1.25f;
    public float serveFlightTime = 0.35f;
    public float serveNetClearance = 0.01f;
    public float serveExtraLift = -0.2f;

    Vector3 vel;
    float nextHitTime;
    bool frozen;

    public float GroundY => tableY + ballRadius;
    public Vector3 Velocity => vel;

    Vector3 lastPos;

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

        if (p.y <= gy && vel.y <= 0f)
        {
            p.y = gy;

            if (Mathf.Abs(vel.y) < stickYVel) vel.y = 0f;
            else vel.y = -vel.y * bounce;
        }

        transform.position = p;
        lastPos = transform.position;
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

    public void Serve(int dirX, float serveSpeedScale)
    {
        float dx = dirX >= 0 ? 1f : -1f;

        Vector3 p = transform.position;

        float landDist = Random.Range(serveLandingMinFromNet, serveLandingMaxFromNet);
        float landingX = netX + dx * landDist;
        float targetZ = Random.Range(zMin, zMax);

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
        frozen = false;
        nextHitTime = 0f;
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

        float landDist = Random.Range(rallyLandingMinFromNet, rallyLandingMaxFromNet);
        float landingX = netX + dx * landDist;
        float targetZ = Random.Range(zMin, zMax);

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