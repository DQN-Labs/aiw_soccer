using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;


public class Ball : MonoBehaviour
{
    private Vector3 initialPosition;
    public FootballAgent lastAgentToTouch;
    private float lastTouchTime = -1.5f;
    private float passCooldown = 0.3f; // seconds

    private void OnCollisionEnter(Collision collision)
    {
        var agent = collision.gameObject.GetComponent<FootballAgent>();
        if (agent != null)
        {
            // check for pass
            if (lastAgentToTouch != null && lastAgentToTouch != agent && lastAgentToTouch.GetTeam() == agent.GetTeam())
            {
                if (Time.time - lastTouchTime > passCooldown)
                {
                    lastAgentToTouch.AddReward(0.05f); // reward passer
                    agent.AddReward(0.05f);         // reward receiver
                    Debug.Log("Pass awarded.");     
                }
            }

            // last touch
            lastAgentToTouch = agent;
            lastTouchTime = Time.time;
        }
    }

    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    public void ResetPosition()
    {
        transform.localPosition = initialPosition;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}