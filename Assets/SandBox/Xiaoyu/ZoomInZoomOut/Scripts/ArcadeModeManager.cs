using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ArcadeModeManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip enterArcadeSfx;
    public AudioClip exitArcadeSfx;

    public string arcadeSceneName = "ArcadeWorld";

    public MainPlayerController mainPlayer;
    public CameraZoomTransition mainCamZoom;
    public Transform screenViewPoint;
    public Canvas arcadeCanvas;

    public float interactDistance = 2.0f;
    public Transform arcadeMachine;

    bool arcadeLoaded;
    bool inArcade;

    ArcadePlayerController arcadePlayerController;

    IEnumerator Start()
    {
        yield return SceneManager.LoadSceneAsync(arcadeSceneName, LoadSceneMode.Additive);
        arcadeLoaded = true;

        arcadePlayerController = Object.FindFirstObjectByType<ArcadePlayerController>(FindObjectsInactive.Include);

        if (arcadeCanvas) 
            arcadeCanvas.gameObject.SetActive(false);

        if (mainCamZoom) 
            mainCamZoom.CacheOriginal();
    }

    void Update()
    {
        if (!arcadeLoaded) 
            return;

        if (GameInput.Instance == null) 
            return;

        if (!inArcade)
        {
            if (GameInput.Instance.MainInteractPressedThisFrame() && CanInteract())
                EnterArcade();
        }
        else
        {
            if (GameInput.Instance.ArcadeExitPressedThisFrame())
                ExitArcade();
        }
    }

    bool CanInteract()
    {
        if (!mainPlayer || !arcadeMachine) 
            return false;
        return Vector3.Distance(mainPlayer.transform.position, arcadeMachine.position) <= interactDistance;
    }

    void EnterArcade()
    {
        inArcade = true;

        if (sfxSource && enterArcadeSfx)
            sfxSource.PlayOneShot(enterArcadeSfx);

        GameInput.Instance.SwitchToArcade();

        if (mainPlayer) 
            mainPlayer.enabled = false;

        if (mainCamZoom && screenViewPoint) 
            mainCamZoom.EnterTo(screenViewPoint);

        if (arcadeCanvas) 
            arcadeCanvas.gameObject.SetActive(true);

        if (arcadePlayerController) 
            arcadePlayerController.enabled = true;
    }

    void ExitArcade()
    {
        inArcade = false;

        if (sfxSource && exitArcadeSfx)
            sfxSource.PlayOneShot(exitArcadeSfx);

        if (arcadePlayerController) 
            arcadePlayerController.enabled = false;

        if (arcadeCanvas) 
            arcadeCanvas.gameObject.SetActive(false);

        if (mainCamZoom) 
            mainCamZoom.ExitBack();

        if (mainPlayer) 
            mainPlayer.enabled = true;

        GameInput.Instance.SwitchToMain();
    }
}
