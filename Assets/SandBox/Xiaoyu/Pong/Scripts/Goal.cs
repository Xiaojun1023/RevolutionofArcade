using UnityEngine;

public class Goal : MonoBehaviour
{
    public enum GoalSide { Left, Right }
    public GoalSide side;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null) return;

        if (side == GoalSide.Left)
            GameManager.Instance.ScoreAI();
        else
            GameManager.Instance.ScorePlayer();
    }
}
