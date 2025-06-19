using UnityEngine;

public class Kai : MonoBehaviour
{
    public void ResetPosition()
    {
        transform.position = new Vector3(-5, 5, 0);
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }
}
