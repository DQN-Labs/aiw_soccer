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

    public GameObject mainMenuPanel;
    public GameObject skinSelectionPanel;
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
        mainMenuPanel.SetActive(false);
        skinSelectionPanel.SetActive(true);
    }

    public void BackToMenuFromSkinSelection()
    {
        skinSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    
    public void GoSettings()
    {
        SceneManager.LoadScene(Settings);
    }
    public void GoGameModeSelection()
    {
        SceneManager.LoadScene(GameModeSelection);
    }
    public void AddKaiGoal(int amount = 1)
    {
        GoalSaver.KaiGoals += amount;
        GoalSaver.SaveScores();
    }
    // Skin Selection
    // public void SelectSkin(int skinIndex)
    // {
    //     PlayerPrefs.SetInt("SelectedSkin", skinIndex);
    //     PlayerPrefs.Save();
    //     Debug.Log("Skin " + skinIndex + " selected.");
    //     FindObjectOfType<CardHandler>()?.UpdateCard();
    // }
    public void SelectSkin(string skinId)
    {
        PlayerPrefs.SetString("SelectedItem", skinId);
        PlayerPrefs.Save();
        Debug.Log("Skin selected: " + skinId);
        // removed the card thing for now
    }
    // here is some dev stuff we can expand later
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared.");
    }
}

