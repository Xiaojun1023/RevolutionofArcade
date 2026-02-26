using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraZoomTransition : MonoBehaviour
{
    public float enterDuration = 0.25f;
    public float exitDuration = 0.25f;
    public float zoomFov = 25f;

    Camera cam;

    Transform cachedParent;
    Vector3 originalLocalPos;
    Quaternion originalLocalRot;
    float originalFov;

    Coroutine running;

    void Awake()
    {
        cam = GetComponent<Camera>();
        CacheOriginal();
    }

    public void CacheOriginal()
    {
        cachedParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        originalFov = cam.fieldOfView;
    }

    public void EnterTo(Transform viewPoint)
    {
        if (running != null) 
            StopCoroutine(running);
        running = StartCoroutine(LerpToWorld(viewPoint.position, viewPoint.rotation, zoomFov, enterDuration));
    }

    public void ExitBack()
    {
        if (running != null) 
            StopCoroutine(running);
        running = StartCoroutine(LerpBackToLocal(originalLocalPos, originalLocalRot, originalFov, exitDuration));
    }

    IEnumerator LerpToWorld(Vector3 targetWorldPos, Quaternion targetWorldRot, float targetFov, float duration)
    {
        Vector3 startWorldPos = transform.position;
        Quaternion startWorldRot = transform.rotation;
        float startFov = cam.fieldOfView;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float s = Mathf.SmoothStep(0, 1, t);

            transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, s);
            transform.rotation = Quaternion.Slerp(startWorldRot, targetWorldRot, s);
            cam.fieldOfView = Mathf.Lerp(startFov, targetFov, s);

            yield return null;
        }
    }

    IEnumerator LerpBackToLocal(Vector3 targetLocalPos, Quaternion targetLocalRot, float targetFov, float duration)
    {
        Vector3 startLocalPos = transform.localPosition;
        Quaternion startLocalRot = transform.localRotation;
        float startFov = cam.fieldOfView;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float s = Mathf.SmoothStep(0, 1, t);

            transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, s);
            transform.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, s);
            cam.fieldOfView = Mathf.Lerp(startFov, targetFov, s);

            yield return null;
        }
    }
}
