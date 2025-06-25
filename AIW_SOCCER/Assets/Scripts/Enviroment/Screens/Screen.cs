using TMPro;
using UnityEngine;

public class Screen : MonoBehaviour
{
    [SerializeField] protected Canvas canvas; // Canvas for displaying text

    protected void SetCanvasText(string Text)
    {
        if (canvas == null)
        {
            Debug.LogWarning("Canvas is not assigned.");
            return;
        }

        // Find the TextMeshProUGUI component in the canvas
        var canvasText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        if (canvasText == null)
        {
            Debug.LogWarning("No TextMeshProUGUI component found in scoreCanvas.");
            return;
        }

        // Set the text to the name of the screen
        canvasText.text = Text;
    }
}
