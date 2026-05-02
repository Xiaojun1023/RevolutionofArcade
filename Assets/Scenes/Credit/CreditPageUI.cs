using UnityEngine;
using UnityEngine.UI;

public class CreditPageUI : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Awake()
    {
        backButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        Time.timeScale = 1f;
    }
}