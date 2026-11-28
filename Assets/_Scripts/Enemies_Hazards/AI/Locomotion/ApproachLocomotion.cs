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

            Vector3 destination = _agent.CurrentDestination;
            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0f;

            float horizontalDistance = toDestination.magnitude;
            float stopDistance = _agent.IsTargetInDetectionRange
                ? _agent.Stats.AttackRange
                : _agent.Stats.SpawnReturnThreshold;

            if (horizontalDistance > stopDistance)
            {
                Vector3 direction = toDestination / horizontalDistance;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);

                Vector3 velocity = direction * _agent.Stats.MaxSpeed;
                velocity.y = _rb.velocity.y;
                _rb.velocity = velocity;
            }
            else
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);

                if (_agent.IsTargetInDetectionRange)
                    _agent.InitiateAttack();
            }
        }
    }
}