using UnityEngine;
public enum ControlScheme
{
    WASD_Arrows,
    IJKL_Shift
}

public class CubeEntity : MonoBehaviour, ICubeEntity
{
    [Header("Dash properties")]
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Kick properties")]
    [SerializeField] private float kickRange = 3f;
    [SerializeField] private float kickAngleDegrees = 65f;
    [SerializeField] private float kickMagnitude = 30f;

    [Tooltip("Choose which keys this agent should respond to when using Heuristic mode.")]
    [SerializeField] private ControlScheme controlScheme = ControlScheme.WASD_Arrows;

    private Vector3 initialPosition;
    private Rigidbody rigidBody;

    // Dash-related variables
    private bool isDashing;
    private bool canDash = true;
    private float dashTimeLeft;
    private float cooldownTimer;
    private Vector3 dashDirection;

    private void Awake()
    {
        initialPosition = transform.position;
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rigidBody.linearVelocity = dashDirection * dashForce;
            dashTimeLeft -= Time.fixedDeltaTime;
            if (dashTimeLeft <= 0f)
            {
                StopDash();
            }
        }
    }

    void Update()
    {
        HandleDashInput();
        HandleCooldown();
        HandleKickInput();
    }

    // Métodos de la interfaz ICubeEntity

    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void ResetPosition(Vector3 initialPosition)
    {
        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }
    public Rigidbody GetRigidbody()
    {
        return rigidBody;
    }

    public ControlScheme GetControlScheme()
    {
        return controlScheme;
    }

    public void SetControlScheme(ControlScheme newControlScheme)
    {
        controlScheme = newControlScheme;
    }

    // Kick functionality

    private void HandleKickInput()
    {
        KeyCode kickKey = controlScheme == ControlScheme.WASD_Arrows ? KeyCode.F : KeyCode.H;
        if (Input.GetKeyDown(kickKey))
        {
            BKick();
        }
    }

    private void BKick()
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
                rb.linearVelocity = kickVelocity;
            }
        }
    }

    // Dash functionality
    private void HandleDashInput()
    {
        KeyCode dashKey = controlScheme == ControlScheme.WASD_Arrows ? KeyCode.LeftShift : KeyCode.B;
        if (Input.GetKeyDown(dashKey) && canDash) StartDash();
    }

    private void HandleCooldown()
    {
        if (!canDash)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f) canDash = true;
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        cooldownTimer = dashCooldown;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        dashDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
    }

    private void StopDash()
    {
        isDashing = false;
        rigidBody.linearVelocity = Vector3.zero;
    }
}
