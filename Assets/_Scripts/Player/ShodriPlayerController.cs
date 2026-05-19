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

    public void Teleport(Vector3 pos, Quaternion rot)
    {
        if (_controller)
            _controller.enabled = false;

        transform.position = pos;
        transform.rotation = rot;

        if (_controller)
            _controller.enabled = true;
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Calculate direction relative to the camera
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
    
}