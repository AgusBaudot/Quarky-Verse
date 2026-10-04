using UnityEngine;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(AIAgent), typeof(Rigidbody))]
    public class BoundingLocomotion : MonoBehaviour, IAILocomotion
    {
        private AIAgent _agent;
        private Rigidbody _rb;
        private bool _canMove = true;
        private float _jumpTimer;

        private void Awake()
        {
            _agent = GetComponent<AIAgent>();
            _rb = GetComponent<Rigidbody>();
        }

        public void EnableLocomotion() => _canMove = true;
        public void DisableLocomotion() => _canMove = false;

        private void Update()
        {
            if (!_canMove || _agent.Target == null) return;

            _jumpTimer -= Time.deltaTime;
        
            if (_jumpTimer <= 0f && Mathf.Abs(_rb.velocity.y) < 0.1f)
            {
                ExecuteJump();
                _jumpTimer = _agent.Stats.JumpCooldown;
            }
        }

        private void ExecuteJump()
        {
            Vector3 directionToTarget = (_agent.Target.position - transform.position).normalized;
        
            Vector3 jumpForce = new Vector3(
                directionToTarget.x * _agent.Stats.JumpForce.z,
                _agent.Stats.JumpForce.y,
                directionToTarget.z * _agent.Stats.JumpForce.z
            );

            _rb.velocity = Vector3.zero;
            _rb.AddForce(jumpForce, ForceMode.Impulse);
        }
    }
}