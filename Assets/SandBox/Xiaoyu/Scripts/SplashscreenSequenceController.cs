using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SplashscreenSequenceController : MonoBehaviour
{
    [SerializeField] private Texture2D[] logoTextures;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private float overlayFadeOutDuration = 0.35f;
    [SerializeField] [Range(0.1f, 1f)] private float minLogoWidthPercent = 0.45f;
    [SerializeField] [Range(0.1f, 1f)] private float minLogoHeightPercent = 0.2f;
    [SerializeField] [Range(0.1f, 1f)] private float maxLogoWidthPercent = 0.95f;
    [SerializeField] [Range(0.1f, 1f)] private float maxLogoHeightPercent = 0.95f;

    private GameObject overlayRoot;

    private IEnumerator Start()
    {
        if (logoTextures == null || logoTextures.Length == 0)
        {
            yield break;
        }

        if (GetComponent<Canvas>() == null)
        {
            Debug.LogWarning($"{nameof(SplashscreenSequenceController)} requires a Canvas on the same GameObject.");
            yield break;
        }

        RectTransform overlayRect = CreateOverlay(transform);
        CanvasGroup overlayGroup = overlayRoot.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 1f;
        overlayGroup.blocksRaycasts = true;
        overlayGroup.interactable = false;

        RectTransform logoFrame = CreateLogoFrame(overlayRect);
        RawImage logoImage = logoFrame.gameObject.AddComponent<RawImage>();
        logoImage.color = new Color(1f, 1f, 1f, 0f);
        logoImage.raycastTarget = false;

        foreach (Texture2D logoTexture in logoTextures)
        {
            if (logoTexture == null)
            {
                continue;
            }

            logoImage.texture = logoTexture;
            logoImage.SetNativeSize();
            UpdateLogoSize(logoFrame, logoTexture);

            yield return FadeGraphicAlpha(logoImage, 0f, 1f, fadeDuration);
            yield return new WaitForSecondsRealtime(displayDuration);
            yield return FadeGraphicAlpha(logoImage, 1f, 0f, fadeDuration);
        }

        yield return FadeCanvasGroupAlpha(overlayGroup, 1f, 0f, overlayFadeOutDuration);

        if (overlayRoot != null)
        {
            Destroy(overlayRoot);
            overlayRoot = null;
        }
    }

    private void OnDisable()
    {
        if (overlayRoot != null)
        {
            Destroy(overlayRoot);
            overlayRoot = null;
        }
    }

    private RectTransform CreateOverlay(Transform parent)
    {
        overlayRoot = new GameObject("SplashscreenOverlay", typeof(RectTransform), typeof(Image));

        RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.SetParent(parent, false);
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsLastSibling();

        Image background = overlayRoot.GetComponent<Image>();
        background.color = Color.black;
        background.raycastTarget = true;

        return overlayRect;
    }

    private RectTransform CreateLogoFrame(RectTransform parent)
    {
        GameObject logoFrameObject = new GameObject("SplashscreenLogo", typeof(RectTransform));
        RectTransform logoRect = logoFrameObject.GetComponent<RectTransform>();
        logoRect.SetParent(parent, false);
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.anchoredPosition = Vector2.zero;
        return logoRect;
    }

    private void UpdateLogoSize(RectTransform logoRect, Texture2D logoTexture)
    {
        float minWidth = Screen.width * minLogoWidthPercent;
        float minHeight = Screen.height * minLogoHeightPercent;
        float maxWidth = Screen.width * maxLogoWidthPercent;
        float maxHeight = Screen.height * maxLogoHeightPercent;
        float width = logoTexture.width;
        float height = logoTexture.height;
        float targetScale = Mathf.Max(minWidth / width, minHeight / height, 1f);
        float maxAllowedScale = Mathf.Min(maxWidth / width, maxHeight / height);
        float scale = Mathf.Min(targetScale, maxAllowedScale);

        logoRect.sizeDelta = new Vector2(width * scale, height * scale);
    }

    private static IEnumerator FadeGraphicAlpha(Graphic graphic, float startAlpha, float endAlpha, float duration)
    {
        Color color = graphic.color;
        color.a = startAlpha;
        graphic.color = color;

        if (duration <= 0f)
        {
            color.a = endAlpha;
            graphic.color = color;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            graphic.color = color;
            yield return null;
        }

        color.a = endAlpha;
        graphic.color = color;
    }

    private static IEnumerator FadeCanvasGroupAlpha(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        group.alpha = startAlpha;

        if (duration <= 0f)
        {
            group.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        group.alpha = endAlpha;
    }
}
