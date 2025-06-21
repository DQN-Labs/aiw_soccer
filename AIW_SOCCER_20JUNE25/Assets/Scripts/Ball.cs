using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Ball : MonoBehaviour
{
    public Vector3 resetPosition = Vector3.zero;
    [SerializeField] private Player player;
    [SerializeField] private Kai kai;

    private float scoreCooldown = 0.5f; // seconds
    private float lastScoreTime = -1f;

    public event EventHandler<OnGoalScoredEventArgs> OnGoalScored;
    public class OnGoalScoredEventArgs : EventArgs
    {
       public int[] scoreIncrease = new int[2]; // 0 for Player, 1 for Kai
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastScoreTime < scoreCooldown) return;

        if (collision.collider.CompareTag("Goal1"))
        {
            lastScoreTime = Time.time;
            Debug.Log("Scored in Goal 1");
            GoalScored(PlayersNet: false);
        }
        else if (collision.collider.CompareTag("Goal2"))
        {
            lastScoreTime = Time.time;
            Debug.Log("Scored in Goal 2");
            GoalScored(PlayersNet: true);
        }
    }

    private void GoalScored(bool PlayersNet)
    {
        ResetAllPositions();

        OnGoalScored?.Invoke(this, new OnGoalScoredEventArgs
        {
            scoreIncrease = new int[] { PlayersNet ? 0 : 1, PlayersNet ? 1 : 0 } // { PLAYER, KAI }
        });
    }

    private void ResetAllPositions()
    {
        transform.position = resetPosition;

        // Stop any motion
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

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