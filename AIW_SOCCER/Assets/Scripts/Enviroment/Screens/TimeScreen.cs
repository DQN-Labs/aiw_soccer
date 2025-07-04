using System;
using UnityEngine;

public class TimeScreen : Screen
{
    [SerializeField] private FootballEnvController envController;

    private int lastDisplayedTime = -1;

    private void Start()
    {
        if (envController == null)
        {
            Debug.LogError("FootballEnvController is not assigned in TimeScreen.");
        }
        UpdateTimeDisplay();
    }

    private void Update()
    {
        if (envController == null) return;

        int currentTime = envController.CurrentTime;
        if (currentTime != lastDisplayedTime)
        {
            UpdateTimeDisplay();
        }
    }

    private void UpdateTimeDisplay()
    {
        if (envController == null) return;
        lastDisplayedTime = envController.CurrentTime;
        SetCanvasText(lastDisplayedTime.ToString());
    }
}
