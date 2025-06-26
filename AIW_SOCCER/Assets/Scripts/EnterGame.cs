using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{   
    public void StartGame()
    {
        Debug.Log("Starting game...");
        
        SceneManager.LoadScene("Test Scene");
    }

    public void StartTrainingGame(){
	Debug.Log("Starting train scene...");
	SceneManager.LoadScene("Train Scene Test");			
    }
}
