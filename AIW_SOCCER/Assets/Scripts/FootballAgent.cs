using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using Unity.VisualScripting.FullSerializer;

// Rewards Constants
public struct Rewards
{
    // --- Terminal Rewards (The Main Goal) ---
    public const float GoalScoredReward = 1.0f;
    public const float GoalConcededPenalty = -1.0f;

    // --- Penalties ---
    public const float TimeLimitPenalty = -0.5f; // Penalty for not scoring in time.
    public const float MovePenalty = -0.0001f;   // Small penalty for every step to encourage speed.

    // --- Shaping Rewards ---
    // Reward for moving towards the ball.
    public const float ApproachBallReward = 0.0001f;

    // Reward for the ball going TOWARDS the opponent's goal.
    public const float BallMovingTowardGoal = 0.002f;

    // A small one-time reward for touching the ball.
    public const float BallTouchReward = 0.0005f;

    public const float kickPenalty = -0.005f; // Penalty for kicking the ball, to encourage strategic kicking
    public const float dashPenalty = -0.002f; // Penalty for dashing, to encourage strategic dashing
    public const float jumpPenalty = -0.002f; // Penalty for jumping, to encourage strategic jumping
}

public class FootballAgent : Agent, ICubeEntity
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpForce;
    private ControlScheme controlScheme;

    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Net netTarget;
    [SerializeField] private Transform spawnPosition;
    private GoalRegister goalRegisterTarget;

    private Rigidbody ballRigidBody; // Reference to the ball's rigidbody
    private CubeEntity cubeEntity;
    private Rigidbody rigidBody;
    private bool isGrounded;

    private int iterationCount = 0;

    public static event EventHandler<OnEpisodeEndEventArgs> OnEpisodeEnd;

    public class OnEpisodeEndEventArgs : EventArgs
    {
        public int envID = 0;
        public int iterationCount = 0;
    }

    private float lastBallDistance;

    // --- Add this flag to track ball collision ---
    private bool isCollidingWithBall = false;
    private float stepsControllingBall = 0f;

    private StatsRecorder statsRecorder;


    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        cubeEntity = GetComponent<CubeEntity>();
        ballRigidBody = ball.GetComponent<Rigidbody>();
        goalRegisterTarget = netTarget.GetComponentInChildren<GoalRegister>();

        Net.OnGoalScored += HandleGoalScored;
        TimeScreen.OnTimeLimitReached += HandleTimeLimitReached;
        controlScheme = cubeEntity.GetControlScheme();

        lastBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        isGrounded = true;

        statsRecorder = Academy.Instance.StatsRecorder;
    }

    public override void OnEpisodeBegin()
    {
        ball.ResetPosition();
        ball.transform.localPosition += new Vector3(UnityEngine.Random.Range(-1f, 1f), 0.1f, UnityEngine.Random.Range(-4f, 4f));

        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.linearVelocity = Vector3.zero;

        if (spawnPosition != null)
            transform.localPosition = spawnPosition.localPosition;
        else
            transform.localPosition = cubeEntity.GetInitialPosition();

        transform.localPosition += new Vector3(UnityEngine.Random.Range(-1f, 1f), 0.1f, UnityEngine.Random.Range(-3f, 3f));
        transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

        ResetPosition(transform.localPosition);

        lastBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
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
        sensor.AddObservation(isCollidingWithBall); // 1

        // Goal register position
        sensor.AddObservation(GetAgentGoalDotProduct()); // 1
    }

    public float GetAgentBallDotProduct()
    {
        Vector3 directionToBall = (ball.transform.localPosition - transform.localPosition).normalized;
        return Vector3.Dot(transform.forward, directionToBall);
    }

    private float GetAgentGoalDotProduct()
    {
        Vector3 directionToGoal = (goalRegisterTarget.transform.localPosition - transform.localPosition).normalized;
        return Vector3.Dot(transform.forward, directionToGoal);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        int rotateAction = actions.DiscreteActions[1];
        int jumpAction = actions.DiscreteActions[2];
        int kickAction = actions.DiscreteActions[3];
        int dashAction = actions.DiscreteActions[4];

        Vector3 moveDir = Vector3.zero;
        if (moveAction == 1) moveDir = transform.forward;
        else if (moveAction == 2) moveDir = -transform.forward;

        Vector3 desiredVelocity = moveDir * moveSpeed;
        rigidBody.linearVelocity = new Vector3(desiredVelocity.x, rigidBody.linearVelocity.y, desiredVelocity.z);

        if (rotateAction == 1)
            rigidBody.angularVelocity = new Vector3(0f, -rotationSpeed * Mathf.Deg2Rad, 0f);
        else if (rotateAction == 2)
            rigidBody.angularVelocity = new Vector3(0f, rotationSpeed * Mathf.Deg2Rad, 0f);
        else
            rigidBody.angularVelocity = Vector3.zero;

        if (jumpAction == 1 && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            AddReward(Rewards.jumpPenalty);
            statsRecorder.Add($"Rewards/{name}/jumpPenalty", Rewards.jumpPenalty, StatAggregationMethod.Sum);
        }

        if (kickAction == 1 && cubeEntity.CanKick())
        {
            cubeEntity.BKick();
            AddReward(Rewards.kickPenalty);
            statsRecorder.Add($"Football/{name}/KicksAttempted", 1f, StatAggregationMethod.Sum);
            statsRecorder.Add($"Rewards/{name}/kickPenalty", Rewards.kickPenalty, StatAggregationMethod.Sum);
        }

        if (dashAction == 1 && cubeEntity.CanDash())
        {
            cubeEntity.StartDash();
            AddReward(Rewards.dashPenalty);
            statsRecorder.Add($"Rewards/{name}/DashPenalty", Rewards.dashPenalty, StatAggregationMethod.Sum);
        }

        // 1. Reward for approaching ball
        float currentBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        if (currentBallDistance < lastBallDistance)
        {
            AddReward(Rewards.ApproachBallReward);
            statsRecorder.Add($"Rewards/{name}/ApproachBallReward", Rewards.ApproachBallReward, StatAggregationMethod.Sum);
        }
        lastBallDistance = currentBallDistance;


        // 2. BallMovingTowardGoalReward
        Vector3 ballVelocity = ballRigidBody.linearVelocity;
        if (ballVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 directionToGoal = (goalRegisterTarget.transform.localPosition - ball.transform.localPosition).normalized;
            float dot = Vector3.Dot(ballVelocity.normalized, directionToGoal);

            if (dot > 0.5f)
            {

                float velocity = ballVelocity.magnitude;
                if (velocity > 5f) velocity = 5f; // Sometimes this equals the ball kick power (30), but we dont want that

                AddReward(Rewards.BallMovingTowardGoal * dot * (velocity / 5f));
                statsRecorder.Add($"Rewards/{name}/BallMovingTowardGoal", Rewards.BallMovingTowardGoal * dot * (velocity / 5f), StatAggregationMethod.Sum);
                statsRecorder.Add($"Football/{name}/ProductiveShotPower", velocity, StatAggregationMethod.Histogram);
            }
        }

        // --- Add BallTouchReward for every step in collision with the ball ---
        if (isCollidingWithBall)
        {
            AddReward(Rewards.BallTouchReward);
            statsRecorder.Add($"Rewards/{name}/BallTouchReward", Rewards.BallTouchReward, StatAggregationMethod.Sum);
            stepsControllingBall += 1f;
        }

        AddReward(Rewards.MovePenalty);
        statsRecorder.Add($"Rewards/{name}/MovePenalty", Rewards.MovePenalty, StatAggregationMethod.Sum);
        statsRecorder.Add($"Football/steps_per_episode", 1f, StatAggregationMethod.Sum);
    }

    private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return;

        if (e.netID == netTarget.GetNetID())
        {
            AddReward(Rewards.GoalScoredReward); // Add reward if the goal was scored in the agent's target net
            statsRecorder.Add($"Football/{name}/GoalsScored", 1, StatAggregationMethod.Sum); // Increment the goals scored counter in StatsRecorder
            statsRecorder.Add($"Rewards/{name}/GoalScoredReward", Rewards.GoalScoredReward, StatAggregationMethod.Sum);
        }
        else
        {
            AddReward(Rewards.GoalConcededPenalty); // Add penalty if the goal was scored in the agent's net
            statsRecorder.Add($"Rewards/{name}/GoalConcededPenalty", Rewards.GoalConcededPenalty, StatAggregationMethod.Sum);
        }

        iterationCount++;
        OnEpisodeEnd?.Invoke(this, new OnEpisodeEndEventArgs
        {
            envID = cubeEntity.GetEnvID(),
            iterationCount = iterationCount
        });

        EndEpisode();
    }

    private void HandleTimeLimitReached(object sender, TimeScreen.OnTimeLimitReachedEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return;
        AddReward(Rewards.TimeLimitPenalty);
        iterationCount++;
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
                discreteActions[3] = Input.GetKey(KeyCode.F) ? 1 : 0;
                discreteActions[4] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0;
                break;

            case ControlScheme.IJKL_Shift:
                discreteActions[0] = Input.GetKey(KeyCode.I) ? 1 : Input.GetKey(KeyCode.K) ? 2 : 0;
                discreteActions[1] = Input.GetKey(KeyCode.J) ? 1 : Input.GetKey(KeyCode.L) ? 2 : 0;
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
        // --- Set flag if colliding with ball ---
        if (collision.gameObject.CompareTag("Ball"))
        {
            isCollidingWithBall = true;
            stepsControllingBall = 0f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // --- Unset flag when no longer colliding with ball ---
        if (collision.gameObject.CompareTag("Ball"))
        {
            isCollidingWithBall = false;
            statsRecorder.Add($"Football/{name}/StepsTouchingBall", stepsControllingBall, StatAggregationMethod.Sum);
        }
    }

    public void ResetPosition(Vector3 initialPosition)
    {
        cubeEntity.ResetPosition(initialPosition);
    }

    public float[] GetMovementAttributes() => new float[] { moveSpeed, rotationSpeed, jumpForce };

    public ControlScheme GetControlScheme() => controlScheme;

    public void SetControlScheme(ControlScheme newScheme) => controlScheme = newScheme;

    public void SetMovementAttributes(float? moveSpeed = null, float? rotationSpeed = null, float? jumpForce = null)
    {
        if (moveSpeed.HasValue) this.moveSpeed = moveSpeed.Value;
        if (rotationSpeed.HasValue) this.rotationSpeed = rotationSpeed.Value;
        if (jumpForce.HasValue) this.jumpForce = jumpForce.Value;
    }

    public Vector3 GetInitialPosition() => cubeEntity.GetInitialPosition();

    public GameObject GetGameObject()
    {
        return gameObject;
    }


}