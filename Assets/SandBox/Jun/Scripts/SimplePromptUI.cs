using TMPro;
using UnityEngine;

public class SimplePromptUI : MonoBehaviour
{
    public TextMeshProUGUI promptText;

    void Awake()
    {
        if (promptText == null)
            promptText = GetComponent<TextMeshProUGUI>();

        Hide();
    }

    public void Show(string message)
    {
        if (promptText == null) return;

        promptText.gameObject.SetActive(true);
        promptText.text = message;
    }

    public void Hide()
    {
        if (promptText == null) return;

        promptText.gameObject.SetActive(false);
    }
}