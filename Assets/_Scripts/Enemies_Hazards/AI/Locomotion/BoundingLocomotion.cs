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
            Vector3 displacement = _agent.Target.position - transform.position;
            float targetHeight = displacement.y;
            displacement.y = 0;

            float gravity = Physics.gravity.y;
            float verticalVelocity = _agent.Stats.JumpForce.y;

            float timeToApex = -verticalVelocity / gravity;
    
            float apexHeight = (verticalVelocity * verticalVelocity) / (2 * Mathf.Abs(gravity));
    
            float fallHeight = Mathf.Max(0, apexHeight - targetHeight);
            float timeToFall = Mathf.Sqrt(2 * fallHeight / Mathf.Abs(gravity));

            float totalAirTime = timeToApex + timeToFall;

            Vector3 horizontalVelocity = displacement / totalAirTime;

            Vector3 finalVelocity = horizontalVelocity;
            finalVelocity.y = verticalVelocity;

            _rb.velocity = finalVelocity;
        }
    }
}