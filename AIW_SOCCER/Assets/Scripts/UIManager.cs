using UnityEngine;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{

    public const string FieldScene = "Field";
    public const string SmallRoom = "Small Room";
    public const string MainScene = "Main Scene";
    public const string SkinSelection = "SkinSelection";
    public const string Settings = "Settings";
    public const string GameModeSelection = "GameModeSelection";
    public void StartGame()
    {
        SceneManager.LoadScene(FieldScene);
    }

    public void StartTrainingGame()
    {
        SceneManager.LoadScene(SmallRoom);
    }

    public void GoMenu()
    {
        SceneManager.LoadScene(MainScene);
    }
    public void GoSkinSelection()
    {
        SceneManager.LoadScene(SkinSelection);
    }
    public void GoSettings()
    {
        SceneManager.LoadScene(Settings);
    }
    public void GoGameModeSelection()
    {
        SceneManager.LoadScene(GameModeSelection);
    }
    // Skin Selection
    public void SelectSkin(int skinIndex)
    {
        PlayerPrefs.SetInt("SelectedSkin", skinIndex);
        PlayerPrefs.Save();
        Debug.Log("Skin " + skinIndex + " selected.");
        FindObjectOfType<CardHandler>()?.UpdateCard();
    }
    // here is some dev stuff we can expand later
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared.");
    }
}