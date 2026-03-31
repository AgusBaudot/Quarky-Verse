using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ShodriPlayerController : MonoBehaviour
{
    [Header("Movement (Floating)")]
    [SerializeField] private float _topSpeed = 8f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private float _deceleration = 25f;
    [SerializeField] private float _turnSmoothTime = 0.1f;

    [Header("References")]
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _controller;
    private Vector3 _currentMoveVelocity;
    private float _turnSmoothVelocity;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Calculate direction relative to the camera
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            
            // Smoothly rotate character model
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate actual movement direction
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // Accelerate
            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, moveDirection * _topSpeed, _acceleration * Time.deltaTime);
        }
        else
        {
            // Decelerate
            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, Vector3.zero, _deceleration * Time.deltaTime);
        }

        // Apply horizontal movement only (no gravity)
        _controller.Move(_currentMoveVelocity * Time.deltaTime);
    }
}