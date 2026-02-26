using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSwapScaler : MonoBehaviour
{
    [Header("Anchor")]
    public Transform ballAnchor;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Transition Color")]
    public Color transitionColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Stages (0..4)")]
    public GameObject Falt2DPrefab;
    public GameObject Ball3DPrefab;
    public GameObject BaseballPrefab;
    public GameObject AmericanFootballPrefab;
    public GameObject TennisballPrefab;

    [Header("Input")]
    public KeyCode key = KeyCode.X;
    public int pressesPerSwitch = 3;

    [Header("Transition")]
    public float swapDuration = 0.8f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Per Stage")]
    public Vector3 stage0Scale = Vector3.one;
    public Vector3 stage1Scale = Vector3.one;
    public Vector3 stage2Scale = Vector3.one;
    public Vector3 stage3Scale = Vector3.one;
    public Vector3 stage4Scale = Vector3.one;
    public AnimationCurve scaleEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool scaleDuringTransition = true;

    [Header("Mask The Swap Moment")]
    [Range(0f, 0.25f)] public float holdAtDarkSeconds = 0.06f;

    [Header("Extra Damping")]
    public bool dampSpecularDuringTransition = true;
    [Range(0f, 1f)] public float transitionMetallic = 0f;
    [Range(0f, 1f)] public float transitionSmoothness = 0.05f;

    private int pressCount = 0;
    private int stageIndex = 0;
    private bool isSwapping = false;
    private GameObject currentBall;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int MetallicId = Shader.PropertyToID("_Metallic");
    static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

    private MaterialPropertyBlock mpb;

    struct TintTarget
    {
        public Renderer r;
        public int colorPropId;
        public Color original;

        public bool hasMetallic;
        public float originalMetallic;

        public bool hasSmoothness;
        public float originalSmoothness;
    }

    void Start()
    {
        if (ballAnchor == null)
        {
            Debug.LogError("BallAnchor is not set.");
            return;
        }

        GameObject p0 = GetStagePrefab(0);
        if (p0 == null)
        {
            Debug.LogError("Stage 0 prefab is missing.");
            return;
        }

        currentBall = Instantiate(p0, ballAnchor.position, ballAnchor.rotation, ballAnchor);
        currentBall.transform.localScale = GetStageScale(0);
        stageIndex = 0;
    }

    void Update()
    {
        if (isSwapping) return;

        if (Input.GetKeyDown(key))
        {
            pressCount++;
            if (pressCount >= pressesPerSwitch)
            {
                pressCount = 0;
                int nextStage = Mathf.Min(stageIndex + 1, 4);

                if (nextStage != stageIndex)
                    StartCoroutine(SwapToStage(nextStage));
            }
        }
    }

    IEnumerator SwapToStage(int nextStage)
    {
        isSwapping = true;

        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);

        GameObject nextPrefab = GetStagePrefab(nextStage);
        if (nextPrefab == null)
        {
            Debug.LogError("Next stage prefab is missing.");
            isSwapping = false;
            yield break;
        }

        GameObject oldBall = currentBall;
        if (oldBall == null)
        {
            isSwapping = false;
            yield break;
        }

        float total = Mathf.Max(0.01f, swapDuration);
        float darkenTime = total * 0.45f;
        float brightenTime = total * 0.55f;

        Color darkGray = transitionColor;

        List<TintTarget> oldTargets = CaptureTintTargets(oldBall);

        Vector3 oldScale = oldBall.transform.localScale;
        Vector3 nextScale = GetStageScale(nextStage);

        float t = 0f;
        while (t < darkenTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / darkenTime);
            float e = ease.Evaluate(k);

            ApplyLerpToTargets(oldTargets, darkGray, e);
            ApplyDamping(oldTargets, transitionMetallic, transitionSmoothness);

            if (scaleDuringTransition && oldBall != null)
            {
                float se = scaleEase.Evaluate(k);
                oldBall.transform.localScale = Vector3.Lerp(oldScale, nextScale, se);
            }

            yield return null;
        }

        ApplyTargetsColor(oldTargets, darkGray);
        ApplyDamping(oldTargets, transitionMetallic, transitionSmoothness);
        if (scaleDuringTransition && oldBall != null) oldBall.transform.localScale = nextScale;

        GameObject newBall = Instantiate(nextPrefab, ballAnchor.position, ballAnchor.rotation, ballAnchor);
        newBall.transform.localScale = nextScale;

        List<TintTarget> newTargets = CaptureTintTargets(newBall);
        ApplyTargetsColor(newTargets, darkGray);
        ApplyDamping(newTargets, transitionMetallic, transitionSmoothness);

        var oldRenderers = GetAllRenderers(oldBall);
        var newRenderers = GetAllRenderers(newBall);

        SetRenderersEnabled(newRenderers, false);
        SetRenderersEnabled(oldRenderers, true);

        float hold = Mathf.Max(0f, holdAtDarkSeconds);
        if (hold > 0f) yield return new WaitForSeconds(hold);

        SetRenderersEnabled(oldRenderers, false);
        SetRenderersEnabled(newRenderers, true);

        Destroy(oldBall);

        currentBall = newBall;
        stageIndex = nextStage;

        t = 0f;
        while (t < brightenTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / brightenTime);
            float e = ease.Evaluate(k);

            ApplyLerpFromGrayToOriginal(newTargets, darkGray, e);
            LerpDampingFromTransitionToOriginal(newTargets, transitionMetallic, transitionSmoothness, e);

            yield return null;
        }

        RestoreOriginal(newTargets);
        RestoreDampingOriginal(newTargets);
        ClearPropertyBlocks(newBall);

        isSwapping = false;
    }

    Renderer[] GetAllRenderers(GameObject obj)
    {
        if (obj == null) return new Renderer[0];
        return obj.GetComponentsInChildren<Renderer>(true);
    }

    void SetRenderersEnabled(Renderer[] rs, bool enabled)
    {
        if (rs == null) return;
        for (int i = 0; i < rs.Length; i++)
        {
            if (rs[i] == null) continue;
            rs[i].enabled = enabled;
        }
    }

    GameObject GetStagePrefab(int index)
    {
        switch (index)
        {
            case 0: return Falt2DPrefab;
            case 1: return Ball3DPrefab;
            case 2: return BaseballPrefab;
            case 3: return AmericanFootballPrefab;
            case 4: return TennisballPrefab;
            default: return Falt2DPrefab;
        }
    }

    Vector3 GetStageScale(int index)
    {
        switch (index)
        {
            case 0: return stage0Scale;
            case 1: return stage1Scale;
            case 2: return stage2Scale;
            case 3: return stage3Scale;
            case 4: return stage4Scale;
            default: return stage0Scale;
        }
    }

    void EnsureMPB()
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();
    }

    List<TintTarget> CaptureTintTargets(GameObject obj)
    {
        var list = new List<TintTarget>();
        if (obj == null) return list;

        Renderer[] rs = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rs)
        {
            if (r == null) continue;

            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;

                if (m.HasProperty(BaseColorId))
                {
                    var tt = new TintTarget
                    {
                        r = r,
                        colorPropId = BaseColorId,
                        original = m.GetColor(BaseColorId),

                        hasMetallic = m.HasProperty(MetallicId),
                        originalMetallic = m.HasProperty(MetallicId) ? m.GetFloat(MetallicId) : 0f,

                        hasSmoothness = m.HasProperty(SmoothnessId),
                        originalSmoothness = m.HasProperty(SmoothnessId) ? m.GetFloat(SmoothnessId) : 0f
                    };

                    list.Add(tt);
                    break;
                }
                else if (m.HasProperty(ColorId))
                {
                    var tt = new TintTarget
                    {
                        r = r,
                        colorPropId = ColorId,
                        original = m.GetColor(ColorId),

                        hasMetallic = m.HasProperty(MetallicId),
                        originalMetallic = m.HasProperty(MetallicId) ? m.GetFloat(MetallicId) : 0f,

                        hasSmoothness = m.HasProperty(SmoothnessId),
                        originalSmoothness = m.HasProperty(SmoothnessId) ? m.GetFloat(SmoothnessId) : 0f
                    };

                    list.Add(tt);
                    break;
                }
            }
        }
        return list;
    }

    void ApplyTargetsColor(List<TintTarget> targets, Color c)
    {
        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            tt.r.GetPropertyBlock(mpb);
            mpb.SetColor(tt.colorPropId, c);
            tt.r.SetPropertyBlock(mpb);
        }
    }

    void ApplyLerpToTargets(List<TintTarget> targets, Color to, float e)
    {
        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            Color c = Color.Lerp(tt.original, to, e);

            tt.r.GetPropertyBlock(mpb);
            mpb.SetColor(tt.colorPropId, c);
            tt.r.SetPropertyBlock(mpb);
        }
    }

    void ApplyLerpFromGrayToOriginal(List<TintTarget> targets, Color from, float e)
    {
        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            Color c = Color.Lerp(from, tt.original, e);

            tt.r.GetPropertyBlock(mpb);
            mpb.SetColor(tt.colorPropId, c);
            tt.r.SetPropertyBlock(mpb);
        }
    }

    void RestoreOriginal(List<TintTarget> targets)
    {
        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            tt.r.GetPropertyBlock(mpb);
            mpb.SetColor(tt.colorPropId, tt.original);
            tt.r.SetPropertyBlock(mpb);
        }
    }

    void ApplyDamping(List<TintTarget> targets, float metallic, float smoothness)
    {
        if (!dampSpecularDuringTransition) return;

        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            tt.r.GetPropertyBlock(mpb);

            if (tt.hasMetallic) mpb.SetFloat(MetallicId, metallic);
            if (tt.hasSmoothness) mpb.SetFloat(SmoothnessId, smoothness);

            tt.r.SetPropertyBlock(mpb);
        }
    }

    void LerpDampingFromTransitionToOriginal(List<TintTarget> targets, float metallicFrom, float smoothnessFrom, float e)
    {
        if (!dampSpecularDuringTransition) return;

        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            tt.r.GetPropertyBlock(mpb);

            if (tt.hasMetallic)
            {
                float m = Mathf.Lerp(metallicFrom, tt.originalMetallic, e);
                mpb.SetFloat(MetallicId, m);
            }

            if (tt.hasSmoothness)
            {
                float s = Mathf.Lerp(smoothnessFrom, tt.originalSmoothness, e);
                mpb.SetFloat(SmoothnessId, s);
            }

            tt.r.SetPropertyBlock(mpb);
        }
    }

    void RestoreDampingOriginal(List<TintTarget> targets)
    {
        if (!dampSpecularDuringTransition) return;

        EnsureMPB();

        for (int i = 0; i < targets.Count; i++)
        {
            var tt = targets[i];
            if (tt.r == null) continue;

            tt.r.GetPropertyBlock(mpb);

            if (tt.hasMetallic) mpb.SetFloat(MetallicId, tt.originalMetallic);
            if (tt.hasSmoothness) mpb.SetFloat(SmoothnessId, tt.originalSmoothness);

            tt.r.SetPropertyBlock(mpb);
        }
    }

    void ClearPropertyBlocks(GameObject obj)
    {
        if (obj == null) return;

        Renderer[] rs = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rs)
        {
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
    }
}