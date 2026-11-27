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
    [SerializeField] private float _grabDistance = 6f;
    [SerializeField] private float _grabRadius = 0.5f;
    [SerializeField] private float _holdDistance = 2f;
    [SerializeField] private Transform _cameraTransform2;
    [SerializeField] private Transform _holdPoint;
    [SerializeField] private Collider _playerCollider;
    private PickableObject _heldObject;
    private PushableObject _currentPushable;

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
        if (_currentPushable != null)
        {
            Vector3 horizontalMove = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);
            bool pushingInput = _heldObject == null && Input.GetKey(KeyCode.F) && horizontalMove.magnitude >= 0.1f && Vector3.Dot(transform.forward, horizontalMove.normalized) > 0.5f;
            if (pushingInput && _currentPushable.IsWithinRange(transform)) _currentPushable.Push(transform, transform.forward);
            else
            {
                _currentPushable.StopPush();
                _currentPushable = null;
            }
        }
    }
    private void FixedUpdate()
    {
        if (_heldObject != null) HoldObject();
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
        if (Input.GetKeyDown(KeyCode.F) && _heldObject == null && _currentPushable == null) TryGrab();
        if (Input.GetKeyUp(KeyCode.F) && _heldObject != null) Release();
    }
    private void TryGrab()
    {
        if (Physics.SphereCast(_cameraTransform2.position,_grabRadius,_cameraTransform2.forward,out RaycastHit hit,_grabDistance))
        {
            PickableObject pickable = hit.collider.GetComponent<PickableObject>();
            if (pickable == null) return;
            if (_currentPushable != null || !pickable.IsWithinRange(transform)) return;
            // ¿El jugador está parado sobre este objeto?
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f,Vector3.down,out RaycastHit groundHit,1.5f))
            {
                if (groundHit.collider == pickable.Collider)
                {
                    Debug.Log("No se puede agarrar un objeto sobre el que estás parado.");
                    return;
                }
            }
            Bounds playerBounds = _playerCollider.bounds;
            Bounds objectBounds = pickable.Collider.bounds;
            // ¿El jugador está apoyado sobre el objeto?
            bool standingOnObject = playerBounds.min.y >= objectBounds.max.y - 0.05f && playerBounds.min.y <= objectBounds.max.y + 0.2f && playerBounds.Intersects(objectBounds);
            if (standingOnObject) return;
            _heldObject = pickable;
            _heldObject.OnGrab();
            Physics.IgnoreCollision(_playerCollider,pickable.Collider,true);
            Physics.IgnoreCollision(_controller, pickable.Collider, true);
        }
    }
    private void HoldObject()
    {
        Vector3 holdTarget = _holdPoint != null ? _holdPoint.position : _cameraTransform2.position + _cameraTransform2.forward * _heldObject.HoldDistance;
        Vector3 direction = holdTarget - _heldObject.transform.position;
        float distance = direction.magnitude;

        if (distance > 0.01f)
        {
            Vector3 targetPosition = holdTarget;
            float radius = Mathf.Max(0.05f,Mathf.Min(_heldObject.Collider.bounds.extents.x,_heldObject.Collider.bounds.extents.y,_heldObject.Collider.bounds.extents.z));
            RaycastHit[] hits = Physics.SphereCastAll(_heldObject.transform.position,radius,direction.normalized,distance);
            float nearestHit = distance;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == _heldObject.Collider || hit.collider == _playerCollider || hit.collider == _controller) continue;  
                if (hit.distance < nearestHit)
                {
                    nearestHit = hit.distance;
                    targetPosition = hit.point + hit.normal * radius;
                }
            }
            Vector3 nextPosition = Vector3.MoveTowards(_heldObject.Rigidbody.position,targetPosition,15f * Time.fixedDeltaTime);
            _heldObject.Rigidbody.MovePosition(nextPosition);
        }
        if (_cameraTransform2.forward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_cameraTransform2.forward);
            _heldObject.Rigidbody.MoveRotation(Quaternion.Lerp(_heldObject.Rigidbody.rotation,targetRotation,15f * Time.fixedDeltaTime));
        }
    }
    private void Release()
    {
        Physics.IgnoreCollision(_playerCollider, _heldObject.Collider, false);
        Physics.IgnoreCollision(_controller, _heldObject.Collider, false);
        _heldObject.OnRelease();
        _heldObject = null;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PushableObject pushable = hit.collider.GetComponent<PushableObject>();
        if (pushable == null)
        {
            if (_currentPushable != null) _currentPushable.StopPush();
            _currentPushable = null;
            return;
        }
        Vector3 horizontalMove = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);
        bool pushingTowardObject = !_isDashing && horizontalMove.magnitude >= 0.1f && Vector3.Dot(transform.forward, horizontalMove.normalized) > 0.5f && Vector3.Dot(transform.forward, -hit.normal) > 0.5f;
        if (pushingTowardObject && Input.GetKey(KeyCode.F) && pushable.IsWithinRange(transform)) _currentPushable = pushable;
        else
        {
            pushable.StopPush();
            _currentPushable = null;
        }
    }
}