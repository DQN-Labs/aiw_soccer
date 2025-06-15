using UnityEngine;
using UnityEngine.UI;

public class Scoring : MonoBehaviour
{
    public int score1 = 0; 
    public int score2 = 0; 
    public Text scoreText1; 
    public Text scoreText2; 
    void Start()
    {
        scoreText1.text = "Team 1: " + score1;
        scoreText2.text = "Team 2: " + score2;
    }
    
    public void AddScoreToTeam(int teamNumber)
    {
        if (teamNumber == 1) 
        {
            score1++;
            scoreText1.text = "Team 1: " + score1; 
        }
        else if (teamNumber == 2)
        {
            score2++;
            scoreText1.text = "Team 2: " + score2;
    }


} // I removed the yapping
}