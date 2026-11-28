using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    private Camera _activeCamera;

    private void OnEnable()
    {
        RefreshActiveCamera();
        AlignToCamera();
    }

    private void LateUpdate()
    {
        if (_activeCamera == null || !_activeCamera.isActiveAndEnabled)
            RefreshActiveCamera();

        AlignToCamera();
    }

    private void AlignToCamera()
    {
        if (_activeCamera == null) return;

        transform.rotation = _activeCamera.transform.rotation;
    }

    private void RefreshActiveCamera()
    {
        Camera main = Camera.main;
        if (main != null && main.isActiveAndEnabled)
        {
            _activeCamera = main;
            return;
        }

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.isActiveAndEnabled)
            {
                _activeCamera = cam;
                return;
            }
        }

        _activeCamera = null;
    }
}