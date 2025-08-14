using UnityEngine;
using Unity.MLAgents;             
using Unity.MLAgents.Policies;    

public class ONNXFrameOffset : MonoBehaviour
{
    public int frameOffset = 0;       // which frame to start
    public int decisionPeriod = 6;    // decision period
    private BehaviorParameters behaviorParams;

    void Start()
    {
        behaviorParams = GetComponent<BehaviorParameters>();
    }

    void Update()
    {
        int frame = Time.frameCount;

        // check if agent infers now
        if ((frame + frameOffset) % decisionPeriod == 0)
        {
            RequestDecision(); // asks for decision
        }
    }

    void RequestDecision()
    {
        var agent = GetComponent<Agent>();
        if(agent != null) agent.RequestDecision();
    }
}
