using UnityEngine;

public class GoalRegister : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        // Only process collision if the other object has a Ball component
        if (collision.gameObject.GetComponent<Ball>() == null)
        {
            //Debug.Log($"Collision with {collision.gameObject.name} does not have a Ball component, ignoring.");
            return;
        }

        // Notify the parent Net component
        Net parentNet = GetComponentInParent<Net>();
        if (parentNet != null)
        {
            parentNet.RegisterGoal();
        }
    }
}