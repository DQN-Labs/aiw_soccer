using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Assertions.Must;


public class FootballAgent : Unity.MLAgents.Agent, ICubeEntity
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpForce;

    [Tooltip("Choose which keys this agent should respond to when using Heuristic mode.")]
    [SerializeField] private ControlScheme controlScheme = ControlScheme.WASD_Arrows;


    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Net netTarget;
    [SerializeField] private Transform spawnPosition; // This is not necessary

    private GoalRegister goalRegisterTarget;

    private Vector3 initialPosition;
    private Rigidbody rigidBody;
    private bool isGrounded;
    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        initialPosition = transform.localPosition;
        goalRegisterTarget = netTarget.GetComponentInChildren<GoalRegister>();

        Net.OnGoalScored += HandleGoalScored;
    }

    public override void OnEpisodeBegin()
    {
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        // Add randomness to the spawn position within a range
        //transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));

        if (spawnPosition != null) transform.localPosition = spawnPosition.localPosition; // Use the spawn position if available
        else transform.localPosition = initialPosition; // Use the initial position

        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        ball.transform.localPosition = new Vector3(0, 5, 0);
        ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position); // 3
                                                   // sensor.AddObservation(rigidBody.linearVelocity);
                                                   // sensor.AddObservation(ball.position);
                                                   // sensor.AddObservation(goal.localPosition);
                                                   // sensor.AddObservation((ball.localPosition - transform.localPosition).normalized);
                                                   // sensor.AddObservation((goal.localPosition - transform.localPosition).normalized);
                                                   // sensor.AddObservation(isGrounded ? 1f : 0f);

        sensor.AddObservation(rigidBody.linearVelocity);                      // 3
        sensor.AddObservation((ball.transform.localPosition - transform.localPosition).normalized); // 3
        sensor.AddObservation((goalRegisterTarget.transform.localPosition - transform.localPosition).normalized); // 3
        sensor.AddObservation(isGrounded ? 1f : 0f);                          // 1

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];   // 0 = none, 1 = forward, 2 = backward
        int rotateAction = actions.DiscreteActions[1]; // 0 = none, 1 = left, 2 = right
        int jumpAction = actions.DiscreteActions[2];   // 0 = no jump, 1 = jump

        Vector3 moveDir = Vector3.zero;

        if (moveAction == 1)
            moveDir = transform.forward;
        else if (moveAction == 2)
            moveDir = -transform.forward;


        Vector3 desiredVelocity = moveDir * moveSpeed;
        rigidBody.linearVelocity = new Vector3(desiredVelocity.x, rigidBody.linearVelocity.y, desiredVelocity.z);


        if (rotateAction == 1)
            rigidBody.angularVelocity = new Vector3(0f, -rotationSpeed * Mathf.Deg2Rad, 0f); // convert to rad/s
        else if (rotateAction == 2)
            rigidBody.angularVelocity = new Vector3(0f, rotationSpeed * Mathf.Deg2Rad, 0f);
        else
            rigidBody.angularVelocity = Vector3.zero;


        if (jumpAction == 1 && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // float distanceToBall = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        // if (distanceToBall < 1.5f)
        //     AddReward(0.005f);

        // Changed this; Now this only happens if the event is fired
        //if (Vector3.Distance(ball.transform.localPosition, goalRegisterTarget.transform.localPosition) < 1.5f)
        //{
        //    AddReward(1000.0f);
        //    Debug.Log("Goal Scored!");
        //    EndEpisode();
        //}


        // if (transform.localPosition.y < -1f)
        // {
        //     AddReward(-1.0f);
        //     EndEpisode();
        // }

        AddReward(-0.0005f);
    }

    private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.netID == netTarget.GetNetID()) AddReward(10.0f); // Add reward if the goal was scored in the agent's target net
        else AddReward(-10.0f); // Add penalty if the goal was scored in the agent's net

        Debug.Log($"Goal scored: Reward for {gameObject.name}: {GetCumulativeReward()}");
        EndEpisode();
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        switch (controlScheme)
        {
            case ControlScheme.WASD_Arrows:
                discreteActions[0] = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 :
                                     Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 2 : 0;

                discreteActions[1] = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 :
                                     Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 2 : 0;

                discreteActions[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
                break;

            case ControlScheme.IJKL_Shift:
                discreteActions[0] = Input.GetKey(KeyCode.I) ? 1 :
                                     Input.GetKey(KeyCode.K) ? 2 : 0;

                discreteActions[1] = Input.GetKey(KeyCode.J) ? 1 :
                                     Input.GetKey(KeyCode.L) ? 2 : 0;

                discreteActions[2] = Input.GetKey(KeyCode.RightShift) ? 1 : 0;
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
        // if (collision.gameObject.CompareTag("Ball"))
        // {
        // AddReward(0.5f); // touching the ball is a good thing
        // }
        // if (collision.gameObject.CompareTag("Barrier"))
        // {
        //     AddReward(-0.1f);
        // }
    }

    public void ResetPosition(Vector3 initialPosition)
    {
        transform.localPosition = spawnPosition.position;
    }

    public float[] GetMovementAttributes()
    {
        return new float[] { moveSpeed, rotationSpeed, jumpForce };
    }

    public ControlScheme GetControlScheme()
    {
        return controlScheme;
    }

    public void SetControlScheme(ControlScheme newScheme)
    {
        controlScheme = newScheme;
    }

    public void SetMovementAttributes(float? moveSpeed = null, float? rotationSpeed = null, float? jumpForce = null)
    {
        if (moveSpeed.HasValue) this.moveSpeed = moveSpeed.Value;
        if (rotationSpeed.HasValue) this.rotationSpeed = rotationSpeed.Value;
        if (jumpForce.HasValue) this.jumpForce = jumpForce.Value;
    }

    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}