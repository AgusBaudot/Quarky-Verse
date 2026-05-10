using UnityEngine;

public class OdysseyThirdPersonCamera : MonoBehaviour
{
    [Header("Target & Tracking")]
    [SerializeField, Tooltip("The player object to follow")] 
    private Transform _target;
    [SerializeField, Tooltip("Offset from the _target's pivot (e.g., aim at chest level)")] 
    private Vector3 _targetOffset = new Vector3(0, 1.5f, 0);

    [Header("Camera Distance & Collision")]
    [SerializeField, Tooltip("Normal distance from the player")] 
    private float _defaultDistance = 5f;
    [SerializeField, Tooltip("Minimum distance when hitting a wall")] 
    private float _minDistance = 1f;
    [SerializeField, Tooltip("Layers the camera should collide with")] 
    private LayerMask _collisionMask;

    [Header("Mouse Sensitivity & Limits")]
    [SerializeField] private float _mouseSensitivityX = 3f;
    [SerializeField] private float _mouseSensitivityY = 2f;
    [SerializeField, Tooltip("Lowest angle you can look down (- degrees)")] 
    private float _minPitch = -20f;
    [SerializeField, Tooltip("Highest angle you can look up (+ degrees)")] 
    private float _maxPitch = 70f;
    [SerializeField, Tooltip("How smoothly the camera follows the player")] 
    private float _smoothSpeed = 10f;

    private float _currentYaw;
    private float _currentPitch;
    private Vector3 _currentVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!_target) return;
        
        _currentYaw += Input.GetAxis("Mouse X") * _mouseSensitivityX;
        _currentPitch -= Input.GetAxis("Mouse Y") * _mouseSensitivityY;
        
        _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);

        Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
        Vector3 focusPoint = _target.position + _targetOffset;
        Vector3 desiredCameraPos = focusPoint - (rotation * Vector3.forward * _defaultDistance);

        float currentDistance = _defaultDistance;
        if (Physics.SphereCast(focusPoint, 0.2f, (desiredCameraPos - focusPoint).normalized, out RaycastHit hit, _defaultDistance, _collisionMask))
        {
            currentDistance = Mathf.Clamp(hit.distance - 0.1f, _minDistance, _defaultDistance);
        }

        Vector3 finalPosition = focusPoint - rotation * Vector3.forward * currentDistance;
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _currentVelocity, 1f / _smoothSpeed);
        transform.LookAt(focusPoint);
    }

    public void SetTarget(Transform newTarget)
        => _target = newTarget;
}