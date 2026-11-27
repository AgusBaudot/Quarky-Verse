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

    [Header("Grab System")]
    [SerializeField] private float _grabDistance = 6f;
    [SerializeField] private float _grabRadius = 0.5f;
    [SerializeField] private float _holdDistance = 2f;
    [SerializeField] private Transform _cameraTransform2;
    [SerializeField] private Collider _playerCollider;
    private PickableObject _heldObject;
    private PushableObject _currentPushable;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
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
            Vector3 horizontalMove =
                new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);

            bool pushingInput =
                _heldObject == null &&
                Input.GetKey(KeyCode.F) &&
                horizontalMove.magnitude >= 0.1f &&
                Vector3.Dot(transform.forward, horizontalMove.normalized) > 0.5f;

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
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(_dashKey) &&
            Time.time >= _lastDashTime + _dashCooldown &&
            !_isDashing)
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
                _controller.Move(_dashDirection * _dashSpeed * Time.deltaTime);
            }
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashStartTime = Time.time;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                _cameraTransform.eulerAngles.y;

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
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                _cameraTransform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref _turnSmoothVelocity,
                _turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection =
                Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            _currentMoveVelocity = Vector3.MoveTowards(
                _currentMoveVelocity,
                moveDirection * _topSpeed,
                _acceleration * Time.deltaTime
            );
        }
        else
        {
            _currentMoveVelocity = Vector3.MoveTowards(
                _currentMoveVelocity,
                Vector3.zero,
                _deceleration * Time.deltaTime
            );
        }

        _controller.Move(_currentMoveVelocity * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        _isGrounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundDistance,
            _groundMask
        );

        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

        float currentGravity = _gravity;
        if (_velocity.y < 0)
            currentGravity *= _fallMultiplier;

        _velocity.y += currentGravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleGrab()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_heldObject == null)
                TryGrab();
            else
                Release();
        }

        if (_heldObject != null)
            HoldObject();
    }

    private void TryGrab()
    {
        if (Physics.SphereCast(
            _cameraTransform2.position,
            _grabRadius,
            _cameraTransform2.forward,
            out RaycastHit hit,
            _grabDistance))
        {
            PickableObject pickable = hit.collider.GetComponent<PickableObject>();
            if (pickable == null) return;
            if (_currentPushable != null) return;

            if (Physics.Raycast(
                transform.position + Vector3.up * 0.1f,
                Vector3.down,
                out RaycastHit groundHit,
                1.5f))
            {
                if (groundHit.collider == pickable.Collider)
                {
                    Debug.Log("No se puede agarrar un objeto sobre el que estás parado.");
                    return;
                }
            }

            Bounds playerBounds = _playerCollider.bounds;
            Bounds objectBounds = pickable.Collider.bounds;

            bool standingOnObject =
                playerBounds.min.y >= objectBounds.max.y - 0.05f &&
                playerBounds.min.y <= objectBounds.max.y + 0.2f &&
                playerBounds.Intersects(objectBounds);

            if (standingOnObject) return;

            _heldObject = pickable;
            _heldObject.OnGrab();

            Vector3 holdTarget =
                _cameraTransform2.position +
                _cameraTransform2.forward * _holdDistance;

            _heldObject.Rigidbody.position = holdTarget;
            Physics.IgnoreCollision(_playerCollider, pickable.Collider, true);
        }
    }

    private void HoldObject()
    {
        Vector3 holdTarget =
            _cameraTransform2.position +
            _cameraTransform2.forward * _holdDistance;

        Vector3 direction = holdTarget - _heldObject.transform.position;

        if (Physics.Raycast(
            _heldObject.transform.position,
            direction.normalized,
            out RaycastHit hit,
            direction.magnitude))
        {
            if (hit.collider.gameObject != _heldObject.gameObject)
                holdTarget = hit.point - direction.normalized * 0.3f;
        }

        float distanceToTarget =
            Vector3.Distance(_heldObject.transform.position, holdTarget);

        if (distanceToTarget > 0.05f)
        {
            Vector3 newPosition = Vector3.Lerp(
                _heldObject.transform.position,
                holdTarget,
                Time.deltaTime * 15f
            );

            const float minDistance = 1f;

            Vector3 horizontalPlayer =
                new Vector3(transform.position.x, 0f, transform.position.z);

            Vector3 horizontalObject =
                new Vector3(newPosition.x, 0f, newPosition.z);

            if (Vector3.Distance(horizontalPlayer, horizontalObject) > minDistance)
                _heldObject.Rigidbody.MovePosition(newPosition);
        }

        Quaternion targetRotation = Quaternion.LookRotation(_cameraTransform2.forward);

        _heldObject.Rigidbody.MoveRotation(
            Quaternion.Lerp(
                _heldObject.transform.rotation,
                targetRotation,
                Time.deltaTime * 15f
            )
        );
    }

    private void Release()
    {
        Physics.IgnoreCollision(_playerCollider, _heldObject.Collider, false);
        _heldObject.OnRelease();
        _heldObject = null;
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

        Vector3 horizontalMove =
            new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);

        bool pushingTowardObject =
            !_isDashing &&
            horizontalMove.magnitude >= 0.1f &&
            Vector3.Dot(transform.forward, horizontalMove.normalized) > 0.5f &&
            Vector3.Dot(transform.forward, -hit.normal) > 0.5f;

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
}