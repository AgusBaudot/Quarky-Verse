using System;
using UnityEngine;

namespace Enemies_Hazards
{
    public class AIAgent : MonoBehaviour
    {
        [SerializeField] private EnemyStatsSO _stats;

        public EnemyStatsSO Stats => _stats;
        public Transform Target { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public bool IsTargetInDetectionRange { get; private set; }

        public Vector3 CurrentDestination =>
            IsTargetInDetectionRange && Target != null ? Target.position : SpawnPosition;

        public event Action OnTargetSpotted;
        public event Action OnTargetLost;
        public event Action OnAttackInitiated;
        public event Action OnDefeated;

        private IAILocomotion _locomotionComponent;
        private IAIAttack _attackComponent;

        private bool _isDefeated = false;

        private void Awake()
        {
            SpawnPosition = transform.position;
            _locomotionComponent = GetComponent<IAILocomotion>();
            _attackComponent = GetComponent<IAIAttack>();
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // Let it explode if player is not present for some reason.
            Target = player.transform;
        }

        private void Update()
        {
            if (_isDefeated || Target == null || _stats == null)
                return;

            float sqrDistance = (Target.position - transform.position).sqrMagnitude;
            bool inRange = sqrDistance <= _stats.DetectionRadius * _stats.DetectionRadius;

            if (inRange != IsTargetInDetectionRange)
            {
                IsTargetInDetectionRange = inRange;

                if (inRange)
                    OnTargetSpotted?.Invoke();
                else
                    OnTargetLost?.Invoke();
            }
        }

        // Called by companion components when conditions are met
        public void InitiateAttack()
        {
            if (_isDefeated)
                return;

            _locomotionComponent?.DisableLocomotion();
            _attackComponent?.TriggerAttack();
            OnAttackInitiated?.Invoke();
        }

        // Called by the WeakPointTrigger
        public void TakeDamage()
        {
            if (_isDefeated)
                return;

            _isDefeated = true;

            _locomotionComponent?.DisableLocomotion();
            _attackComponent?.CancelAttack();

            OnDefeated?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (_stats == null)
                return;

            // Detection radius follows the enemy
            Gizmos.color = Application.isPlaying && IsTargetInDetectionRange
                ? Color.red
                : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _stats.DetectionRadius);

            // Spawn point anchor and return line
            Vector3 spawnAnchor = Application.isPlaying ? SpawnPosition : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnAnchor, _stats.SpawnReturnThreshold);

            if (Application.isPlaying)
                Gizmos.DrawLine(transform.position, spawnAnchor);
        }
    }
}