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
    public CharacterController Controller => _controller;
    public bool IsHoldingObject => _heldObject != null;

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
    [SerializeField] private Collider _playerCollider;

    [Header("Grab & Push System")]
    [SerializeField] private float _grabDistance = 6f;
    [SerializeField] private float _grabRadius = 0.5f;
    [SerializeField, Tooltip("Distancia frente al PERSONAJE (recomendado: 2.2 a 3)")]
    private float _holdDistance = 2.5f;
    [SerializeField, Tooltip("Altura del pivote desde los pies del personaje")]
    private float _holdHeightOffset = 1.2f;
    [SerializeField, Tooltip("Qué tan rápido sigue la mira (más alto = menos delay)")]
    private float _holdFollowSpeed = 20f;
    [SerializeField, Tooltip("Velocidad máxima de la caja para evitar que atraviese paredes")]
    private float _maxHoldVelocity = 25f;
    [SerializeField, Tooltip("Radio mínimo para rodear al jugador en giros bruscos")]
    private float _playerAvoidRadius = 1.1f;
    [SerializeField, Tooltip("Si la caja queda trabada atrás de una pared y te alejás más que esto, se suelta")]
    private float _maxBreakDistance = 4.5f;
    [SerializeField, Tooltip("Si está activo, la caja solo gira horizontalmente con la cámara sin inclinarse")]
    private bool _rotateWithCameraY = true;
    [SerializeField, Tooltip("Capas que frenan la caja (excluir la capa Player)")]
    private LayerMask _obstacleMask = ~0;
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
    private Quaternion _initialGrabRotation;

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
        HandleGrabInput();

        if (!_isDashing)
        {
            HandleGravityAndJump();
            HandleMovement();
        }

        HandlePush();
    }

    private void FixedUpdate()
    {
        if (_heldObject != null)
        {
            HoldObjectPhysics();
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

    // =========================
    // GRAB SYSTEM
    // =========================

    private void HandleGrabInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_heldObject == null)
                TryGrab();
            else
                Release();
        }
    }

    private void TryGrab()
    {
        if (_currentPushable != null) return;

        Vector3 playerPivot = transform.position + Vector3.up * _holdHeightOffset;

        // Compensa distancia cámara-personaje para que apuntar desde abajo tenga el mismo alcance
        float camToPlayerDist = Vector3.Dot(playerPivot - _cameraTransform2.position, _cameraTransform2.forward);
        float totalCastDistance = Mathf.Max(0f, camToPlayerDist) + _grabDistance;

        RaycastHit[] hits = Physics.SphereCastAll(
            _cameraTransform2.position,
            _grabRadius,
            _cameraTransform2.forward,
            totalCastDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (IsPlayerCollider(hit.collider)) continue;

            PickableObject pickable = hit.collider.GetComponentInParent<PickableObject>();
            if (pickable == null) continue;

            Vector3 closestPoint = hit.collider.bounds.ClosestPoint(playerPivot);
            if (Vector3.Distance(playerPivot, closestPoint) > _grabDistance) continue;

            Vector3 checkPoint = hit.distance > 0f ? hit.point : pickable.Collider.bounds.center;
            if (Physics.Linecast(
                playerPivot,
                checkPoint,
                out RaycastHit wallHit,
                _obstacleMask,
                QueryTriggerInteraction.Ignore))
            {
                if (!wallHit.collider.transform.IsChildOf(pickable.transform) && !IsPlayerCollider(wallHit.collider))
                {
                    continue;
                }
            }

            if (IsStandingOnObject(pickable))
            {
                Debug.Log("No se puede agarrar un objeto sobre el que estás parado.");
                return;
            }

            _heldObject = pickable;
            _heldObject.OnGrab();

            _initialGrabRotation = Quaternion.Euler(0f, _heldObject.transform.eulerAngles.y, 0f);
            SetIgnorePlayerCollision(_heldObject.Collider, true);
            return;
        }
    }

    private bool IsPlayerCollider(Collider col)
    {
        return col == _controller ||
               col == _playerCollider ||
               col.transform.IsChildOf(transform);
    }

    private bool IsStandingOnObject(PickableObject pickable)
    {
        if (Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            out RaycastHit groundHit,
            0.5f,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            if (groundHit.collider == pickable.Collider)
                return true;
        }

        Bounds playerBounds = _controller.bounds;
        Bounds objectBounds = pickable.Collider.bounds;

        return playerBounds.min.y >= objectBounds.max.y - 0.05f &&
               playerBounds.min.y <= objectBounds.max.y + 0.15f &&
               playerBounds.Intersects(objectBounds);
    }

    private void HoldObjectPhysics()
    {
        Rigidbody rb = _heldObject.Rigidbody;
        Vector3 playerPivot = transform.position + Vector3.up * _holdHeightOffset;

        float camToPlayerDist = Vector3.Dot(playerPivot - _cameraTransform2.position, _cameraTransform2.forward);
        camToPlayerDist = Mathf.Max(0f, camToPlayerDist);

        Vector3 desiredTarget = _cameraTransform2.position + _cameraTransform2.forward * (camToPlayerDist + _holdDistance);

        Vector3 pivotToTarget = desiredTarget - playerPivot;
        float targetDistance = pivotToTarget.magnitude;
        float objectRadius = Mathf.Max(_heldObject.Collider.bounds.extents.x, _heldObject.Collider.bounds.extents.z);

        if (targetDistance > 0.01f)
        {
            if (Physics.SphereCast(
                playerPivot,
                objectRadius * 0.85f,
                pivotToTarget.normalized,
                out RaycastHit wallHit,
                targetDistance,
                _obstacleMask,
                QueryTriggerInteraction.Ignore))
            {
                if (!wallHit.collider.transform.IsChildOf(_heldObject.transform) && !IsPlayerCollider(wallHit.collider))
                {
                    desiredTarget = playerPivot + pivotToTarget.normalized * Mathf.Max(0.8f, wallHit.distance);
                }
            }
        }

        if (Vector3.Distance(rb.position, desiredTarget) > _maxBreakDistance)
        {
            Release();
            return;
        }

        Vector3 moveTarget = desiredTarget;
        Vector3 toObjFlat = rb.position - playerPivot;
        toObjFlat.y = 0f;
        Vector3 toTargetFlat = desiredTarget - playerPivot;
        toTargetFlat.y = 0f;

        if (toObjFlat.sqrMagnitude > 0.01f && toTargetFlat.sqrMagnitude > 0.01f)
        {
            float angle = Vector3.Angle(toObjFlat, toTargetFlat);
            if (angle > 95f)
            {
                Vector3 cross = Vector3.Cross(toObjFlat, toTargetFlat);
                Vector3 sideDir = Vector3.Cross(Vector3.up, toObjFlat.normalized) * (cross.y >= 0f ? 1f : -1f);
                moveTarget = playerPivot + sideDir * Mathf.Max(_playerAvoidRadius, _holdDistance * 0.8f);
                moveTarget.y = desiredTarget.y;
            }
        }

        Vector3 velocity = (moveTarget - rb.position) * _holdFollowSpeed;
        if (velocity.magnitude > _maxHoldVelocity)
        {
            velocity = velocity.normalized * _maxHoldVelocity;
        }
        rb.velocity = velocity;

        Quaternion targetRotation = _initialGrabRotation;
        if (_rotateWithCameraY)
        {
            Vector3 flatForward = _cameraTransform2.forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude > 0.001f)
            {
                targetRotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
            }
        }

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 15f));
    }

    private void Release()
    {
        if (_heldObject == null) return;

        SetIgnorePlayerCollision(_heldObject.Collider, false);
        _heldObject.OnRelease();
        _heldObject = null;
    }

    private void SetIgnorePlayerCollision(Collider targetCollider, bool ignore)
    {
        if (targetCollider == null) return;

        if (_controller != null)
            Physics.IgnoreCollision(_controller, targetCollider, ignore);

        if (_playerCollider != null && _playerCollider != _controller)
            Physics.IgnoreCollision(_playerCollider, targetCollider, ignore);
    }

    // =========================
    // PUSH SYSTEM
    // =========================

    private void HandlePush()
    {
        if (_currentPushable == null) return;

        Vector3 horizontalMove = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);

        bool pushingInput =
            !_isDashing &&
            _heldObject == null &&
            Input.GetKey(KeyCode.F) &&
            horizontalMove.magnitude >= _movementInputThreshold &&
            Vector3.Dot(transform.forward, horizontalMove.normalized) > _pushAngleThreshold;

        if (pushingInput && _currentPushable.IsWithinRange(transform))
        {
            _currentPushable.Push(transform, transform.forward);
        }
        else
        {
            _currentPushable.StopPush();
            _currentPushable = null;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PushableObject pushable = hit.collider.GetComponent<PushableObject>();

        if (pushable == null)
        {
            if (_currentPushable != null)
                _currentPushable.StopPush();

            _currentPushable = null;
            return;
        }

        Vector3 horizontalMove = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);

        bool pushingTowardObject =
            !_isDashing &&
            _heldObject == null &&
            horizontalMove.magnitude >= _movementInputThreshold &&
            Vector3.Dot(transform.forward, horizontalMove.normalized) > _pushAngleThreshold &&
            Vector3.Dot(transform.forward, -hit.normal) > _pushAngleThreshold;

        if (pushingTowardObject &&
            Input.GetKey(KeyCode.F) &&
            pushable.IsWithinRange(transform))
        {
            _currentPushable = pushable;
        }
        else
        {
            pushable.StopPush();
            _currentPushable = null;
        }
    }

    // =========================
    // EXTERNAL CONTROL
    // =========================

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        if (_controller != null)
            _controller.enabled = false;

        transform.position = position;
        transform.rotation = rotation;

        if (_controller != null)
            _controller.enabled = true;
    }

    public void OnControlLost()
    {
        if (_heldObject != null) Release();
        if (_currentPushable != null)
        {
            _currentPushable.StopPush();
            _currentPushable = null;
        }

        _isDashing = false;
        _currentMoveVelocity = Vector3.zero;
        _velocity = Vector3.zero;
    }
}