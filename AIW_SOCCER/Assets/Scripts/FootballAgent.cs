using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Assertions.Must;
using System;


// Rewards Constants
public static class Rewards
{
    public const float MovePenalty = -0.0005f; // Penalty for each action taken
    public const float GoalScoredReward = 10.0f; // Reward for scoring a goal
    public const float GoalConcededPenalty = -10.0f; // Penalty for conceding a goal
    public const float TimeLimitPenalty = -1.0f; // Penalty for time limit reached
}

public class FootballAgent : Unity.MLAgents.Agent, ICubeEntity
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpForce;

    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Net netTarget;
    [SerializeField] private Transform spawnPosition; // This is not necessary

    private Vector3 initialPosition;
    private GoalRegister goalRegisterTarget;
    private ControlScheme controlScheme;

    private Rigidbody rigidBody;
    private bool isGrounded;

    private CubeEntity cubeEntity;

    private int iterationCount = 0; // Counter for the number of iterations

    public static event EventHandler<OnEpisodeEndEventArgs> OnEpisodeEnd; // Event to notify when the episode ends

    public class OnEpisodeEndEventArgs : EventArgs
    {
        public int envID = 0; // This can be used to identify the environment if needed
        public int iterationCount = 0;
    }

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        cubeEntity = GetComponent<CubeEntity>();
        initialPosition = cubeEntity.GetInitialPosition(); // Get the initial position from the CubeEntity
        goalRegisterTarget = netTarget.GetComponentInChildren<GoalRegister>();

        Net.OnGoalScored += HandleGoalScored; // Subscribe to goal scored event
        TimeScreen.OnTimeLimitReached += HandleTimeLimitReached; // Subscribe to time limit reached event
        controlScheme = GetComponent<CubeEntity>().GetControlScheme(); // Default control scheme
    }

    public override void OnEpisodeBegin()
    {
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        if (spawnPosition != null) transform.localPosition = spawnPosition.localPosition; // Use the spawn position if available
        else cubeEntity.ResetPosition(cubeEntity.GetInitialPosition()); // Use the initial position

        // Add randomness to the spawn position within a range
        transform.localPosition += new Vector3(UnityEngine.Random.Range(-2f, 2f), 0.0f, UnityEngine.Random.Range(-4f, 4f));
        transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

        ball.ResetPosition(); // Reset the ball position back to the starting position
        ball.transform.localPosition += new Vector3(UnityEngine.Random.Range(-1f, 1f), 0.0f, UnityEngine.Random.Range(-4f, 4f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Self observations
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(rigidBody.linearVelocity); // 3
        sensor.AddObservation(isGrounded ? 1f : 0f); // 1
        sensor.AddObservation(cubeEntity.CanDash()); // 1 
        sensor.AddObservation(cubeEntity.CanKick()); // 1

        // Ball
        sensor.AddObservation(GetAgentBallDotProduct()); // 1
        sensor.AddObservation(ball.GetComponent<Rigidbody>().linearVelocity); // 3

        // Goal register position
        sensor.AddObservation(GetAgentGoalDotProduct()); // 1
    }

    public float GetAgentBallDotProduct()
    {
        // Calculate the dot product between the agent's forward direction and the direction to the ball
        Vector3 directionToBall = (ball.transform.localPosition - transform.localPosition).normalized;
        return Vector3.Dot(transform.forward, directionToBall);
    }

    private float GetAgentGoalDotProduct()
    {
        // Calculate the dot product between the agent's forward direction and the direction to the goal
        Vector3 directionToGoal = (goalRegisterTarget.transform.localPosition - transform.localPosition).normalized;
        return Vector3.Dot(transform.forward, directionToGoal);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Action array: [move, rotate, jump, kick, dash]
        int moveAction = actions.DiscreteActions[0];   // 0 = none, 1 = forward, 2 = backward
        int rotateAction = actions.DiscreteActions[1]; // 0 = none, 1 = left, 2 = right
        int jumpAction = actions.DiscreteActions[2];   // 0 = no jump, 1 = jump
        int kickAction = actions.DiscreteActions[3];   // 0 = none, 1 = kick,
        int dashAction = actions.DiscreteActions[4];   // 0 = none, 1 = dash


        // Movement
        Vector3 moveDir = Vector3.zero;

        if (moveAction == 1)
            moveDir = transform.forward;
        else if (moveAction == 2)
            moveDir = -transform.forward;

        Vector3 desiredVelocity = moveDir * moveSpeed;
        rigidBody.linearVelocity = new Vector3(desiredVelocity.x, rigidBody.linearVelocity.y, desiredVelocity.z);

        // Rotation
        if (rotateAction == 1)
            rigidBody.angularVelocity = new Vector3(0f, -rotationSpeed * Mathf.Deg2Rad, 0f); // convert to rad/s
        else if (rotateAction == 2)
            rigidBody.angularVelocity = new Vector3(0f, rotationSpeed * Mathf.Deg2Rad, 0f);
        else
            rigidBody.angularVelocity = Vector3.zero;


        // Jumping
        if (jumpAction == 1 && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Kicking
        if (kickAction == 1 && cubeEntity.CanKick())
        {
            cubeEntity.BKick();
        }

        // Dashing
        if (dashAction == 1 && cubeEntity.CanDash())
        {
            cubeEntity.StartDash();
        }

        AddReward(Rewards.MovePenalty);
    }

    private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return; // Check if the event is for the current environment

        if (e.netID == netTarget.GetNetID()) AddReward(Rewards.GoalScoredReward); // Add reward if the goal was scored in the agent's target net
        else AddReward(Rewards.GoalConcededPenalty); // Add penalty if the goal was scored in the agent's net

        //Debug.Log($"Goal scored: Reward for {gameObject.name}: {GetCumulativeReward()}");
        iterationCount++;
        OnEpisodeEnd?.Invoke(this, new OnEpisodeEndEventArgs {
            envID = cubeEntity.GetEnvID(), // This can be used to identify the environment if needed
            iterationCount = iterationCount
        }); // Notify subscribers that the episode has ended

        cubeEntity.ResetPosition(cubeEntity.GetInitialPosition());
        EndEpisode();
    }

    private void HandleTimeLimitReached(object sender, TimeScreen.OnTimeLimitReachedEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return;
      
        //Debug.Log($"Time limit reached for {gameObject.name}. Ending episode.");
        AddReward(Rewards.TimeLimitPenalty); // Penalty for time limit reached
        iterationCount++;

        // The episode end event is triggered by the TimeScreen, so we don't need to call it here
        cubeEntity.ResetPosition(cubeEntity.GetInitialPosition());
        EndEpisode();
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Action array: [move, rotate, jump, kick, dash]
        var discreteActions = actionsOut.DiscreteActions;

        switch (controlScheme)
        {
            case ControlScheme.WASD_Arrows:
                discreteActions[0] = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 :
                                     Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 2 : 0;

                discreteActions[1] = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 :
                                     Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 2 : 0;

                discreteActions[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
                discreteActions[3] = Input.GetKey(KeyCode.F) ? 1 : 0;
                discreteActions[4] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0;
                break;

            

            case ControlScheme.IJKL_Shift:
                discreteActions[0] = Input.GetKey(KeyCode.I) ? 1 :
                                     Input.GetKey(KeyCode.K) ? 2 : 0;

                discreteActions[1] = Input.GetKey(KeyCode.J) ? 1 :
                                     Input.GetKey(KeyCode.L) ? 2 : 0;

                discreteActions[2] = Input.GetKey(KeyCode.RightShift) ? 1 : 0;
                discreteActions[3] = Input.GetKey(KeyCode.H) ? 1 : 0;
                discreteActions[4] = Input.GetKey(KeyCode.B) ? 1 : 0;
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
        transform.localPosition = spawnPosition.localPosition;
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
        return cubeEntity.GetInitialPosition();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}