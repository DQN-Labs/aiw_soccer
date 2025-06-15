using UnityEngine;
using UnityEngine.UI;

public class Scoring : MonoBehaviour
{
    public int score1 = 0; //this declares 1st score and it can be used in any script
    public int score2 = 0; // same goes here
    public Text scoreText1; // text to show the score
    public Text scoreText2; //another one
    void Start()
    {
        scoreText1.text = "Team 1: " + score1;
        scoreText2.text = "Team 2: " + score2;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Goal1")) // this checks if collision
        {
            Debug.Log("ball scored on 1st team");// this prints a message when the ball touches
            score2++;// increase score using teh operator ++
            scoreText2.text = "Team 2: " + score2; //updates the score Text
        }
        else if (collision.collider.CompareTag("Goal2")) // also checks if collision
        {
            Debug.Log("ball scored on 2nd team"); // also prints a message
            score1++;// increase score
            scoreText1.text = "Team 1: " + score1;//updates the score text
        }
    }


}