using UnityEngine;
    
namespace Enemies_Hazards
{
    [RequireComponent(typeof(Collider))]
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField, Tooltip("Check this if this damage dealer requires the player to touch it physically (like spikes), rather than acting as a trigger.")]
        private bool _useCollisionInsteadOfTrigger = false;

        [SerializeField, Tooltip("The tag this hitbox is looking to damage. Use 'Player' for hazards/enemies, and 'Enemy' for player attacks.")]
        private LayerMask _targetLayer;

        private void OnTriggerEnter(Collider other)
        {
            if (_useCollisionInsteadOfTrigger) return;
            TryDealDamage(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_useCollisionInsteadOfTrigger) return;
            TryDealDamage(collision.gameObject);
        }

        private void TryDealDamage(GameObject target)
        {
            if (((1 << target.layer) & _targetLayer.value) != 0)
            {
                Debug.Log(target.name);
                if (target.TryGetComponent<AIAgent>(out var agent))
                {
                    if (agent != null)
                    {
                        agent.TakeDamage();
                        Debug.Log("Enemy eliminated by " + gameObject.name);
                    }
                }
                else if (target.GetComponentInParent<PlayerAnimator>() != null)
                {
                    Debug.Log("Player took damage from " + gameObject.name);
                }
            }
        }
    }
}