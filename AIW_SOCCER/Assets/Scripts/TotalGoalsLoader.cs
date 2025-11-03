using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TotalGoalsLoader : MonoBehaviour
{
    public TMP_Text kaiText;
    public TMP_Text albertText;
    public TMP_Text percentageText;
    void Start()
    {
        GoalSaver.LoadScores();
        int kai = GoalSaver.KaiGoals;
        int albert = GoalSaver.AlbertGoals;
        kaiText.text = " " + GoalSaver.KaiGoals;
        albertText.text = " " + GoalSaver.AlbertGoals;

        int total = kai + albert;
        if (total > 0)
        {
            float kaiPercent = (kai / (float)total) * 100f;
            float albertPercent = (albert / (float)total) * 100f;

            percentageText.text = $"Kai: {kaiPercent:F1}%  |  Albert: {albertPercent:F1}%";
        }
    }
}
