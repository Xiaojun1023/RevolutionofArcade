using System.Collections;
using UnityEngine;

public class CoinPulse : MonoBehaviour
{
    public RectTransform target;

    [Header("Scale")]
    public float scaleUp = 1.18f;
    public float duration = 0.18f;

    [Header("Shake")]
    public float shakeAmount = 6f;
    public int shakeVibrato = 6;
    public bool enableShake = true;

    Vector3 baseScale;
    Vector2 baseAnchoredPos;
    Coroutine running;

    void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        if (target != null)
        {
            baseScale = target.localScale;
            baseAnchoredPos = target.anchoredPosition;
        }
    }

    public void PlayPulse()
    {
        if (target == null) return;

        if (running != null)
            StopCoroutine(running);

        target.localScale = baseScale;
        target.anchoredPosition = baseAnchoredPos;

        running = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        float half = duration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);
            float s = Mathf.Lerp(1f, scaleUp, k);
            target.localScale = baseScale * s;

            if (enableShake)
                ApplyShake(k);

            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);
            float s = Mathf.Lerp(scaleUp, 1f, k);
            target.localScale = baseScale * s;

            if (enableShake)
                ApplyShake(1f - k);

            yield return null;
        }

        target.localScale = baseScale;
        target.anchoredPosition = baseAnchoredPos;
        running = null;
    }

    void ApplyShake(float strength01)
    {
        float time = Time.unscaledTime * shakeVibrato * 20f;

        float x = Mathf.Sin(time) * shakeAmount * strength01;
        float y = Mathf.Cos(time * 1.3f) * shakeAmount * 0.35f * strength01;

        target.anchoredPosition = baseAnchoredPos + new Vector2(x, y);
    }
}