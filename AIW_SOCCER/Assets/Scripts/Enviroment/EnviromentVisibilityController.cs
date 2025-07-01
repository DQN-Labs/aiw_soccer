using UnityEngine;

public class EnvironmentVisibilityController : MonoBehaviour
{
    // A flag to know if visuals are currently on or off.
    private bool areVisualsEnabled = true;

    // Cache references to Renderer or other components in Awake/Start.
    private Renderer[] renderers;
    private Light[] lights;
    private Camera[] cameras;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        lights = GetComponentsInChildren<Light>(true);
        cameras = GetComponentsInChildren<Camera>(true);
    }

    // Call this method from your spawner to control visibility.
    public void SetVisibility(bool isVisible)
    {
        // Don't do unnecessary work if the state isn't changing.
        if (isVisible == areVisualsEnabled)
        {
            return;
        }

        // Use SetActive instead of enabling/disabling components if possible for better performance.
        foreach (var r in renderers)
        {
            r.enabled = isVisible;
        }

        foreach (var l in lights)
        {
            l.enabled = isVisible;
        }

        // Also disable any cameras if they are part of the prefab.
        foreach (var c in cameras)
        {
            c.enabled = isVisible;
        }

        // Update our state flag.
        areVisualsEnabled = isVisible;
    }
}