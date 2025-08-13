using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;

    private int currentCameraIndex = 0;

    void Start()
    {
        for (int i = 1; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetCamera((currentCameraIndex + 1) % cameras.Length);
        }
    }

    public void SetCamera(int index)
    {
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogError("Invalid camera index: " + index);
            return;
        }

        cameras[currentCameraIndex].gameObject.SetActive(false);
        // Activate new camera
        currentCameraIndex = index;
        cameras[currentCameraIndex].gameObject.SetActive(true);

        // If the new camera is a CubeCamera, set its position to the current TargetObject position
        CubeCamera cubeCam = cameras[currentCameraIndex].GetComponent<CubeCamera>();
        if (cubeCam != null && cubeCam.GetTargetObject() != null)
        {
            cubeCam.SetCameraPositionStart(cubeCam.GetTargetObject().position);
        }
    }
}
