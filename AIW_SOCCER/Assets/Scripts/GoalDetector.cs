using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public int teamToScore; 
    public Scoring scoringSystem; 

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            scoringSystem.AddScoreToTeam(teamToScore);
            Debug.Log("Goal for Team " + teamToScore);
        }
    }
}
//I didn't add yapping to this one