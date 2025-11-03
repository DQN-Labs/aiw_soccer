
using UnityEngine;

public enum CubeType
{
    Kai,
    Albert
}

[CreateAssetMenu(fileName = "CubeSO", menuName = "Scriptable Objects/CubeSO")]
public class CubeSO : ScriptableObject
{
    public GameObject cubePrefab; // Prefab for the cube
    public Unity.InferenceEngine.ModelAsset brain;
    public CubeType cubeType;
}
