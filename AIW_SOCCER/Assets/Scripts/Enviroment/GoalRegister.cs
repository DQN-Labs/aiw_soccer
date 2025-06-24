using UnityEngine;

public class GoalRegister : MonoBehaviour
{
    [SerializeField] private SoccerAgent ownerAgent;     //Agent defending this goal
    [SerializeField] private SoccerAgent opponentAgent;  //Agent who scores if this goal hit

    private void OnCollisionEnter(Collision collision)
    {
        // check if the colliding object is the ball
        if (collision.gameObject.GetComponent<Ball>() == null)
        {
            Debug.Log($"Collision with {collision.gameObject.name} does not have a Ball component, ignoring.");
            return;
        }

        Debug.Log("GOAL! Ball entered goal!");

        //Defending agent punished
        if (ownerAgent != null)
        {
            ownerAgent.OnGoalScored();
        }

        //Opponent gets reward
        if (opponentAgent != null)
        {
            opponentAgent.OnGoalMade();
        }
    }
}
