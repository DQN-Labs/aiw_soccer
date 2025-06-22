using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AlbertAgent : Agent
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded;

    public Transform ball;
    public Transform goal;

    public enum ControlScheme
    {
        WASD_Arrows,
        IJKL_Shift
    }

    [Tooltip("Choose which keys this agent should respond to when using Heuristic mode.")]
    public ControlScheme controlScheme = ControlScheme.WASD_Arrows;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        ball.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f));
        ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);
        sensor.AddObservation(ball.localPosition);
        sensor.AddObservation(goal.localPosition);
        sensor.AddObservation((ball.localPosition - transform.localPosition).normalized);
        sensor.AddObservation((goal.localPosition - transform.localPosition).normalized);
        sensor.AddObservation(isGrounded ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];   // 0 = none, 1 = forward, 2 = backward
        int rotateAction = actions.DiscreteActions[1]; // 0 = none, 1 = left, 2 = right
        int jumpAction = actions.DiscreteActions[2];   // 0 = no jump, 1 = jump

        if (moveAction == 1)
            rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
        else if (moveAction == 2)
            rb.MovePosition(rb.position - transform.forward * moveSpeed * Time.fixedDeltaTime);

        if (rotateAction == 1)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, -rotationSpeed * Time.fixedDeltaTime, 0f));
        else if (rotateAction == 2)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime, 0f));

        if (jumpAction == 1 && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        float distanceToBall = Vector3.Distance(transform.localPosition, ball.localPosition);
        if (distanceToBall < 1.5f)
            AddReward(0.05f);

        if (Vector3.Distance(ball.localPosition, goal.localPosition) < 1.5f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        if (transform.localPosition.y < -1f)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
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
    }
}
