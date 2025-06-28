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

    private int envID; // Environment ID, if needed

    // Update event to use NetID in EventArgs
    public static event EventHandler<OnGoalScoredEventArgs> OnGoalScored;
    public class OnGoalScoredEventArgs : EventArgs
    {
        public NetID netID;
        public int envID; // Optional: if you want to include environment ID
    }

    private void Awake()
    {
       envID = Enviroment.GetCurrentEnviromentID(gameObject); // Get the environment ID from the parent Enviroment component
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
            netID = netID,
            envID = envID // Pass the environment ID if needed
        });
    }

    public NetID GetNetID()
    {
        return netID;
    }
}
