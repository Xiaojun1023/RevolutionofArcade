namespace USCG.Core
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    using System.Collections;
    using System.Collections.Generic;

    public class ReloadOpenScenes : MonoBehaviour
    {
        [Tooltip("Pressing the combination of these keys will return to the Arcade Store scene.")]
        [SerializeField]
        private List<KeyCode> KeyCodes = new()
        {
            KeyCode.LeftShift,
            KeyCode.R
        };

        [Tooltip("Name of the Arcade Store scene (must be added in Build Settings).")]
        [SerializeField]
        private string ArcadeStoreSceneName = "ArcadeStore";

        private static ReloadOpenScenes instance = null;
        private static bool bIsReloadingScenes = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                DestroyImmediate(this);
            }
        }

        private void Update()
        {
            if (bIsReloadingScenes || KeyCodes.Count == 0)
            {
                return;
            }

            bool bAreKeyCodesPressed = true;
            foreach (KeyCode key in KeyCodes)
            {
                bAreKeyCodesPressed &= Input.GetKey(key);
            }

            if (bAreKeyCodesPressed)
            {
                bIsReloadingScenes = true;
                StartCoroutine(LoadArcadeStoreScene());
            }
        }

        private IEnumerator LoadArcadeStoreScene()
        {
            Debug.Log($"Loading scene '{ArcadeStoreSceneName}'...");

            AsyncOperation op = SceneManager.LoadSceneAsync(ArcadeStoreSceneName, LoadSceneMode.Single);
            while (op != null && !op.isDone)
            {
                yield return null;
            }

            Debug.Log("Finished loading Arcade Store scene!");
            bIsReloadingScenes = false;
        }
    }
}
