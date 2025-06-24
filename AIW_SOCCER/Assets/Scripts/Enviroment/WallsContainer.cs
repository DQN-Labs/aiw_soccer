using UnityEngine;
using UnityEngine.Rendering;

public class WallsContainer : MonoBehaviour
{
    [Header("Wall References")]
    [SerializeField] private GameObject backWall;
    [SerializeField] private GameObject frontWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject ceiling;
    [SerializeField] private GameObject floor; // Optional, if you want to include a floor

    [Header("Room Dimensions")]
    [SerializeField] private float width = 5f;
    [SerializeField] private float depth = 5f;
    [SerializeField] private float height = 3f;
    [SerializeField] private float thickness = 0.1f; // Wall thickness
    [SerializeField] private float floorHeight = 0f; // Height of the floor

    [Header("Screens (optional)")]
    [SerializeField] private ScoreScreen backScoreScreen;
    [SerializeField] private ScoreScreen frontScoreScreen;
    [SerializeField] private GameObject sideBigScreen;
    [SerializeField] private float bigScreenHeight = 3f; // Height of the big screen
    [SerializeField] private GameObject sideSmallScreen;
    [SerializeField] private float smallScreenHeight = 1.5f; // Height of the small screen

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
            backWall.transform.localPosition = new Vector3(
                -depth / 2f - thickness / 2f, 
                height / 2f, 
                0);
            backWall.transform.localScale = new Vector3(width, height, thickness);
        }

        // Front Wall (Z+)
        if (frontWall != null)
        {
            frontWall.transform.localPosition = new Vector3(
                depth / 2f + thickness / 2f, 
                height / 2f, 
                0);
            frontWall.transform.localScale = new Vector3(thickness, height, width);
        }

        // Right Wall (X+)
        if (rightWall != null)
        {
            rightWall.transform.localPosition = new Vector3(
                0, 
                height / 2f, 
                width / 2f + thickness / 2f);
            rightWall.transform.localScale = new Vector3(depth, height, thickness);
        }

        // Left Wall (X-)
        if (leftWall != null)
        {
            leftWall.transform.localPosition = new Vector3(
                0, 
                height / 2f, 
                -width / 2f - thickness / 2f);
            leftWall.transform.localScale = new Vector3(depth, height, thickness);
        }

        // Ceiling (Y+)
        if (ceiling != null)
        {
            ceiling.transform.localPosition = new Vector3(
                0, 
                height + thickness / 2f, 
                0);
            ceiling.transform.localScale = new Vector3(width, thickness, depth);
        }

        if (floor != null)
        {
            // Floor (Y-)
            floor.transform.localPosition = new Vector3(
                0, 
                floorHeight / 2f, 
                0);
            floor.transform.localScale = new Vector3(depth, floorHeight, width);
        }

        // Update score screens if they exist
        if (backScoreScreen != null)
        {
            backScoreScreen.transform.localPosition = new Vector3(
                0.1f + -depth / 2f, 
                2.8f + height / 2f, 
                0);
        }

        if (frontScoreScreen != null)
        {
            frontScoreScreen.transform.localPosition = new Vector3(
                -0.1f + depth / 2f, 
                2.8f + height / 2f, 
                0);
        }

        if (sideBigScreen != null)
        {
            sideBigScreen.transform.localPosition = new Vector3(
                0, 
                bigScreenHeight + height / 2f, 
                -0.1f + width / 2f);
            sideBigScreen.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        if (sideSmallScreen != null)
        {
            sideSmallScreen.transform.localPosition = new Vector3(
                0, 
                smallScreenHeight + height / 2f, 
                -0.1f + width / 2f);
            sideSmallScreen.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
    }
}