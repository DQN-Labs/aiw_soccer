using UnityEngine;
using TMPro;

public class ScoreScreen : MonoBehaviour
{
    [SerializeField] private Canvas scoreCanvas; // Prefab for displaying score text

    private int[] scoreAmount = new int[2]; // 0 for Player, 1 for Kai

    private void Start()
    {
        Net.OnGoalScored += HandleGoalScored;
    }

    private void HandleGoalScored(object sender, Net.OnGoalScoredEventArgs e)
    {
        // Add the received scoreIncrease to the current scoreAmount
        scoreAmount[0] += e.netID == NetID.PlayerNet ? 1 : 0;
        scoreAmount[1] += e.netID == NetID.KaiNet ? 1 : 0;
        Debug.Log($"Goal Scored! Player: {scoreAmount[1]}, Kai: {scoreAmount[0]}");
        SetCanvasScore(scoreAmount);
    }

    private void SetCanvasScore(int[] newScoreAmount)
    {
        if (scoreCanvas == null)
        {
            Debug.LogWarning("Score canvas is not assigned.");
            return;
        }

        // Find the TextMeshProUGUI component in the canvas
        var scoreText = scoreCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Debug.LogWarning("No TextMeshProUGUI component found in scoreCanvas.");
            return;
        }

        // Format and set the score text
        scoreText.text = $"{newScoreAmount[0]} - {newScoreAmount[1]}";
    }

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
}
