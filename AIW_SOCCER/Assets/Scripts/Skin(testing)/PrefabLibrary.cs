using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabLibrary", menuName = "Game/Prefab Library")]
public class PrefabLibrary : ScriptableObject
{
    [System.Serializable]
    public class IDPrefabPair
    {
        public string id;
        public GameObject prefab;
    }

    public List<IDPrefabPair> entries = new List<IDPrefabPair>();

    public GameObject GetPrefabById(string id)
    {
        foreach (var entry in entries)
        {
            if (entry.id == id)
                return entry.prefab;
        }
        Debug.LogWarning($"No prefab found with ID: {id}");
        return null;
    }
}