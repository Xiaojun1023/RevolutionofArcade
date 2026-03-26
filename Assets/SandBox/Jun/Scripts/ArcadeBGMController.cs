using UnityEngine;

public class ArcadeBGMController : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;

    [Header("Volume")]
    [SerializeField] private float outsideVolume = 0.05f;
    [SerializeField] private float insideVolume = 0.18f;
    [SerializeField] private float machineVolume = 0.06f;
    [SerializeField] private float fadeSpeed = 1.2f;

    private float targetVolume;
    private bool isInsideArcade;
    private bool isInsideMachine;

    private void Start()
    {
        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        isInsideArcade = false;
        isInsideMachine = false;
        RefreshTargetVolume();

        if (bgmSource != null)
        {
            bgmSource.volume = targetVolume;

            if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
    }

    private void Update()
    {
        if (bgmSource == null) return;

        bgmSource.volume = Mathf.MoveTowards(
            bgmSource.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime
        );
    }

    public void SetInsideArcade(bool inside)
    {
        isInsideArcade = inside;
        RefreshTargetVolume();
    }

    public void SetInsideMachine(bool inside)
    {
        isInsideMachine = inside;
        RefreshTargetVolume();
    }

    private void RefreshTargetVolume()
    {
        if (isInsideMachine)
            targetVolume = machineVolume;
        else if (isInsideArcade)
            targetVolume = insideVolume;
        else
            targetVolume = outsideVolume;
    }
}