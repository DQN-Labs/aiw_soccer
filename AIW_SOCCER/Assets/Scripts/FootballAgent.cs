using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public static class Rewards
{
    public const float GoalScoredReward = 10.0f;
    public const float GoalConcededPenalty = -10.0f;
    public const float TimeLimitPenalty = -5f;
    public const float MovePenalty = -0.0001f;
    public const float ApproachBallReward = 0.0002f;
    public const float BallMovingTowardGoal = 0.05f;
    public const float BallTouchReward = 0.05f;
    public const float kickPenalty = -0.01f;
    public const float dashPenalty = -0.01f;
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
    private float lastBallToGoalDistance;
    private float lastBallTouchTime = -10f;
    private float ballTouchCooldown = 0.2f;

    private StatsRecorder statsRecorder;

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        cubeEntity = GetComponent<CubeEntity>();
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
        lastBallToGoalDistance = Vector3.Distance(ball.transform.localPosition, goalRegisterTarget.transform.localPosition);
        lastBallTouchTime = -10f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rigidBody.linearVelocity);
        sensor.AddObservation(isGrounded ? 1f : 0f);
        sensor.AddObservation(cubeEntity.CanDash());
        sensor.AddObservation(cubeEntity.CanKick());

        sensor.AddObservation(GetAgentBallDotProduct());
        sensor.AddObservation(ball.GetComponent<Rigidbody>().linearVelocity);
        sensor.AddObservation(GetAgentGoalDotProduct());
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
        }

        if (kickAction == 1 && cubeEntity.CanKick())
        {
            cubeEntity.BKick();
            AddReward(Rewards.kickPenalty);
            statsRecorder.Add($"Football/{name}/KicksAttempted", 1f, StatAggregationMethod.Sum);
        }

        if (dashAction == 1 && cubeEntity.CanDash())
        {
            cubeEntity.StartDash();
            AddReward(Rewards.dashPenalty);
        }

        // 1. Reward for approaching ball
        float currentBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        if (currentBallDistance < lastBallDistance)
        {
            AddReward(Rewards.ApproachBallReward);
        }
        lastBallDistance = currentBallDistance;

        // 2. Reward if ball moves toward goal
        Vector3 ballVelocity = ball.GetComponent<Rigidbody>().linearVelocity;
        if (ballVelocity.magnitude > 0.1f)
        {
            Vector3 directionToGoal = (goalRegisterTarget.transform.localPosition - ball.transform.localPosition).normalized;
            float dot = Vector3.Dot(ballVelocity.normalized, directionToGoal);

            if (dot > 0.5f)
            {
                AddReward(Rewards.BallMovingTowardGoal * dot * (ballVelocity.magnitude / 4.5f));
                statsRecorder.Add($"Football/{name}/ProductiveShotPower", ballVelocity.magnitude, StatAggregationMethod.Histogram);
            }
        }

        // 3. NEW: Ball closer to goal reward (distance reducing version)
        float currentBallToGoalDistance = Vector3.Distance(ball.transform.localPosition, goalRegisterTarget.transform.localPosition);
        if (currentBallToGoalDistance < lastBallToGoalDistance)
        {
            AddReward(0.005f);
        }
        lastBallToGoalDistance = currentBallToGoalDistance;

        

        AddReward(Rewards.MovePenalty);
    }

    private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return;

        if (e.netID == netTarget.GetNetID())
        {
            AddReward(Rewards.GoalScoredReward);
            statsRecorder.Add($"Football/{name}/GoalsScored", 1, StatAggregationMethod.Sum);
        }
        else
        {
            AddReward(Rewards.GoalConcededPenalty);
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

    public GameObject GetGameObject() => gameObject;
}