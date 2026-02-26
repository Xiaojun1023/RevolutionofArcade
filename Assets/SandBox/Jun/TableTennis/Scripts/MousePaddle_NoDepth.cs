using UnityEngine;

public class MousePaddle_NoDepth : MonoBehaviour
{
    public float sensitivity = 0.018f;
    public float smoothTime = 0.06f;

    public float yMin = 0.7f;
    public float yMax = 1.5f;
    public float zMin = -0.8f;
    public float zMax = 0.8f;

    float fixedX;
    float targetY;
    float targetZ;

    float velY;
    float velZ;

    void Start()
    {
        fixedX = transform.position.x;
        targetY = transform.position.y;
        targetZ = transform.position.z;

        LockCursor(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Input.GetMouseButtonDown(0)) LockCursor(true);

        float dx = Input.GetAxisRaw("Mouse X");
        float dy = Input.GetAxisRaw("Mouse Y");

        targetZ -= dx * sensitivity;
        targetY += dy * sensitivity;

        targetY = Mathf.Clamp(targetY, yMin, yMax);
        targetZ = Mathf.Clamp(targetZ, zMin, zMax);

        Vector3 pos = transform.position;

        pos.x = fixedX;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref velY, smoothTime);
        pos.z = Mathf.SmoothDamp(pos.z, targetZ, ref velZ, smoothTime);

        transform.position = pos;
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;

        velY = 0f;
        velZ = 0f;
    }
}