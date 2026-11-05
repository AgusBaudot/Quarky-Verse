using UnityEngine;
using System.Collections;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(AIAgent))]
    public class MeleeAttack : MonoBehaviour, IAIAttack
    {
        [SerializeField] private DamageDealer _damageHitbox;
        [SerializeField] private Animator _animator;
    
        private AIAgent _agent;
        private Coroutine _attackCoroutine;

        private void Awake()
        {
            _agent = GetComponent<AIAgent>();
            if (_damageHitbox != null) _damageHitbox.gameObject.SetActive(false);
        }

        public void TriggerAttack()
        {
            if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
            _attackCoroutine = StartCoroutine(AttackRoutine());
        }

        public void CancelAttack()
        {
            if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
            if (_damageHitbox != null) _damageHitbox.gameObject.SetActive(false);
        }

        private IEnumerator AttackRoutine()
        {
            if (_animator != null) _animator.SetTrigger("Attack");
        
            yield return new WaitForSeconds(_agent.Stats.AttackStartupTime);
        
            if (_damageHitbox != null) _damageHitbox.gameObject.SetActive(true);
            yield return new WaitForSeconds(_agent.Stats.AttackActiveFramesDuration);
        
            if (_damageHitbox != null) _damageHitbox.gameObject.SetActive(false);
        
            GetComponent<IAILocomotion>()?.EnableLocomotion();
        }
    }
}