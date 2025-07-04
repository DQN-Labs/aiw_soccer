using UnityEngine;

[CreateAssetMenu(fileName = "EnviromentSO", menuName = "Scriptable Objects/EnviromentSO")]
public class EnviromentSO : ScriptableObject
{
    public GameObject envPrefab; // Prefab for the environment
    public string envName; // Name of the environment
}
