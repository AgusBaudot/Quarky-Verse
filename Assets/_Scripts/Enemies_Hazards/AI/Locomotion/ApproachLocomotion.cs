using UnityEngine;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(AIAgent), typeof(Rigidbody))]
    public class ApproachLocomotion : MonoBehaviour, IAILocomotion
    {
        private AIAgent _agent;
        private Rigidbody _rb;
        private bool _canMove = true;

        private void Awake()
        {
            _agent = GetComponent<AIAgent>();
            _rb = GetComponent<Rigidbody>();
        }

        public void EnableLocomotion() => _canMove = true;
        public void DisableLocomotion() => _canMove = false;

        private void FixedUpdate()
        {
            if (!_canMove || _agent.Target == null) return;

            Vector3 direction = (_agent.Target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }

            float distance = Vector3.Distance(transform.position, _agent.Target.position);
    
            if (distance > _agent.Stats.AttackRange)
            {
                Vector3 velocity = direction * _agent.Stats.MaxSpeed;
                velocity.y = _rb.velocity.y;
                _rb.velocity = velocity;
            }
            else
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                _agent.InitiateAttack();
            }
        }
    }
}