using System;
using UnityEngine;

public class CounterScreen : Screen
{
    [SerializeField] private int iterationsCount = 0; // Number of iterations to display

    private int envID; // Environment ID, if needed

    private void Start()
    {
        envID = Enviroment.GetCurrentEnviromentID(gameObject); // Get the environment ID from the parent Enviroment component
        FootballAgent.OnEpisodeEnd += FootballAgent_IncrementIterationsCount; // Subscribe to the episode end event
        TimeScreen.OnTimeLimitReached += TimeScreen_IncrementIterationsCount; // Subscribe to the time limit reached event
    }

    public void FootballAgent_IncrementIterationsCount(object sender, FootballAgent.OnEpisodeEndEventArgs e)
    {
        if (e.envID == envID)
        {
            SetIterationsCount(e.iterationCount);
            SetCanvasText(iterationsCount.ToString());
        }
    }

    public void TimeScreen_IncrementIterationsCount(object sender, TimeScreen.OnTimeLimitReachedEventArgs e)
    {
        if (e.envID == envID)
        {
            iterationsCount++;
            SetCanvasText(iterationsCount.ToString());
        }
    }

    public void SetIterationsCount(int count)
    {
        iterationsCount = count;
        SetCanvasText(count.ToString());
    }

    public int GetIterationsCount()
    {
        return iterationsCount;
    }
}
