using UnityEngine;

public class OdysseyFirstPersonCamera : MonoBehaviour
{
    [Header("Target & Tracking")]
    [SerializeField, Tooltip("The player object (or head bone) to follow")] 
    private Transform _target;
    [SerializeField, Tooltip("Offset from the _target's pivot (e.g., eye level)")] 
    private Vector3 _targetOffset = new Vector3(0, 1.6f, 0);

    [Header("Mouse Sensitivity & Limits")]
    [SerializeField] private float _mouseSensitivityX = 3f;
    [SerializeField] private float _mouseSensitivityY = 2f;
    [SerializeField, Tooltip("Lowest angle you can look down (- degrees)")] 
    private float _minPitch = -80f; 
    [SerializeField, Tooltip("Highest angle you can look up (+ degrees)")] 
    private float _maxPitch = 80f; 

    private float _currentYaw;
    private float _currentPitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        // When the CollapseGlassesController enables this camera, 
        // sync the internal yaw/pitch to match its current rotation so it doesn't snap.
        _currentYaw = transform.eulerAngles.y;
        _currentPitch = transform.eulerAngles.x;

        // Correct pitch wrap-around (so 350 degrees becomes -10, matching the clamp limits)
        if (_currentPitch > 180f) _currentPitch -= 360f;
    }

    void LateUpdate()
    {
        if (!_target) return;

        // 1. Get Mouse Input
        _currentYaw += Input.GetAxis("Mouse X") * _mouseSensitivityX;
        _currentPitch -= Input.GetAxis("Mouse Y") * _mouseSensitivityY;
        
        // Clamp the pitch so we never flip over
        _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);

        // 2. Apply Position (locked inside the head) & Rotation
        transform.position = _target.position + _targetOffset;
        transform.rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
    }

    public void SetTarget(Transform newTarget)
        => _target = newTarget;
}