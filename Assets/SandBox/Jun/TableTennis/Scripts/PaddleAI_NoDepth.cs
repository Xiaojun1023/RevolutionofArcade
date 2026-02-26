using UnityEngine;

public class PaddleAI_NoDepth : MonoBehaviour
{
    public Transform ball;
    public float followSpeed = 10f;
    public float reactionDelay = 0.06f;

    public float yMin = 0.7f;
    public float yMax = 1.5f;
    public float zMin = -0.8f;
    public float zMax = 0.8f;

    float fixedX;
    float targetY;
    float targetZ;
    float nextUpdate;

    void Start()
    {
        fixedX = transform.position.x;
        targetY = transform.position.y;
        targetZ = transform.position.z;
        nextUpdate = 0f;
    }

    void Update()
    {
        if (ball == null) return;

        if (Time.time >= nextUpdate)
        {
            targetY = Mathf.Clamp(ball.position.y, yMin, yMax);
            targetZ = Mathf.Clamp(ball.position.z, zMin, zMax);
            nextUpdate = Time.time + reactionDelay;
        }

        Vector3 pos = transform.position;
        pos.x = fixedX;
        pos.y = Mathf.Lerp(pos.y, targetY, followSpeed * Time.deltaTime);
        pos.z = Mathf.Lerp(pos.z, targetZ, followSpeed * Time.deltaTime);
        transform.position = pos;
    }
}