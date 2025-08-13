using UnityEngine;
using System.Collections;

public class Skinloader : MonoBehaviour
{
    public GameObject[] skinPrefabs;  
    public Transform modelHolder;    

    void Start()
    {
        StartCoroutine(LoadSkin());
    }

    IEnumerator LoadSkin()
    {
        yield return null;
        int index = PlayerPrefs.GetInt("SelectedSkin", 0);
        if (index < 0 || index >= skinPrefabs.Length) index = 0;
        foreach (Transform child in modelHolder)
            Destroy(child.gameObject);
        if (skinPrefabs.Length > 0)
        {
            GameObject skin = Instantiate(skinPrefabs[index], modelHolder);
            skin.transform.localPosition = Vector3.zero;
            skin.transform.localRotation = Quaternion.identity;
        }
    }
}
