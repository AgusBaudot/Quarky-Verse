using UnityEngine;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(AIAgent), typeof(Rigidbody))]
    public class ChaseLocomotion : MonoBehaviour, IAILocomotion
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
        public void DisableLocomotion()
        {
            _canMove = false;
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        }

        private void FixedUpdate()
        {
            if (!_canMove || _agent.Target == null) return;

            Vector3 destination = _agent.CurrentDestination;
            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0f;

            float horizontalDistance = toDestination.magnitude;

            if (!_agent.IsTargetInDetectionRange && horizontalDistance <= _agent.Stats.SpawnReturnThreshold)
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                return;
            }

            if (horizontalDistance > 0.001f)
            {
                Vector3 direction = toDestination / horizontalDistance;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);

                Vector3 targetVelocity = direction * _agent.Stats.MaxSpeed;
                targetVelocity.y = _rb.velocity.y;
                _rb.velocity = Vector3.MoveTowards(_rb.velocity, targetVelocity, _agent.Stats.Acceleration * Time.fixedDeltaTime);
            }

            if (_agent.IsTargetInDetectionRange &&
                Vector3.Distance(transform.position, _agent.Target.position) <= _agent.Stats.AttackRange)
            {
                _agent.InitiateAttack();
            }
        }
    }
}