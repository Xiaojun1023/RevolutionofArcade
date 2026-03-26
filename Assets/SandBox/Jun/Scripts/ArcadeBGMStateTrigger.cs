using UnityEngine;

public class ArcadeBGMStateTrigger : MonoBehaviour
{
    [SerializeField] private ArcadeBGMController bgmController;
    [SerializeField] private bool setInsideArcade = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (bgmController == null) return;

        bgmController.SetInsideArcade(setInsideArcade);
    }
}