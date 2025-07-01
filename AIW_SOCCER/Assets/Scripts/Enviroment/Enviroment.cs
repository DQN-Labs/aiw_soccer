using UnityEngine;

public class Enviroment : MonoBehaviour
{
    [SerializeField] private int ID;


    public int GetID()
    {
        return ID; // Return the unique ID of this environment instance
    }

    public static int GetCurrentEnviromentID(GameObject gameObject)
    {
        GameObject parentObject = gameObject.transform.parent?.gameObject; // Get the parent object of the given game object
        while (parentObject != null && parentObject.GetComponent<Enviroment>() == null)
        {
            parentObject = parentObject.transform.parent?.gameObject; // Traverse up the hierarchy to find the Enviroment component
        }

        //Debug.Log($"Current Environment ID: {parentObject?.GetComponent<Enviroment>()?.GetID()}"); // Log the found environment ID for debugging
        return gameObject.GetComponentInParent<Enviroment>().GetID(); // Get the environment ID from the parent Enviroment component
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
}
