using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCamera : MonoBehaviour
{
    [SerializeField] private Transform targetObject; // This should be a different type, not GameObject. It should work for both Player and Kai, maybe an Interface they both inherit from?.
    [SerializeField] private Vector3 offset;
    [SerializeField] private Quaternion rotationOffset = Quaternion.identity;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float initialFollowSpeed = 2f;

    void Start()
    {
        SetCameraPosition(targetObject.position);
    }

    void LateUpdate()
    {
        // Follow the target object with a specified offset
        if (targetObject != null)
        {
            Quaternion objectRotation = targetObject.rotation * rotationOffset;
            Vector3 desiredPosition = targetObject.position + objectRotation * offset;

            SetCameraPosition(desiredPosition);
            SetCameraRotation(objectRotation);
        }
    }

    private void SetCameraRotation(Quaternion objectRotation)
    {
       transform.rotation = objectRotation;
    }

    public void SetCameraPositionStart(Vector3 desiredPosition)
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, initialFollowSpeed * Time.deltaTime);
    }

    private void SetCameraPosition(Vector3 desiredPosition)
    {
        transform.position = desiredPosition;
    }

    private void SetCameraPosition(Vector3 desiredPosition, float followSpeed)
    {
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