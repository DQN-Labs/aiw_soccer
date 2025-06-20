using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 resetPosition = Vector3.zero;
    [SerializeField] private Player player;
    [SerializeField] private Kai kai;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Goal1"))
        {
            Debug.Log("Scored in Goal 1");
            transform.position = resetPosition;
            ResetAllPositions();
        }
        if (collision.collider.CompareTag("Goal2"))
        {
            Debug.Log("Scored in Goal 2");
            ResetAllPositions();
        }
    }

    private void ResetAllPositions()
    {
        transform.position = resetPosition;

        // Stop any motion
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (player != null)
        {
            player.ResetPosition();
        }

        if (kai != null)
        {
            kai.ResetPosition();
        }
    }
}