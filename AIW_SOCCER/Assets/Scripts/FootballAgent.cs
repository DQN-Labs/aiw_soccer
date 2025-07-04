#region Using Directives
using NUnit;
using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
#endregion

#region Rewards Constants

public struct Rewards
{
    public const float GoalScoredReward = 1.0f;
    public const float GoalConcededPenalty = -1.0f;

    public const float TimeLimitPenalty = -0.5f;
    public const float MovePenalty = -0.0001f;

    public const float ApproachBallReward = 0.0001f;
    public const float BallMovingTowardGoal = 0.002f;
    public const float BallTouchReward = 0.0005f;

    public const float kickPenalty = -0.005f;
    public const float dashPenalty = -0.002f;
    public const float jumpPenalty = -0.002f;
// using System;

// public static class Rewards
// {
//     public const float GoalScoredReward = 10.0f;
//     public const float GoalConcededPenalty = -10.0f;
//     public const float TimeLimitPenalty = -5f;
//     public const float MovePenalty = -0.0001f;
//     public const float ApproachBallReward = 0.0002f;
//     public const float BallMovingTowardGoal = 0.05f;
//     public const float BallTouchReward = 0.05f;
//     public const float kickPenalty = -0.01f;
//     public const float dashPenalty = -0.01f;
// }
#endregion

public class FootballAgent : Agent
{
    #region Serialized Fields
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpForce;
    private ControlScheme controlScheme;

    [Header("References")]
    [SerializeField] private Ball ball;
    [SerializeField] private Net netTarget;
    #endregion

    #region Private Fields
    private GoalRegister goalRegisterTarget;
    private CubeEntity cubeEntity;
    private Rigidbody rigidBody;
    private Rigidbody ballRigidBody;
    private bool isGrounded;
    private bool isCollidingWithBall = false;
    private float stepsControllingBall = 0f;
    private float lastBallDistance = -10f;

    // private int iterationCount = 0;

    // public static event EventHandler<OnEpisodeEndEventArgs> OnEpisodeEnd;

    // public class OnEpisodeEndEventArgs : EventArgs
    // {
    //     public int envID = 0;
    //     public int iterationCount = 0;
    // }

    // private float lastBallDistance;
    // private float lastBallToGoalDistance;
    // private float lastBallTouchTime = -10f;
    // private float ballTouchCooldown = 0.2f;

    private StatsRecorder statsRecorder;
    #endregion

    public Team team;

    #region Unity Lifecycle
    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        cubeEntity = GetComponent<CubeEntity>();
        goalRegisterTarget = netTarget.GetComponentInChildren<GoalRegister>();

        controlScheme = cubeEntity.GetControlScheme();

        isGrounded = true;
        lastBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);

        statsRecorder = Academy.Instance.StatsRecorder;
    }

    private void Start()
    {
        int iskai = GetComponent<BehaviorParameters>().TeamId == 0 ? 1 : 0;

        cubeEntity.SetEnvID(FootballEnvController.GetCurrentEnviromentID(gameObject)); // Set the environment ID in the CubeEntity

        SetTeamID((cubeEntity.GetEnvID() * 2 - iskai) + 1);
    }

    public override void OnEpisodeBegin()
    {
        lastBallDistance = Vector3.Distance(transform.localPosition, ball.transform.localPosition);
        lastBallToGoalDistance = Vector3.Distance(ball.transform.localPosition, goalRegisterTarget.transform.localPosition);
        lastBallTouchTime = -10f;
    }

    #endregion

    #region ML-Agents Overrides
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rigidBody.linearVelocity);
        sensor.AddObservation(isGrounded ? 1f : 0f);
        sensor.AddObservation(cubeEntity.CanDash());
        sensor.AddObservation(cubeEntity.CanKick());

        sensor.AddObservation(GetAgentBallDotProduct());
        sensor.AddObservation(ballRigidBody.linearVelocity);
        sensor.AddObservation(isCollidingWithBall);

        sensor.AddObservation(GetAgentGoalDotProduct());
        // sensor.AddObservation(ball.GetComponent<Rigidbody>().linearVelocity);
        // sensor.AddObservation(GetAgentGoalDotProduct());
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
        if (currentBallDistance <= lastBallDistance)
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

    // private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    // {
    //     if (e.envID != cubeEntity.GetEnvID()) return;

    //     if (e.netID == netTarget.GetNetID())
    //     {
    //         AddReward(Rewards.GoalScoredReward);
    //         statsRecorder.Add($"Football/{name}/GoalsScored", 1, StatAggregationMethod.Sum);
    //     }
    //     else
    //     {
    //         AddReward(Rewards.GoalConcededPenalty);
    //     }

    //     iterationCount++;
    //     OnEpisodeEnd?.Invoke(this, new OnEpisodeEndEventArgs
    //     {
    //         envID = cubeEntity.GetEnvID(),
    //         iterationCount = iterationCount
    //     });

    //     EndEpisode();
    // }

    // private void HandleTimeLimitReached(object sender, TimeScreen.OnTimeLimitReachedEventArgs e)
    // {
    //     if (e.envID != cubeEntity.GetEnvID()) return;
    //     AddReward(Rewards.TimeLimitPenalty);
    //     iterationCount++;
    //     EndEpisode();
    // }

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
    #endregion

    #region Helper Methods
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
    #endregion

    #region Event listeners
    public void HandleGoalScored(Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != cubeEntity.GetEnvID()) return;

        if (e.TeamScored == team)
        {
            AddReward(Rewards.GoalScoredReward);
            statsRecorder.Add($"Football/{name}/GoalsScored", 1, StatAggregationMethod.Sum);
            statsRecorder.Add($"Rewards/{name}/GoalScoredReward", Rewards.GoalScoredReward, StatAggregationMethod.Sum);
        }
        else
        {
            AddReward(Rewards.GoalConcededPenalty);
            statsRecorder.Add($"Rewards/{name}/GoalConcededPenalty", Rewards.GoalConcededPenalty, StatAggregationMethod.Sum);
        }

        EndEpisode();
    }
    #endregion

    #region Collision Handlers
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
        if (collision.gameObject.CompareTag("Ball"))
        {
            isCollidingWithBall = true;
            stepsControllingBall = 0f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
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
    #endregion

    #region Utility Methods

    public Vector3 GetInitialPosition() => cubeEntity.GetInitialPosition();

    public void SetTeamID(int teamID) => GetComponent<BehaviorParameters>().TeamId = teamID;

    public int GetTeamID() => GetComponent<BehaviorParameters>().TeamId;

    public void SetTeam(Team team) => this.team = team;

    public Team GetTeam() => team;

    public GameObject GetGameObject() => gameObject;
    #endregion
}