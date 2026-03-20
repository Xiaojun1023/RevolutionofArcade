using TMPro;
using UnityEngine;

public class MatchResultUI : MonoBehaviour
{
    public CanvasGroup rootCanvasGroup;
    public TMP_Text resultText;

    void Awake()
    {
        HideImmediate();
    }

    public void ShowWin()
    {
        Show("YOU WON");
    }

    public void ShowLose()
    {
        Show("YOU LOST");
    }

    public void Show(string message)
    {
        if (resultText != null)
            resultText.text = message;

        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = 1f;
            rootCanvasGroup.interactable = true;
            rootCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void HideImmediate()
    {
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = 0f;
            rootCanvasGroup.interactable = false;
            rootCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}