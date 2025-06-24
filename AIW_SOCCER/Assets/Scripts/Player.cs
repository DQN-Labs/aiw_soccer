using UnityEngine;

public class Player : MonoBehaviour, ICubeEntity
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed = 15;
    [SerializeField] private float rotationSpeed = 200;
    [SerializeField] private float jumpForce = 5;

    private Vector3 initialPosition;
    private Rigidbody rigidBody;
    private bool isGrounded;

    private ControlScheme controlScheme; // Default control scheme

    private void Awake()
    {
        // Store the initial position of the cube
        initialPosition = transform.position;
        rigidBody = GetComponent<Rigidbody>();

        controlScheme = GetComponent<CubeEntity>().GetControlScheme(); // Get the control scheme from CubeEntity
    }

    // Movement

    void FixedUpdate()
    {
        float moveInput = 0f;
        float rotationInput = 0f;

        // Adapt input based on control scheme
        if (controlScheme == ControlScheme.WASD_Arrows)
        {
            moveInput = Input.GetAxis("Vertical");
            rotationInput = Input.GetAxis("Horizontal");
        }
        else if (controlScheme == ControlScheme.IJKL_Shift)
        {
            // I = forward, K = backward, J = left, L = right
            if (Input.GetKey(KeyCode.I)) moveInput += 1f;
            if (Input.GetKey(KeyCode.K)) moveInput -= 1f;
            if (Input.GetKey(KeyCode.L)) rotationInput += 1f;
            if (Input.GetKey(KeyCode.J)) rotationInput -= 1f;
        }

        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rigidBody.MovePosition(rigidBody.position + move);

        Quaternion turn = Quaternion.Euler(0f, rotationInput * rotationSpeed * Time.fixedDeltaTime, 0f);
        rigidBody.MoveRotation(rigidBody.rotation * turn);
    }

    void Update()
    {
        bool jumpPressed = false;

        // Adapt jump input based on control scheme
        if (controlScheme == ControlScheme.WASD_Arrows)
        {
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
        }
        else if (controlScheme == ControlScheme.IJKL_Shift)
        {
            jumpPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        }

        if (jumpPressed && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if touching the ground to enable jumping again
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    // Methods

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

    public ControlScheme GetControlScheme()
    {
        return controlScheme;
    }

    public void SetControlScheme(ControlScheme newScheme)
    {
        controlScheme = newScheme;
    }

    public float[] GetMovementAttributes()
    {
        return new float[] { moveSpeed, rotationSpeed, jumpForce };
    }

    // Allow partial updates by using nullable parameters
    public void SetMovementAttributes(float? moveSpeed = null, float? rotationSpeed = null, float? jumpForce = null)
    {
        if (moveSpeed.HasValue) this.moveSpeed = moveSpeed.Value;
        if (rotationSpeed.HasValue) this.rotationSpeed = rotationSpeed.Value;
        if (jumpForce.HasValue) this.jumpForce = jumpForce.Value;
    }

    public Rigidbody GetRigidbody()
    {
        return rigidBody;
    }
    public void ResetPosition()
    {
        transform.position = new Vector3(5, 6, 0);
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }
}
