using System;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class TimeScreen : Screen
{
    [SerializeField] private int timeLimit = 60; // Time limit in seconds

    private int currentTime;
    private float timer = 0f;

    private int envID; // Environment ID, if needed

    public static event EventHandler<OnTimeLimitReachedEventArgs> OnTimeLimitReached; // Event to notify when the time limit is reached

    public class OnTimeLimitReachedEventArgs : EventArgs
    {
        public int envID; // Environment ID when the time limit is reached
    }

    private void Start()
    {
        envID = Enviroment.GetCurrentEnviromentID(gameObject); // Get the environment ID from the parent Enviroment component

        FootballAgent.OnEpisodeEnd += HandleEpisodeEnd; // Subscribe to the episode end event

        currentTime = timeLimit; // Initialize current time
        SetCanvasText(currentTime.ToString()); 
    }

    private void Update()
    {
        if (currentTime > 0)
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer -= 1f;
                currentTime--;
                SetCanvasText(currentTime.ToString()); // Update text on the canvas
            }
        }
        else
        {
            Debug.Log("Time limit reached!");
            OnTimeLimitReached?.Invoke(this, new OnTimeLimitReachedEventArgs
            {
                envID = envID // Trigger the event with the environment ID
            }); // Trigger the event when time limit is reached

            ResetTime(); // Reset the time
            SetCanvasText(currentTime.ToString());
        }
    }

    private void HandleEpisodeEnd(object sender, FootballAgent.OnEpisodeEndEventArgs e)
    {
        if (e.envID == envID)
        {
            ResetTime();
            SetCanvasText(currentTime.ToString());
        }
    }

    public void ResetTime()
    {
        currentTime = timeLimit; 
        timer = 0f; 
    }

    public void SetTimeLimit(int limit)
    {
        timeLimit = limit;
        currentTime = limit;
        timer = 0f; 
        SetCanvasText(limit.ToString());
    }

    public int GetTimeLimit()
    {
        return timeLimit; 
    }

    public int GetCurrentTime()
    {
        return currentTime;
    }
}
