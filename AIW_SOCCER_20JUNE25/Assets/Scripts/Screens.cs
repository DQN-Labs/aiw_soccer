using UnityEngine;

public class Screens : MonoBehaviour
{
    [SerializeField] private Ball ball;

    private int[] scoreAmount = new int[2]; // 0 for Player, 1 for Kai

    // Getter for scoreAmount
    public int[] GetScoreAmount()
    {
        return scoreAmount;
    }

    // Setter for scoreAmount
    public void SetScoreAmount(int[] newScoreAmount)
    {
        if (newScoreAmount != null && newScoreAmount.Length == 2)
        {
            scoreAmount = newScoreAmount;
        }
        else
        {
            Debug.LogWarning("Invalid scoreAmount array. It must be non-null and of length 2.");
        }
    }

    private void Start()
    {
        if (ball != null)
        {
            ball.OnGoalScored += HandleGoalScored;
        }
    }

    private void HandleGoalScored(object sender, Ball.OnGoalScoredEventArgs e)
    {
        // Add the received scoreIncrease to the current scoreAmount
        scoreAmount[0] += e.scoreIncrease[0];
        scoreAmount[1] += e.scoreIncrease[1];
        Debug.Log($"Goal Scored! Player: {scoreAmount[0]}, Kai: {scoreAmount[1]}");
    }
}
