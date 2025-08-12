using UnityEngine;

public class Skinloader : MonoBehaviour
{
    public GameObject[] skinPrefabs;  
    public Transform modelHolder;    

    void Start()
    {
        int index = PlayerPrefs.GetInt("SelectedSkin", 0);

        if (index < 0 || index >= skinPrefabs.Length)
        {
            Debug.LogWarning($"Invalid skin index {index}, resetting to 0");
            index = 0;
        }

        foreach (Transform child in modelHolder)
            Destroy(child.gameObject);

        if (skinPrefabs.Length > 0)
        {
            GameObject skin = Instantiate(skinPrefabs[index], modelHolder);
            skin.transform.localPosition = Vector3.zero;
            skin.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("No skinPrefabs assigned in inspector");
        }
    }
}
