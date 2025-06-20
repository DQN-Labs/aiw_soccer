using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Starting game...");
        
        SceneManager.LoadScene("Test Scene");
    }
}
