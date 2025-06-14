using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform targetObject;
    private Vector3 initialOffset;
    private Vector3 cameraPosition;
    private float followSpeed = 5f;

    void Start()
    {
        initialOffset = transform.position - targetObject.position;
    }

    void LateUpdate()
    {
        if (targetObject != null)
        {

            Vector3 desiredPosition = targetObject.position + initialOffset;
            
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
    }
}