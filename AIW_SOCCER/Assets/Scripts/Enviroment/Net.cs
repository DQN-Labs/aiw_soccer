using System;
using UnityEngine;

// Move NetID outside the class and make it public
public enum NetID { AlbertNet, KaiNet }

public class Net : MonoBehaviour
{
    [SerializeField] private GameObject goalRegister;
    [SerializeField] private NetID netID;

    private float scoreCooldown = 0.5f; // seconds
    private float lastScoreTime = -1f;

    // Update event to use NetID in EventArgs
    static public event EventHandler<OnGoalScoredEventArgs> OnGoalScored;
    public class OnGoalScoredEventArgs : EventArgs
    {
        public NetID netID;
    }

    // Remove OnCollisionEnter, add RegisterGoal to be called by GoalRegister
    public void RegisterGoal()
    {
        if (Time.time - lastScoreTime < scoreCooldown) return;

        lastScoreTime = Time.time;
        Debug.Log($"Scored in {netID}");
        GoalScored(netID);
    }

    private void GoalScored(NetID netID)
    {
        OnGoalScored?.Invoke(this, new OnGoalScoredEventArgs
        {
            netID = netID
        });
    }

    public NetID GetNetID()
    {
        return netID;
    }
}
