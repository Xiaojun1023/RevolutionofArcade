using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.New_MainArcadeScene);
        });

        creditButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.CreditPage);
        });

        quitButton.onClick.AddListener(QuitGame);

        Time.timeScale = 1f;
    }

    private void QuitGame()
    {
        Debug.Log("Quit button clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}