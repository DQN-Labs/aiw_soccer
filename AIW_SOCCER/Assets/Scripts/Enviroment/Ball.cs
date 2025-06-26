using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Ball : MonoBehaviour
{
    private Vector3 initialPosition;

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