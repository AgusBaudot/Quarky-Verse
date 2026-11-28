using UnityEngine;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(BoundingLocomotion), typeof(Rigidbody))]
    public class JumpingEnemyAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _jumpStartTrigger = "JumpStart";
        [SerializeField] private string _isGroundedParam = "IsGrounded";
        [SerializeField] private float _groundedVelocityThreshold = 0.1f;

        private BoundingLocomotion _bounding;
        private Rigidbody _rb;

        private int _jumpStartHash;
        private int _isGroundedHash;
        private bool _wasAirborne;

        private void Awake()
        {
            _bounding = GetComponent<BoundingLocomotion>();
            _rb = GetComponent<Rigidbody>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            _jumpStartHash = Animator.StringToHash(_jumpStartTrigger);
            _isGroundedHash = Animator.StringToHash(_isGroundedParam);
        }

        private void OnEnable()
        {
            _bounding.OnJumpStarted += HandleJumpStarted;
        }

        private void OnDisable()
        {
            _bounding.OnJumpStarted -= HandleJumpStarted;
        }

        private void Update()
        {
            if (_animator == null) return;

            bool isGrounded = Mathf.Abs(_rb.velocity.y) < _groundedVelocityThreshold && !_wasAirborne;

            if (_wasAirborne && Mathf.Abs(_rb.velocity.y) < _groundedVelocityThreshold)
            {
                _wasAirborne = false;
                isGrounded = true;
            }

            _animator.SetBool(_isGroundedHash, isGrounded);
        }

        private void HandleJumpStarted()
        {
            _wasAirborne = true;

            if (_animator != null)
            {
                _animator.SetBool(_isGroundedHash, false);
                _animator.SetTrigger(_jumpStartHash);
            }
        }
    }
}