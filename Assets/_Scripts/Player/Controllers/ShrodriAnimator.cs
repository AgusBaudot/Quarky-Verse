using UnityEngine;

[RequireComponent(typeof(OdysseyPlayerController))]
public class ShrodriAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("Animator component on Shrodri's child 3D model. Auto-found if left empty.")]
    private Animator _animator;

    [Header("Settings")]
    [SerializeField, Tooltip("Damping time for smooth Idle <-> Walk transitions")]
    private float _speedDampTime = 0.08f;

    private OdysseyPlayerController _controller;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpStartHash = Animator.StringToHash("JumpStart");
    private static readonly int LandHash = Animator.StringToHash("Land");

    private void Awake()
    {
        _controller = GetComponent<OdysseyPlayerController>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        _controller.OnJump += HandleJump;
        _controller.OnLand += HandleLand;

        if (_animator != null)
        {
            _animator.ResetTrigger(JumpStartHash);
            _animator.ResetTrigger(LandHash);
            _animator.SetFloat(SpeedHash, 0f);
            _animator.SetBool(IsGroundedHash, _controller.IsGrounded);
        }
    }

    private void OnDisable()
    {
        _controller.OnJump -= HandleJump;
        _controller.OnLand -= HandleLand;
    }

    private void Update()
    {
        if (_animator == null) return;

        float horizontalSpeed = _controller.CurrentHorizontalVelocity.magnitude;
        _animator.SetFloat(SpeedHash, horizontalSpeed, _speedDampTime, Time.deltaTime);
        _animator.SetBool(IsGroundedHash, _controller.IsGrounded);
    }

    private void HandleJump()
    {
        if (_animator == null) return;

        _animator.ResetTrigger(LandHash);
        _animator.SetTrigger(JumpStartHash);
    }

    private void HandleLand()
    {
        if (_animator == null) return;

        _animator.ResetTrigger(JumpStartHash);
        _animator.SetTrigger(LandHash);
    }
}