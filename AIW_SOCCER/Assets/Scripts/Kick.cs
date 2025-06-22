using UnityEngine;

public class Kick : MonoBehaviour
{
  public float kickRange = 3f;
  public float kickAngleDegrees =45f;
  public float kickMagnitude = 10f;
  
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
        BKick();
    }
  }
  void BKick()
  {
    Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, kickRange);

    foreach (Collider hit in hits)
    {
        if (hit.CompareTag("Ball"))
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) return;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 kickDirection = Quaternion.AngleAxis(kickAngleDegrees, transform.right) * forward;
            Vector3 kickVelocity = kickDirection * kickMagnitude;
            rb.velocity = kickVelocity;
        }
    }
  }
}
