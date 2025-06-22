using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    // stats of the dash change however you want rest is yapping
  public float dashForce = 20f;
  public float dashDuration = 0.2f;
  public float dashCD = 1.5f;
  
  private bool isDashing = false;
  private bool canDash = true;
  private float dashTimeLeft;
  private float CDTimer;
  private Rigidbody rb;
  private Vector3 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)  && canDash)
        {
          StartDash();
        }

        if (!canDash)
        {
          CDTimer -= Time.deltaTime;
          if (CDTimer <= 0f)
          {
            canDash = true;
          }
        }
    }

    void FixedUpdate()
    {
      if(isDashing)
      {
        rb.linearVelocity = dashDirection * dashForce;

        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0f)
        {
          StopDash();
        }
      }
    }
      // alright les start
      void StartDash()
      {
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        CDTimer = dashCD;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        dashDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        if (dashDirection == Vector3.zero)
        {
          dashDirection = transform.forward;
        }
      }
      void StopDash()
      {
        isDashing = false;
       // rb.velocity = Vector3.zero;
      }

}
