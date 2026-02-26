using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int leftScore;
    public int rightScore;

    public TMP_Text scoreText;

    void Start()
    {
        Refresh();
    }

    public void AddLeftPoint()
    {
        leftScore++;
        Refresh();
    }

    public void AddRightPoint()
    {
        rightScore++;
        Refresh();
    }

    void Refresh()
    {
        if (scoreText)
            scoreText.text = $"{leftScore} : {rightScore}";
    }
}
