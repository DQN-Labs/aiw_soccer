using UnityEngine;

public interface ICubeEntity 
{
    public void ResetPosition(Vector3 initialPosition);


    public Vector3 GetInitialPosition();

    public GameObject GetGameObject();
}
