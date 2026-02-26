using UnityEngine;

public class AngleController3D : MonoBehaviour
{
    [Header("Angle (degrees)")]
    public float angle = 45f;
    public float minAngle = 10f;
    public float maxAngle = 80f;
    public float rotateSpeed = 80f;

    [Header("Keys")]
    public KeyCode decreaseKey = KeyCode.W;
    public KeyCode increaseKey = KeyCode.S;

    void Update()
    {
        if (Input.GetKey(decreaseKey))
            angle -= rotateSpeed * Time.deltaTime;

        if (Input.GetKey(increaseKey))
            angle += rotateSpeed * Time.deltaTime;

        angle = Mathf.Clamp(angle, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
