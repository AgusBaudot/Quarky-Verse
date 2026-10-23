using System;
using UnityEngine;

namespace Enemies_Hazards
{
    public class AIAgent : MonoBehaviour
    {
        [SerializeField] private EnemyStatsSO _stats;

        public EnemyStatsSO Stats => _stats;
        public Transform Target { get; private set; }

        public event Action OnTargetSpotted;
        public event Action OnAttackInitiated;
        public event Action OnDefeated;

        private IAILocomotion _locomotionComponent;
        private IAIAttack _attackComponent;

        private bool _isDefeated = false;

        private void Awake()
        {
            _locomotionComponent = GetComponent<IAILocomotion>();
            _attackComponent = GetComponent<IAIAttack>();
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            //Let it explode if player is not present for some reason.
            Target = player.transform;
        }

        //Called by companion components when conditions are met
        public void InitiateAttack()
        {
            if (_isDefeated) 
                return;
            
            _locomotionComponent?.DisableLocomotion();
            _attackComponent?.TriggerAttack();
            OnAttackInitiated?.Invoke();
        }

        //Called by the WeakPointTrigger
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
    }
}