using UnityEngine;

public class PrefabLoader : MonoBehaviour
{
    public PrefabLibrary library;
    public Transform holder;

    void Start()
    {
        SelectionManager.OnItemSelected += LoadById;
        LoadById(PlayerPrefs.GetString("SelectedItem", "DefaultSkin"));
    }

    void OnDestroy()
    {
        SelectionManager.OnItemSelected -= LoadById;
    }

    public void LoadById(string id)
    {
        foreach (Transform child in holder)
            Destroy(child.gameObject);

        GameObject prefab = library.GetPrefabById(id);
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, holder);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning($"No prefab loaded for ID: {id}");
        }
    }
}