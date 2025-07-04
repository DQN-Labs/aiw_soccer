using System;
using UnityEngine;

public class CounterScreen : Screen
{
    [SerializeField] private FootballEnvController envController;

    private void Start()
    {
        if (envController == null)
        {
            envController = GetComponentInParent<FootballEnvController>();
        }

        if (envController != null)
        {
            envController.OnIterationsCountChanged += UpdateDisplay;
            UpdateDisplay(envController.IterationsCount);
        }
    }

    private void OnDestroy()
    {
        if (envController != null)
        {
            envController.OnIterationsCountChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(int count)
    {
        SetCanvasText(count.ToString());
    }
}
