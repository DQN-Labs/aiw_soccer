using Unity.VisualScripting;
using UnityEngine;

public class EnviromentSpawner : MonoBehaviour
{
    [SerializeField] private EnviromentSO enviromentSO;

    [SerializeField] private int rows = 4; // E.g. 4x8 for 32 environments
    [SerializeField] private int columns = 8;
    [SerializeField] private float distanceBetweenInstances = 30f;

    [SerializeField] private bool visualizeOneEnvironment = true;

    private void Awake()
    {
        var children = new GameObject[transform.childCount];
        for (int i = 0; i < children.Length; i++)
            children[i] = transform.GetChild(i).gameObject;

        foreach (var child in children)
            child.GetComponent<Enviroment_Old>()?.DestroyEnviroment();

        if (enviromentSO == null || enviromentSO.envPrefab == null)
        {
            Debug.LogError("EnvironmentSO or its prefab is not assigned in the Spawner!");
            return;
        }

        // Cache envPrefab to avoid repeated access
        var prefab = enviromentSO.envPrefab;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 offset = new Vector3(col * distanceBetweenInstances, 0, row * distanceBetweenInstances);
                GameObject env = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform);

                // Cache GetComponent calls
                var visibilityController = env.GetComponent<EnvironmentVisibilityController>();
                if (visibilityController != null)
                {
                    bool shouldBeVisible = (row == 0 && col == 0);
                    visibilityController.SetVisibility(visualizeOneEnvironment && shouldBeVisible);
                }
                else
                {
                    Debug.LogWarning($"The environment prefab '{env.name}' is missing the EnvironmentVisibilityController script!", env);
                }

                var envComponent = env.GetComponent<FootballEnvController>();
                if (envComponent != null)
                    envComponent.SetEnviromentID(row * columns + col);

                // env.transform.parent = transform; // Already set by Instantiate with parent argument
            }
        }
    }
}