using UnityEngine;

public class WallsContainer : MonoBehaviour
{
    [Header("Wall References")]
    [SerializeField] private GameObject backWall;
    [SerializeField] private GameObject frontWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject ceiling;

    [Header("Room Dimensions")]
    [SerializeField] private float width = 5f;
    [SerializeField] private float depth = 5f;
    [SerializeField] private float height = 3f;
    [SerializeField] private float thickness = 0.1f; // Wall thickness

    private void OnValidate()
    {
        UpdateDimensions();
    }

    public void SetDimensions(float newWidth, float newDepth, float newHeight)
    {
        width = newWidth;
        depth = newDepth;
        height = newHeight;
        UpdateDimensions();
    }

    public (float width, float depth, float height) GetDimensions()
    {
        return (width, depth, height);
    }

    private void UpdateDimensions()
    {
        // Back Wall (Z-)
        if (backWall != null)
        {
            backWall.transform.localPosition = new Vector3(-depth / 2f, height / 2f, 0);
            backWall.transform.localScale = new Vector3(width, height, thickness);
        }

        // Front Wall (Z+)
        if (frontWall != null)
        {
            frontWall.transform.localPosition = new Vector3(depth / 2f, height / 2f, 0);
            frontWall.transform.localScale = new Vector3(width, height, thickness);
        }

        // Right Wall (X+)
        if (rightWall != null)
        {
            rightWall.transform.localPosition = new Vector3(0, height / 2f, width / 2f);
            rightWall.transform.localScale = new Vector3(depth, height, thickness);
        }

        // Left Wall (X-)
        if (leftWall != null)
        {
            leftWall.transform.localPosition = new Vector3(0, height / 2f, -width / 2f);
            leftWall.transform.localScale = new Vector3(depth, height, thickness);
        }

        // Ceiling (Y+)
        if (ceiling != null)
        {
            ceiling.transform.localPosition = new Vector3(0, height, 0);
            ceiling.transform.localScale = new Vector3(width, thickness, depth);
        }
    }
}