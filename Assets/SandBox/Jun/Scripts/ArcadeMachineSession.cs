using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArcadeMachineSession : MonoBehaviour
{
    public string miniGameSceneName = "TennisforTwoScene";
    public Camera gameCaptureCamera;
    public string miniGameLayerName = "MiniGame";
    public MonoBehaviour[] disableOnPlay;

    public Transform playerCamera;
    public Transform screenFocusPoint;
    public Transform screenLookAt;

    public float zoomDuration = 0.35f;
    public KeyCode exitKey = KeyCode.Escape;

    public bool forceAspect = true;
    public float contentAspect = 16f / 9f;

    public Texture idleScreenshot;

    Vector3 camPosBefore;
    Quaternion camRotBefore;
    bool camSaved;

    bool loaded;
    Coroutine running;

    void Start()
    {
        if (gameCaptureCamera != null)
            gameCaptureCamera.enabled = false;

        SetPlayerControl(true);
        FillRTWithIdle();
    }

    void Update()
    {
        if (loaded && Input.GetKeyDown(exitKey))
            EndSession();
    }

    public void TryStartSession()
    {
        if (loaded) return;
        if (running != null) return;
        running = StartCoroutine(LoadRoutine());
    }

    public void EndSession()
    {
        if (!loaded) return;
        if (running != null) return;
        running = StartCoroutine(UnloadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(miniGameSceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        int layer = LayerMask.NameToLayer(miniGameLayerName);
        if (layer != -1)
            SetSceneToLayer(miniGameSceneName, layer);

        DisableAllCamerasInScene(miniGameSceneName);

        SetPlayerControl(false);

        yield return ZoomToScreen();

        AlignCaptureCameraToMiniGameAnchor();
        ApplyAspectToCaptureCamera();

        if (gameCaptureCamera != null)
            gameCaptureCamera.enabled = true;

        loaded = true;
        running = null;
    }

    IEnumerator UnloadRoutine()
    {
        if (gameCaptureCamera != null)
            gameCaptureCamera.enabled = false;

        AsyncOperation op = SceneManager.UnloadSceneAsync(miniGameSceneName);
        if (op != null)
            while (!op.isDone) yield return null;

        FillRTWithIdle();

        yield return ZoomBack();

        SetPlayerControl(true);

        loaded = false;
        running = null;
    }

    void FillRTWithIdle()
    {
        if (idleScreenshot == null) return;
        if (gameCaptureCamera == null) return;
        if (gameCaptureCamera.targetTexture == null) return;

        var prev = RenderTexture.active;
        RenderTexture.active = gameCaptureCamera.targetTexture;
        GL.Clear(true, true, Color.black);
        Graphics.Blit(idleScreenshot, gameCaptureCamera.targetTexture);
        RenderTexture.active = prev;
    }

    void DisableAllCamerasInScene(string sceneName)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded) return;

        foreach (GameObject root in s.GetRootGameObjects())
        {
            Camera[] cams = root.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cams.Length; i++)
                cams[i].enabled = false;
        }
    }

    void AlignCaptureCameraToMiniGameAnchor()
    {
        if (gameCaptureCamera == null) return;

        Scene s = SceneManager.GetSceneByName(miniGameSceneName);
        if (!s.isLoaded) return;

        MiniGameCameraAnchor anchor = null;

        foreach (var root in s.GetRootGameObjects())
        {
            anchor = root.GetComponentInChildren<MiniGameCameraAnchor>(true);
            if (anchor != null) break;
        }

        if (anchor == null) return;

        Transform a = anchor.transform;
        Transform c = gameCaptureCamera.transform;

        c.position = a.position;
        c.rotation = a.rotation;

        Camera anchorCam = anchor.GetComponent<Camera>();
        if (anchorCam != null)
        {
            gameCaptureCamera.orthographic = anchorCam.orthographic;
            gameCaptureCamera.fieldOfView = anchorCam.fieldOfView;
            gameCaptureCamera.orthographicSize = anchorCam.orthographicSize;
            gameCaptureCamera.nearClipPlane = anchorCam.nearClipPlane;
            gameCaptureCamera.farClipPlane = anchorCam.farClipPlane;
        }

        gameCaptureCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCaptureCamera.backgroundColor = Color.black;
    }

    void ApplyAspectToCaptureCamera()
    {
        if (!forceAspect) return;
        if (gameCaptureCamera == null) return;
        if (gameCaptureCamera.targetTexture == null) return;

        float rtAspect = (float)gameCaptureCamera.targetTexture.width / gameCaptureCamera.targetTexture.height;
        float desired = Mathf.Max(0.01f, contentAspect);

        Rect r = new Rect(0f, 0f, 1f, 1f);

        if (rtAspect > desired)
        {
            float h = desired / rtAspect;
            float y = (1f - h) * 0.5f;
            r = new Rect(0f, y, 1f, h);
        }
        else if (rtAspect < desired)
        {
            float w = rtAspect / desired;
            float x = (1f - w) * 0.5f;
            r = new Rect(x, 0f, w, 1f);
        }

        gameCaptureCamera.rect = r;
        gameCaptureCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCaptureCamera.backgroundColor = Color.black;
    }

    void SetSceneToLayer(string sceneName, int layer)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded) return;

        foreach (GameObject root in s.GetRootGameObjects())
            SetLayerRecursive(root, layer);
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursive(t.GetChild(i).gameObject, layer);
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableOnPlay == null) return;
        for (int i = 0; i < disableOnPlay.Length; i++)
            if (disableOnPlay[i] != null)
                disableOnPlay[i].enabled = enabled;
    }

    IEnumerator ZoomToScreen()
    {
        if (playerCamera == null || screenFocusPoint == null || screenLookAt == null)
            yield break;

        if (!camSaved)
        {
            camPosBefore = playerCamera.position;
            camRotBefore = playerCamera.rotation;
            camSaved = true;
        }

        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        Vector3 endPos = screenFocusPoint.position;
        Quaternion endRot = Quaternion.LookRotation((screenLookAt.position - endPos).normalized, Vector3.up);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, zoomDuration);
            playerCamera.position = Vector3.Lerp(startPos, endPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }

    IEnumerator ZoomBack()
    {
        if (playerCamera == null || !camSaved)
            yield break;

        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        Vector3 endPos = camPosBefore;
        Quaternion endRot = camRotBefore;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, zoomDuration);
            playerCamera.position = Vector3.Lerp(startPos, endPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        camSaved = false;
    }
}