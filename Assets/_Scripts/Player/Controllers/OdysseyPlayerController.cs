using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class OdysseyPlayerController : MonoBehaviour
{
    public event Action OnJump;
    public event Action OnLand;
    
    public Vector2 CurrentHorizontalVelocity => new Vector2(_currentMoveVelocity.x, _currentMoveVelocity.z);
    public bool IsGrounded => _isGrounded;
    public float VerticalVelocity => _velocity.y;
    
    [Header("Movement")]
    [SerializeField] private float _topSpeed = 8f;
    [SerializeField, Tooltip("How fast the player reaches top speed")] 
    private float _acceleration = 20f;
    [SerializeField, Tooltip("How fast the player stops when letting go of keys")] 
    private float _deceleration = 25f;
    [SerializeField, Tooltip("How quickly the model rotates to face the moving direction")] 
    private float _turnSmoothTime = 0.1f;
    [SerializeField, Tooltip("Input threshold before registering movement")]
    private float _movementInputThreshold = 0.1f;

    [Header("Momentum & Grip (Ice)")]
    [SerializeField, Tooltip("1 = Original Ice. < 1 = More Ice (slippery). > 1 = Less Ice (snappier turns).")]
    private float _directionChangeMultiplier = 1.0f;

    [Header("Jump & Gravity (Kinematic)")]
    [SerializeField, Tooltip("Maximum height the player can jump")] 
    private float _maxJumpHeight = 3.0f;
    [SerializeField, Tooltip("Total time in seconds from jump start to landing (full arc)")] 
    private float _maxJumpTime = 0.75f;
    [SerializeField, Tooltip("Multiplier applied to gravity when falling")] 
    private float _fallMultiplier = 2f;
    [SerializeField, Tooltip("Multiplier applied to upward velocity when spacebar is released early (0 = instant stop, 1 = floaty)")]
    [Range(0f, 1f)] private float _earlyJumpCutoff = 0.5f;
    [SerializeField] private float _terminalVelocity = -20f;
    [SerializeField, Tooltip("Gravity applied while grounded to prevent bouncing down slopes")] 
    private float _groundedGravity = -2f;
    [SerializeField, Tooltip("Time the player is locked in place after landing to prevent sliding")]
    private float _landingPauseDuration = 0.15f;

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
    [SerializeField] private Transform _cameraTransform2;

    [Header("Grab & Push System")]
    [SerializeField] private float _grabDistance = 5f;
    [SerializeField] private float _holdDistance = 2f;
    [SerializeField, Tooltip("How fast the object smoothly tracks to the hold position")]
    private float _grabLerpSpeed = 15f;
    [SerializeField, Tooltip("Distance to keep the object away from walls to prevent clipping")]
    private float _holdWallOffset = 0.3f;
    [SerializeField, Tooltip("Dot product threshold (1 = perfectly straight, 0 = sideways) required to push objects")]
    [Range(0f, 1f)] private float _pushAngleThreshold = 0.5f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private float _turnSmoothVelocity;
    
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isJumping;
    private bool _isJumpPressed;

    private float _gravity;
    private float _initialJumpVelocity;
    private float _movementLockEndTime;

    private bool _isDashing;
    private float _dashStartTime;
    private float _lastDashTime = -100f;
    private Vector3 _dashDirection;

    private PickableObject _heldObject;
    private PushableObject _currentPushable;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        InitializeJumpVariables();
    }

    private void OnValidate()
    {
        InitializeJumpVariables();
    }

    private void InitializeJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2f;
        _gravity = (-2f * _maxJumpHeight) / Mathf.Pow(timeToApex, 2f);
        _initialJumpVelocity = (2f * _maxJumpHeight) / timeToApex;
    }

    private void Update()
    {
        HandleDash();
        HandleGrab();
        
        if (!_isDashing)
        {
            HandleGravityAndJump();
            HandleMovement();
        }
        
        if (_currentPushable != null)
        {
            Vector3 horizontalMove = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);
            if (horizontalMove.magnitude < _movementInputThreshold) _currentPushable.StopPush();
        }
    }
    
    private void HandleGravityAndJump()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (!_wasGrounded && _isGrounded && _velocity.y <= 0f)
        {
            _isJumping = false;
            OnLand?.Invoke();
            
            _movementLockEndTime = Time.time + _landingPauseDuration;
            _currentMoveVelocity = Vector3.zero;
        }

        _isJumpPressed = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded && Time.time >= _movementLockEndTime)
        {
            _isJumping = true;
            _velocity.y = _initialJumpVelocity;
            OnJump?.Invoke();
        }

        if (Input.GetKeyUp(KeyCode.Space) && _velocity.y > 0f)
        {
            _velocity.y *= _earlyJumpCutoff; 
        }

        if (_isGrounded && _velocity.y <= 0)
        {
            _velocity.y = _groundedGravity;
        }
        else
        {
            bool isFalling = _velocity.y <= 0.0f || !_isJumpPressed;
            float currentMultiplier = isFalling ? _fallMultiplier : 1.0f;
        
            float previousYVelocity = _velocity.y;
            float newYVelocity = _velocity.y + (_gravity * currentMultiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
        
            _velocity.y = Mathf.Max(nextYVelocity, _terminalVelocity);
        }

        _controller.Move(_velocity * Time.deltaTime); 
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + _dashCooldown && !_isDashing && Time.time >= _movementLockEndTime)
        {
            StartDash();
        }

        if (_isDashing)
        {
            if (Time.time >= _dashStartTime + _dashDuration) EndDash();
            else _controller.Move(_dashDirection * _dashSpeed * Time.deltaTime);
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashStartTime = Time.time;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= _movementInputThreshold)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            _dashDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else _dashDirection = transform.forward;

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
        if (Time.time < _movementLockEndTime)
        {
            _currentMoveVelocity = Vector3.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal"); 
        float vertical = Input.GetAxisRaw("Vertical");     
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= _movementInputThreshold)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 targetVelocity = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * _topSpeed;
            
            float turnDot = Vector3.Dot(_currentMoveVelocity.normalized, targetVelocity.normalized);
            float activeAcceleration = _acceleration;

            if (turnDot < 0.5f) 
            {
                activeAcceleration *= _directionChangeMultiplier;
            }

            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, targetVelocity, activeAcceleration * Time.deltaTime);
        }
        else
        {
            _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, Vector3.zero, _deceleration * Time.deltaTime);
        }

        _controller.Move(_currentMoveVelocity * Time.deltaTime);
    }

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
        Vector3 holdTarget = _cameraTransform2.position + _cameraTransform2.forward * _holdDistance;
        Vector3 direction = holdTarget - _heldObject.transform.position;
        
        if (Physics.Raycast(_heldObject.transform.position, direction.normalized, out RaycastHit hit, direction.magnitude))
        {
            if (hit.collider.gameObject != _heldObject.gameObject) holdTarget = hit.point - direction.normalized * _holdWallOffset;
        }
        
        float distanceToTarget = Vector3.Distance(_heldObject.transform.position, holdTarget);
        if (distanceToTarget > 0.05f) 
        {
            Vector3 newPosition = Vector3.Lerp(_heldObject.transform.position, holdTarget, Time.deltaTime * _grabLerpSpeed);
            _heldObject.Rigidbody.MovePosition(newPosition);
        }
        
        Quaternion targetRotation = Quaternion.LookRotation(_cameraTransform2.forward);
        _heldObject.Rigidbody.MoveRotation(Quaternion.Lerp(_heldObject.transform.rotation, targetRotation, Time.deltaTime * _grabLerpSpeed));
    }

    private void Release()
    {
        _heldObject.OnRelease();
        _heldObject = null;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PushableObject pushable = hit.collider.GetComponent<PushableObject>();

        if (pushable == null)
        {
            _currentPushable = null;
            return;
        }
        
        Vector3 horizontalMove = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);
        if (horizontalMove.magnitude < _movementInputThreshold)
        {
            pushable.StopPush();
            return;
        }
        
        float dot = Vector3.Dot(transform.forward, -hit.normal);
        if (dot > _pushAngleThreshold)
        {
            pushable.Push(transform.forward);
            _currentPushable = pushable;
        }
        else pushable.StopPush();
    }
}