using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Ball : MonoBehaviour
{
    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    public void ResetPosition()
    {
        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, -90, 0);
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }
}