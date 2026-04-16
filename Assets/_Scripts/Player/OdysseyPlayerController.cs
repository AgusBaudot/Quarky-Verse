using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class OdysseyPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _topSpeed = 8f;
    [SerializeField, Tooltip("How fast the player reaches top speed")] 
    private float _acceleration = 20f;
    [SerializeField, Tooltip("How fast the player stops when letting go of keys")] 
    private float _deceleration = 25f;
    [SerializeField, Tooltip("How quickly the model rotates to face the moving direction")] 
    private float _turnSmoothTime = 0.1f;

    [Header("Jump & Gravity")]
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private float _gravity = -15f;
    [SerializeField, Tooltip("Multiplier applied to _gravity when falling to make jumps feel heavier")] 
    private float _fallMultiplier = 2f;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 24f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private KeyCode _dashKey = KeyCode.LeftShift;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.3f;
    [SerializeField] private LayerMask _groundMask;

    [Header("References")]
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _controller;
    private Vector3 _velocity; // Vertical _velocity (_gravity/jumping)
    private Vector3 _currentMoveVelocity; // Horizontal _velocity (momentum)
    private float _turnSmoothVelocity;
    private bool _isGrounded;

    // Dash State
    private bool _isDashing;
    private float _dashStartTime;
    private float _lastDashTime = -100f;
    private Vector3 _dashDirection;

    [Header("Grab System")]
    [SerializeField] private float _grabDistance = 5f;
    [SerializeField] private float _holdDistance = 2f;
    [SerializeField] private Transform _cameraTransform2;
    private PickableObject _heldObject;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleDash();
        HandleGrab();
        // Suspend normal movement and _gravity while dashing
        if (!_isDashing)
        {
            HandleGravityAndJump();
            HandleMovement();
        }
    }

    private void HandleDash()
    {
        // Check for dash input
        if (Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + _dashCooldown && !_isDashing)
        {
            StartDash();
        }

        // Process active dash
        if (_isDashing)
        {
            if (Time.time >= _dashStartTime + _dashDuration)
            {
                EndDash();
            }
            else
            {
                // Move the _controller in the dash direction
                _controller.Move(_dashDirection * _dashSpeed * Time.deltaTime);
            }
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashStartTime = Time.time;

        // Calculate input direction
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Dash towards camera-relative input direction
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            _dashDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Snap the character's rotation to instantly face the dash direction
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            // If no input is pressed, dash in the direction the character is currently facing
            _dashDirection = transform.forward;
        }

        // Reset vertical _velocity so the dash is perfectly horizontal
        _velocity.y = 0f;
    }

    private void EndDash()
    {
        _isDashing = false;
        _lastDashTime = Time.time;

        // Preserve momentum by setting current _velocity to top speed in the dash direction
        _currentMoveVelocity = _dashDirection * _topSpeed;
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
            // Decelerate (slide to a stop instead of snapping)
            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, Vector3.zero, _deceleration * Time.deltaTime);
        }

        // Apply horizontal movement
        _controller.Move(_currentMoveVelocity * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Keep snapped to ground
        }

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        // Apply Mario-style falling (_gravity gets stronger when falling down)
        float currentGravity = _gravity;
        if (_velocity.y < 0) 
        {
            currentGravity *= _fallMultiplier; 
        }

        _velocity.y += currentGravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime); // Apply vertical movement
    }
    // Pickup objects and release logic would go here.
    private void HandleGrab()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_heldObject == null) TryGrab();
            else Release();
        }
        if (_heldObject != null) HoldObject();
    }
    private void TryGrab()
    {
        Ray ray = new Ray(_cameraTransform2.position, _cameraTransform2.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _grabDistance))
        {
            PickableObject pickable = hit.collider.GetComponent<PickableObject>();
            if (pickable != null)
            {
                _heldObject = pickable;
                _heldObject.OnGrab();
            }
        }
    }
    private void HoldObject()
    {
        Vector3 targetPosition = _cameraTransform2.position + _cameraTransform2.forward * _holdDistance;
        _heldObject.transform.position = Vector3.Lerp(
            _heldObject.transform.position,
            targetPosition,
            Time.deltaTime * 15f
        );
        _heldObject.transform.rotation = Quaternion.Lerp(
            _heldObject.transform.rotation,
            Quaternion.LookRotation(_cameraTransform2.forward),
            Time.deltaTime * 15f
        );
    }
    private void Release()
    {
        _heldObject.OnRelease();
        _heldObject = null;
    }
}