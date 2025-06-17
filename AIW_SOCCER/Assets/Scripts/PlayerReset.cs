using UnityEngine;

public class PlayerReset : MonoBehaviour
{
    

    public void ResetPosition()
    {
        transform.position = new Vector3(5, 6, 0);
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }
}
