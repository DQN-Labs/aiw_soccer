using UnityEngine;
using TMPro;

public class ScoreScreen : Screen
{

    [Header("Configurations")]
    [SerializeField] private bool isSingleScoreScreen = false; // If true, only one score will be displayed

    [SerializeField] private Net net;

    private int envID; // Environment ID, if needed
    private FootballEnvController envController;

    private void OnValidate()
    {
        SetCanvasText(new int[] { 0, 0 });
    }

    private void Start()
    {
        envID = FootballEnvController.GetCurrentEnviromentID(gameObject);

        envController = GetComponentInParent<FootballEnvController>();
        if (envController == null)
        {
            Debug.LogWarning("No FootballEnvController found in parent hierarchy.");
            return;
        }

        // Suscribirse al evento de cambio de marcador
        envController.OnScoreChanged += OnScoreChanged;

        // Inicializar la UI con el marcador actual
        SetCanvasText(envController.GetScore());
    }

    private void OnDestroy()
    {
        if (envController != null)
            envController.OnScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int changedEnvID, int kaiScore, int albertScore)
    {
        if (changedEnvID != envID) return;
        SetCanvasText(new int[] { kaiScore, albertScore });
    }

    private void SetCanvasText(int[] newScoreAmount)
    {
        if (canvas == null)
        {
            Debug.LogWarning("Score canvas is not assigned.");
            return;
        }

        var scoreText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Debug.LogWarning("No TextMeshProUGUI component found in scoreCanvas.");
            return;
        }

        if (!isSingleScoreScreen)
            scoreText.text = $"{newScoreAmount[0]} - {newScoreAmount[1]}";
        else if (isSingleScoreScreen && net != null)
            scoreText.text = net.GetNetID() == NetID.AlbertNet ? $"{newScoreAmount[1]}" : $"{newScoreAmount[0]}";
    }
}
