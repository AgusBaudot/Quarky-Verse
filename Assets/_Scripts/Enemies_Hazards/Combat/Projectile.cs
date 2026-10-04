using UnityEngine;
using UnityEngine.Pool;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        private IObjectPool<Projectile> _pool;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void SetPool(IObjectPool<Projectile> pool) => _pool = pool;

        public void Launch(Vector3 velocity)
        {
            _rb.velocity = velocity;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Projectile Hit Player!");
            }

            if (_pool != null) _pool.Release(this);
            else gameObject.SetActive(false);
        }
    }
}