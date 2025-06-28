using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Assertions.Must;
using System;


// Rewards Constants
public static class Rewards
{
    // --- Terminal Rewards (The Main Goal) ---
    public const float GoalScoredReward = 10.0f;
    public const float GoalConcededPenalty = -10.0f;

    // --- Penalties ---
    public const float TimeLimitPenalty = -5f; // Penalty for not scoring in time.
    public const float MovePenalty = -0.0001f;   // Small penalty for every step to encourage speed.

    // --- Shaping Rewards ---
    // Reward for moving towards the ball.
    public const float ApproachBallReward = 0.0002f;

    // Reward for the ball going TOWARDS the opponent's goal.
    public const float BallMovingTowardGoal = 0.05f;

    // A small one-time reward for touching the ball.
    public const float BallTouchReward = 0.05f;

    public const float kickPenalty = -0.01f; // Penalty for kicking the ball, to encourage strategic kicking
    public const float dashPenalty = -0.01f; // Penalty for dashing, to encourage strategic dashing
}

public class FootballAgent : Unity.MLAgents.Agent, ICubeEntity
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpForce;
    private ControlScheme controlScheme;

    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Net netTarget;
    [SerializeField] private Transform spawnPosition; // This is not necessary
    private GoalRegister goalRegisterTarget;

    private CubeEntity cubeEntity;
    private Rigidbody rigidBody;
    private bool isGrounded;

    private int iterationCount = 0; // Counter for the number of iterations

    public static event EventHandler<OnEpisodeEndEventArgs> OnEpisodeEnd; // Event to notify when the episode ends

    public class OnEpisodeEndEventArgs : EventArgs
    {
        public int envID = 0; // This can be used to identify the environment if needed
        public int iterationCount = 0;
    }

    // --- Variables using for the rewards ---
    private float lastBallDistance;
    private float lastBallTouchTime = -10f;
    private float ballTouchCooldown = 0.2f; // seconds before the agent gets another reward for touching the ball

    // --- StatsRecorder ---
    private StatsRecorder statsRecorder; // Reference to the StatsRecorder for logging

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        cubeEntity = GetComponent<CubeEntity>();
        goalRegisterTarget = netTarget.GetComponentInChildren<GoalRegister>();

        Net.OnGoalScored += HandleGoalScored; // Subscribe to goal scored event
        TimeScreen.OnTimeLimitReached += HandleTimeLimitReached; // Subscribe to time limit reached event
        controlScheme = GetComponent<CubeEntity>().GetControlScheme(); // Default control scheme

        lastBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);

        isGrounded = true; // Start grounded

        statsRecorder = Academy.Instance.StatsRecorder; // Get the StatsRecorder instance
    }

    public override void OnEpisodeBegin()
    {
        ball.ResetPosition(); // Reset the ball position back to the starting position
        ball.transform.localPosition += new Vector3(UnityEngine.Random.Range(-1f, 1f), 0.1f, UnityEngine.Random.Range(-4f, 4f));

        rigidBody.angularVelocity = Vector3.zero; // Reset angular velocity
        rigidBody.linearVelocity = Vector3.zero; // Reset linear velocity to stop movement

        if (spawnPosition != null)
        {
            transform.localPosition = spawnPosition.localPosition; // Reset the agent's position to the spawn position
        }
        else
        {
            transform.localPosition = cubeEntity.GetInitialPosition(); // Reset to initial position if no spawn position is set
        }

        // Añade aleatoriedad después de resetear
        transform.localPosition += new Vector3(UnityEngine.Random.Range(-1f, 1f), 0.1f, UnityEngine.Random.Range(-3f, 3f));
        transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

        ResetPosition(transform.localPosition);

        lastBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        lastBallTouchTime = -10f;
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
            AddReward(Rewards.kickPenalty);
            statsRecorder.Add($"Football/{name}/KicksAttempted", 1f, StatAggregationMethod.Sum);
        }

        // Dashing
        if (dashAction == 1 && cubeEntity.CanDash())
        {
            cubeEntity.StartDash();
            AddReward(Rewards.dashPenalty);
        }

        // --- ApproachBallReward ---
        float currentBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        if (currentBallDistance < lastBallDistance)
        {
            AddReward(Rewards.ApproachBallReward);
        }
        lastBallDistance = currentBallDistance;

        // 2. BallMovingTowardGoalReward
        Vector3 ballVelocity = ball.GetComponent<Rigidbody>().linearVelocity;
        if (ballVelocity.magnitude > 0.1f)
        {
            Vector3 directionToGoal = (goalRegisterTarget.transform.localPosition - ball.transform.localPosition).normalized;
            float dot = Vector3.Dot(ballVelocity.normalized, directionToGoal);

            // We only reward positive dot products (ball moving towards goal)
            if (dot > 0.5f)
            {
                // The reward is proportional to how fast the ball is moving AND how accurate the shot is.
                AddReward(Rewards.BallMovingTowardGoal * dot * (ballVelocity.magnitude / 4.5f)); // Scale by velocity, avg is 4.5 rn but this can change
                statsRecorder.Add($"Football/{name}/ProductiveShotPower", ballVelocity.magnitude, StatAggregationMethod.Histogram);
            }
        }

        AddReward(Rewards.MovePenalty);
    }

    private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return; // Check if the event is for the current environment

        if (e.netID == netTarget.GetNetID())
        {
            AddReward(Rewards.GoalScoredReward); // Add reward if the goal was scored in the agent's target net
            statsRecorder.Add($"Football/{name}/GoalsScored", 1, StatAggregationMethod.Sum); // Increment the goals scored counter in StatsRecorder
        }
        else 
            AddReward(Rewards.GoalConcededPenalty); // Add penalty if the goal was scored in the agent's net

        //Debug.Log($"Goal scored: Reward for {gameObject.name}: {GetCumulativeReward()}");
        iterationCount++;
        OnEpisodeEnd?.Invoke(this, new OnEpisodeEndEventArgs {
            envID = cubeEntity.GetEnvID(), // This can be used to identify the environment if needed
            iterationCount = iterationCount
        }); // Notify subscribers that the episode has ended

        EndEpisode();
    }

    private void HandleTimeLimitReached(object sender, TimeScreen.OnTimeLimitReachedEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return;
      
        //Debug.Log($"Time limit reached for {gameObject.name}. Ending episode.");
        AddReward(Rewards.TimeLimitPenalty); // Penalty for time limit reached
        iterationCount++;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }

        // --- BallTouchReward ---
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (Time.time - lastBallTouchTime > ballTouchCooldown)
            {
                AddReward(Rewards.BallTouchReward);
                lastBallTouchTime = Time.time;
                statsRecorder.Add($"Football/{name}/BallTouches", 1f, StatAggregationMethod.Sum);
            }
        }
    }

    public void ResetPosition(Vector3 initialPosition)
    {
        cubeEntity.ResetPosition(initialPosition);
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