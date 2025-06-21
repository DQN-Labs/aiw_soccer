using UnityEngine;

public class Field : MonoBehaviour
{

    [SerializeField] private Player player;
    [SerializeField] private Kai kai;
    [SerializeField] private Ball ball;

    private void Start()
    {
        Net.OnGoalScored += HandleGoalScored;
    }

    private void HandleGoalScored(object sender, Net.OnGoalScoredEventArgs e)
    {
        // Reset positions
        ResetAllPositions();
    }

    private void ResetAllPositions()
    {
        if (ball != null)
        {
            ball.ResetPosition();
            // Stop any motion
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

        }

        // TODO: All this should come from the same Interface or base class
        if (player != null)
        {
            player.ResetPosition();
        }

        if (kai != null)
        {
            kai.ResetPosition();
        }
    }
}