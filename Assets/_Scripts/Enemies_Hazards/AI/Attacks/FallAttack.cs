using UnityEngine;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(Rigidbody))]
    public class FallAttack : MonoBehaviour, IAIAttack
    {
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void TriggerAttack() { } 
        public void CancelAttack() { }

        private void OnCollisionEnter(Collision collision)
        {
            if (_rb.velocity.y < -0.1f && collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Jumping Enemy landed on Player!");
            }
        }
    }
}