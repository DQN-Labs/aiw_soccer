using UnityEngine;

public interface ICubeEntity 
{
    public void ResetPosition(Vector3 initialPosition);

    public float[] GetMovementAttributes();

    public ControlScheme GetControlScheme();

    public void SetControlScheme(ControlScheme newScheme);

    public void SetMovementAttributes(float? moveSpeed = null, float? rotationSpeed = null, float? jumpForce = null);

    public Vector3 GetInitialPosition();

    public GameObject GetGameObject();
}
