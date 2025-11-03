using UnityEngine;

public class GoalSaver : MonoBehaviour
{
    public static int KaiGoals = 0;
    public static int AlbertGoals = 0;  
    public static void SaveScores()
    {
        PlayerPrefs.SetInt("AlbertGoals", AlbertGoals);
        PlayerPrefs.SetInt("KaiGoals", KaiGoals);
        PlayerPrefs.Save();
    }

    public static void LoadScores()
    {
        AlbertGoals = PlayerPrefs.GetInt("AlbertGoals", 0);
        KaiGoals = PlayerPrefs.GetInt("KaiGoals", 0);
    }
}
