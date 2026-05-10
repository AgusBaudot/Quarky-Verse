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
    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private float _turnSmoothVelocity;
    private bool _isGrounded;

    private bool _isDashing;
    private float _dashStartTime;
    private float _lastDashTime = -100f;
    private Vector3 _dashDirection;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleDash();

        if (!_isDashing)
        {
            HandleGravityAndJump();
            HandleMovement();
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + _dashCooldown && !_isDashing)
        {
            StartDash();
        }

        if (_isDashing)
        {
            if (Time.time >= _dashStartTime + _dashDuration)
            {
                EndDash();
            }
            else
            {
                _controller.Move(_dashDirection * (_dashSpeed * Time.deltaTime));
            }
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashStartTime = Time.time;

        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            _dashDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            _dashDirection = transform.forward;
        }

        _velocity.y = 0f;
    }

    private void EndDash()
    {
        _isDashing = false;
        _lastDashTime = Time.time;

        _currentMoveVelocity = _dashDirection * _topSpeed;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, moveDirection * _topSpeed, _acceleration * Time.deltaTime);
        }
        else
        {
            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, Vector3.zero, _deceleration * Time.deltaTime);
        }

        _controller.Move(_currentMoveVelocity * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        float currentGravity = _gravity;
        if (_velocity.y < 0) 
        {
            currentGravity *= _fallMultiplier; 
        }

        _velocity.y += currentGravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}