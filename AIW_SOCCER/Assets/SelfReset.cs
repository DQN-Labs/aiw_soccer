using UnityEngine;

public class BallReset : MonoBehaviour
{
    public Vector3 resetPosition = Vector3.zero;
    public Transform transform;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Goal1"))
        {
            Debug.Log("Scored in Goal 1");
            transform.position = resetPosition;

            // Stop any motion
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        if (collision.collider.CompareTag("Goal2"))
        {
            Debug.Log("Scored in Goal 2");
            transform.position = resetPosition;

            // Stop any motion
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}