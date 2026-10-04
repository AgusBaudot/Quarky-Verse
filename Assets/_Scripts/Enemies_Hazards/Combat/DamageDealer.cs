using UnityEngine;
    
namespace Enemies_Hazards
{
    [RequireComponent(typeof(Collider))]
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField, Tooltip("Check this if this damage dealer requires the player to touch it physically (like spikes), rather than acting as a trigger.")]
        private bool _useCollisionInsteadOfTrigger = false;

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
            if (target.CompareTag("Player"))
            {
                Debug.Log("Player took damage from " + gameObject.name);
            }
        }
    }
}