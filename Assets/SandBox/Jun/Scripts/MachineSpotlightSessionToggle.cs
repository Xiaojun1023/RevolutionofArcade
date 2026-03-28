using UnityEngine;

[RequireComponent(typeof(Light))]
public class MachineSpotlightSessionToggle : MonoBehaviour
{
    [SerializeField] private ArcadeMachineSession machineSession;
    [SerializeField] private bool turnOffInSession = true;
    [SerializeField] private bool useSmoothFade = true;
    [SerializeField] private float fadeSpeed = 8f;

    private Light spotLight;
    private float defaultIntensity;
    private float targetIntensity;

    private void Awake()
    {
        spotLight = GetComponent<Light>();
        defaultIntensity = spotLight.intensity;
        targetIntensity = defaultIntensity;
    }

    private void Start()
    {
        if (machineSession == null)
            machineSession = GetComponentInParent<ArcadeMachineSession>();

        UpdateTargetImmediate();
    }

    private void Update()
    {
        if (machineSession == null || spotLight == null) return;

        bool shouldBeOff = turnOffInSession && machineSession.IsInSession;
        targetIntensity = shouldBeOff ? 0f : defaultIntensity;

        if (useSmoothFade)
        {
            spotLight.intensity = Mathf.MoveTowards(
                spotLight.intensity,
                targetIntensity,
                fadeSpeed * Time.deltaTime
            );

            spotLight.enabled = spotLight.intensity > 0.001f;
        }
        else
        {
            spotLight.intensity = targetIntensity;
            spotLight.enabled = targetIntensity > 0.001f;
        }
    }

    private void UpdateTargetImmediate()
    {
        if (machineSession == null || spotLight == null) return;

        bool shouldBeOff = turnOffInSession && machineSession.IsInSession;
        targetIntensity = shouldBeOff ? 0f : defaultIntensity;

        spotLight.intensity = targetIntensity;
        spotLight.enabled = targetIntensity > 0.001f;
    }
}