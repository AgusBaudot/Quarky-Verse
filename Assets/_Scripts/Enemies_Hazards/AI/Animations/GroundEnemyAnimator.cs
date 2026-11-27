using UnityEngine;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(AIAgent), typeof(Rigidbody))]
    public class GroundEnemyAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _speedParam = "Speed";
        [SerializeField] private string _attackTriggerName = "Attack"; // Set to "Shoot" for Ranged

        private AIAgent _agent;
        private Rigidbody _rb;

        private int _speedHash;
        private int _attackHash;

        private void Awake()
        {
            _agent = GetComponent<AIAgent>();
            _rb = GetComponent<Rigidbody>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            _speedHash = Animator.StringToHash(_speedParam);
            _attackHash = Animator.StringToHash(_attackTriggerName);
        }

        private void OnEnable()
        {
            _agent.OnAttackInitiated += HandleAttackInitiated;
        }

        private void OnDisable()
        {
            _agent.OnAttackInitiated -= HandleAttackInitiated;
        }

        private void Update()
        {
            if (_animator == null) return;

            Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            _animator.SetFloat(_speedHash, horizontalVel.magnitude);
        }

        private void HandleAttackInitiated()
        {
            if (_animator != null)
                _animator.SetTrigger(_attackHash);
        }
    }
}