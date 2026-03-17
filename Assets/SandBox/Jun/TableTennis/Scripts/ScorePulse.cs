using System.Collections;
using UnityEngine;
using TMPro;

public class ScorePulse : MonoBehaviour
{
    public TMP_Text targetText;

    public Vector3 normalScale = Vector3.one;
    public Vector3 pulseScale = new Vector3(1.2f, 1.2f, 1.2f);

    public float growTime = 0.08f;
    public float shrinkTime = 0.12f;

    Coroutine pulseRoutine;

    void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        if (targetText != null)
            targetText.rectTransform.localScale = normalScale;
    }

    public void PlayPulse()
    {
        if (targetText == null) return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        RectTransform rt = targetText.rectTransform;

        float t = 0f;
        while (t < growTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / growTime);
            rt.localScale = Vector3.Lerp(normalScale, pulseScale, k);
            yield return null;
        }

        t = 0f;
        while (t < shrinkTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / shrinkTime);
            rt.localScale = Vector3.Lerp(pulseScale, normalScale, k);
            yield return null;
        }

        rt.localScale = normalScale;
        pulseRoutine = null;
    }
}