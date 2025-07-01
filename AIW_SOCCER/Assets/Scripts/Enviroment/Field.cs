using UnityEngine;

public class Field : MonoBehaviour
{

    [SerializeField] private Ball ball;
    [SerializeField] private GameObject[] cubes;

    private int envID; // Environment ID, if needed

    private void Start()
    {
        envID = Enviroment.GetCurrentEnviromentID(gameObject); // Get the environment ID from the parent Enviroment component
        Net.OnGoalScored += HandleGoalScored;
    }

    private void HandleGoalScored(object sender, Net.OnGoalScoredEventArgs e)
    {
        // Reset positions
        if (e.envID != envID) return; // Check if the event is for the current environment
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

        foreach (GameObject cube in cubes)
        {
            if (cube != null)
            {
                var cubeEntity = cube.GetComponent<ICubeEntity>();
                if (cubeEntity != null)
                {
                    // Reinicia la posición usando la interfaz
                    cubeEntity.ResetPosition(cubeEntity.GetInitialPosition());

                    // Detiene cualquier movimiento
                    Rigidbody rb = cube.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }
            }
        }
    }

    public GameObject[] GetCubes()
    {
        return cubes;
    }

    public void SetCubes(GameObject[] newCubes)
    {
        cubes = newCubes;
    }
}