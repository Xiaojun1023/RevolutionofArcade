using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArcadeMiniGameLoader : MonoBehaviour
{
    public string miniGameSceneName = "TennisforTwoScene";

    public Camera gameCaptureCamera;

    public string miniGameLayerName = "MiniGame";

    bool loaded;

    public void LoadMiniGame()
    {
        if (loaded) return;
        StartCoroutine(LoadRoutine());
    }

    public void UnloadMiniGame()
    {
        if (!loaded) return;
        StartCoroutine(UnloadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(miniGameSceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        int layer = LayerMask.NameToLayer(miniGameLayerName);
        SetSceneToLayer(miniGameSceneName, layer);

        DisableAllCamerasInScene(miniGameSceneName);

        if (gameCaptureCamera != null)
            gameCaptureCamera.enabled = true;

        loaded = true;
    }

    IEnumerator UnloadRoutine()
    {
        if (gameCaptureCamera != null)
            gameCaptureCamera.enabled = false;

        AsyncOperation op = SceneManager.UnloadSceneAsync(miniGameSceneName);
        if (op != null)
        {
            while (!op.isDone) yield return null;
        }

        loaded = false;
    }

    void DisableAllCamerasInScene(string sceneName)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded) return;

        foreach (GameObject root in s.GetRootGameObjects())
        {
            foreach (Camera c in root.GetComponentsInChildren<Camera>(true))
            {
                c.enabled = false;
            }
        }
    }

    void SetSceneToLayer(string sceneName, int layer)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        if (!s.isLoaded) return;

        foreach (GameObject root in s.GetRootGameObjects())
        {
            SetLayerRecursive(root, layer);
        }
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);
        }
    }
}