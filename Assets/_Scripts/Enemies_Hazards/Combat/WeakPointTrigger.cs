using UnityEngine;

namespace Enemies_Hazards
{
    /// <summary>
    /// Places on a child object (e.g., "HeadHitbox") of the enemy with a Trigger Collider.
    /// It checks the player's mathematical state to confirm a head-bounce elimination.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeakPointTrigger : MonoBehaviour
    {
        [SerializeField] private AIAgent _agent;
        [SerializeField] private float _bounceForce = 10f;

        private void Awake()
        {
            if (_agent == null) _agent = GetComponentInParent<AIAgent>();
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OdysseyPlayerController player = other.GetComponent<OdysseyPlayerController>();

                if (player != null && player.VerticalVelocity < 0f)
                {
                    //TODO: player impulse when stepping over enemy.
                    
                    _agent.TakeDamage();
                }
            }
        }
    }
}