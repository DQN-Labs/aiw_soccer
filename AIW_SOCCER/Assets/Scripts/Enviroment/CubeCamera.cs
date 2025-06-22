using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCamera : MonoBehaviour
{
    [SerializeField] private Transform targetObject; // This should be a different type, not GameObject. It should work for both Player and Kai, maybe an Interface they both inherit from?.
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed = 5f;

    void Start()
    {
        SetCameraPosition(targetObject.position);
    }

    void LateUpdate()
    {
        if (targetObject != null)
        {
            SetCameraPosition(targetObject.position);

        }
    }

    public void SetCameraPosition(Vector3 playerPosition)
    {
        Vector3 desiredPosition = playerPosition + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    public Vector3 GetOffset()
    {
        return offset;
    }

    public void SetOffset(Vector3 offSet)
    {
        offset = offSet;
    }

    public void SetTargetObject(Transform newTarget)
    {
        targetObject = newTarget;
    }

    public Transform GetTargetObject()
    {
        return targetObject;
    }
}