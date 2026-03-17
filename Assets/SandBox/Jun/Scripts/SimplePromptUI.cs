using TMPro;
using UnityEngine;
using System.Collections;

public class SimplePromptUI : MonoBehaviour
{
    public GameObject promptRoot;
    public TextMeshProUGUI promptText;
    public CanvasGroup canvasGroup;

    public float fadeDuration = 0.12f;

    Coroutine fadeRoutine;

    void Awake()
    {
        if (promptText == null)
            promptText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (promptRoot == null && promptText != null)
            promptRoot = promptText.transform.parent.gameObject;

        if (canvasGroup == null && promptRoot != null)
            canvasGroup = promptRoot.GetComponent<CanvasGroup>();

        if (canvasGroup == null && promptRoot != null)
            canvasGroup = promptRoot.AddComponent<CanvasGroup>();

        HideImmediate();
    }

    public void Show(string message)
    {
        if (promptText == null) return;

        promptText.text = message;

        if (promptRoot != null && !promptRoot.activeSelf)
            promptRoot.SetActive(true);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(1f));
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy)
        {
            HideImmediate();
            return;
        }

        if (promptRoot == null || canvasGroup == null)
        {
            HideImmediate();
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(0f));
    }

    public void HideImmediate()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (promptRoot != null)
            promptRoot.SetActive(false);
        else if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    IEnumerator FadeRoutine(float targetAlpha)
    {
        if (promptRoot != null && !promptRoot.activeSelf)
            promptRoot.SetActive(true);

        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, k);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = targetAlpha;

        if (Mathf.Approximately(targetAlpha, 0f) && promptRoot != null)
            promptRoot.SetActive(false);

        fadeRoutine = null;
    }
}