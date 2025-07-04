using Unity.MLAgents.Policies;
using UnityEngine;

public class Enviroment_Old : MonoBehaviour
{
    [SerializeField] private int ID;

    private FootballAgent[] footballAgents; // Array to hold references to FootballAgent components in this environment

    private void Awake()
    {
        InitializeAgents(); // Initialize the football agents when the environment is created
    }

    public void SetEnviromentID(int id)
    {
        ID = id; // Set the unique ID for this environment instance
    }

    public int GetEnviromentID()
    {
        return ID; // Return the unique ID of this environment instance
    }

    public void DestroyEnviroment()
    {
        Destroy(gameObject); // Destroy this environment instance
    }

    public void InitializeAgents()
    {
        footballAgents = GetComponentsInChildren<FootballAgent>(); // Get all FootballAgent components in the children of this environment
    }

    public FootballAgent[] GetFootballAgentsFromTeamID(int teamID) {
        // Return only the agents whose teamID matches the given teamID
        return System.Array.FindAll(footballAgents, agent => agent.GetTeamID() == teamID);
    }

    public static int GetCurrentEnviromentID(GameObject gameObject)
    {
        GameObject parentObject = gameObject.transform.parent?.gameObject; // Get the parent object of the given game object
        while (parentObject != null && parentObject.GetComponent<Enviroment_Old>() == null)
        {
            parentObject = parentObject.transform.parent?.gameObject; // Traverse up the hierarchy to find the Enviroment component
        }

        //Debug.Log($"Current Environment ID: {parentObject?.GetComponent<Enviroment>()?.GetID()}"); // Log the found environment ID for debugging
        return gameObject.GetComponentInParent<Enviroment_Old>().GetEnviromentID(); // Get the environment ID from the parent Enviroment component
    }
}