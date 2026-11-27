using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        private IObjectPool<Projectile> _pool;
        private Rigidbody _rb;
        private bool _isReleased;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _isReleased = false;
        }

        private void ReturnToPool()
        {
            if (_isReleased)
                return;
            
            _isReleased = true;
            _pool.Release(this);
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

            if (_pool != null) 
                ReturnToPool();
            else 
                gameObject.SetActive(false);
        }
    }
}