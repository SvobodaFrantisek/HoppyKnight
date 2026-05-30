using UnityEngine;

public class WorldSpaceBarBillboard : MonoBehaviour
{
    public Camera targetCamera;
    public bool lockYRotation = false;
    public float searchInterval = 0.5f;

    private Camera cachedCamera;
    private float nextCameraSearchTime;

    void LateUpdate()
    {
        Camera cameraToFace = GetCameraToFace();

        if (cameraToFace == null)
        {
            return;
        }

        if (lockYRotation)
        {
            FaceCameraOnlyOnY(cameraToFace);
            return;
        }

        transform.rotation = Quaternion.LookRotation(cameraToFace.transform.forward, cameraToFace.transform.up);
    }

    Camera GetCameraToFace()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }

        if (cachedCamera != null && cachedCamera.isActiveAndEnabled)
        {
            return cachedCamera;
        }

        if (Time.time < nextCameraSearchTime)
        {
            return null;
        }

        nextCameraSearchTime = Time.time + searchInterval;
        cachedCamera = Camera.main;

        if (cachedCamera == null)
        {
            cachedCamera = FindFirstObjectByType<Camera>();
        }

        return cachedCamera;
    }

    void FaceCameraOnlyOnY(Camera cameraToFace)
    {
        Vector3 directionToCamera = transform.position - cameraToFace.transform.position;
        directionToCamera.y = 0f;

        if (directionToCamera.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
    }
}
