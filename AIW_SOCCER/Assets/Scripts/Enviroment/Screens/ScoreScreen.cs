using UnityEngine;
using TMPro;

public class ScoreScreen : Screen
{

    [Header("Configurations")]
    [SerializeField] private bool isSingleScoreScreen = false; // If true, only one score will be displayed

    [SerializeField] private Net net;

    private int envID; // Environment ID, if needed

    private int[] scoreAmount = new int[2]; // 0 for Player, 1 for Kai

    private void Awake()
    {
        envID = Enviroment.GetCurrentEnviromentID(gameObject); // Get the environment ID from the parent Enviroment component
        if (isSingleScoreScreen && net == null)
        {
            Debug.LogWarning("Single Score Screen mode selected, but no net assigned");
        }
    }

    private void OnValidate()
    {
        if (isSingleScoreScreen && net)
        {
            SetCanvasText(new int[] { 0, 0 });
        }
        else
        {
            SetCanvasText(new int[] { 0, 0 }); // Default to AlbertNet for initial display
        }
    }

    private void Start()
    {
        if (isSingleScoreScreen && net == null)
        {
            Debug.LogWarning("Single Score Screen mode selected, but no net assigned");
            return;
        }

        Net.OnGoalScored += HandleGoalScored;
    }

    private void HandleGoalScored(object sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != envID) return;

        // Add the received scoreIncrease to the current scoreAmount
        scoreAmount[0] += e.netID == NetID.AlbertNet ? 1 : 0;
        scoreAmount[1] += e.netID == NetID.KaiNet ? 1 : 0;
        //Debug.Log($"Goal Scored! Albert's Team: {scoreAmount[1]}, Kai's Team: {scoreAmount[0]}");
        SetCanvasText(scoreAmount);
    }

    private void SetCanvasText(int[] newScoreAmount)
    {
        if (canvas == null)
        {
            Debug.LogWarning("Score canvas is not assigned.");
            return;
        }

        // Find the TextMeshProUGUI component in the canvas
        var scoreText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Debug.LogWarning("No TextMeshProUGUI component found in scoreCanvas.");
            return;
        }

        // Format and set the score text
        if (!isSingleScoreScreen) scoreText.text = $"{newScoreAmount[0]} - {newScoreAmount[1]}";
        else if (isSingleScoreScreen && net != null) {
            scoreText.text = net.GetNetID() == NetID.AlbertNet ? $"{newScoreAmount[1]}" : $"{newScoreAmount[0]}";
        }

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
