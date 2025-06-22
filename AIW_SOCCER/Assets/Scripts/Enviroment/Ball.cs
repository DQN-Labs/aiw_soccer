using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Ball : MonoBehaviour
{
    [SerializeField] private Vector3 resetPosition;
    public void ResetPosition()
    {
        transform.position = resetPosition;
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }
}