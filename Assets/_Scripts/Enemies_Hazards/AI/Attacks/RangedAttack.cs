using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace Enemies_Hazards
{
    [RequireComponent(typeof(AIAgent))]
    public class RangedAttack : MonoBehaviour, IAIAttack
    {
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _projectileSpeed = 15f;
        [SerializeField] private float _fireRate = 2f;

        private AIAgent _agent;
        private IObjectPool<Projectile> _pool;
        private Coroutine _fireCoroutine;

        private void Awake()
        {
            _agent = GetComponent<AIAgent>();
            _pool = new ObjectPool<Projectile>(CreateProjectile, OnTakeFromPool, OnReturnedToPool, OnDestroyProjectile,
                true, 10, 20);
        }

        private Projectile CreateProjectile()
        {
            Projectile proj = Instantiate(_projectilePrefab);
            proj.SetPool(_pool);
            return proj;
        }

        private void OnTakeFromPool(Projectile proj) => proj.gameObject.SetActive(true);
        private void OnReturnedToPool(Projectile proj) => proj.gameObject.SetActive(false);
        private void OnDestroyProjectile(Projectile proj) => Destroy(proj.gameObject);

        public void TriggerAttack()
        {
            if (_fireCoroutine == null)
                _fireCoroutine = StartCoroutine(FireRoutine());
        }

        public void CancelAttack()
        {
            if (_fireCoroutine != null)
            {
                StopCoroutine(_fireCoroutine);
                _fireCoroutine = null;
            }
        }

        private IEnumerator FireRoutine()
        {
            while (true)
            {
                Vector3 direction = (_agent.Target.position - _firePoint.position).normalized;

                Projectile projectile = _pool.Get();
                projectile.transform.position = _firePoint.position;
                projectile.transform.rotation = Quaternion.LookRotation(direction);

                projectile.Launch(direction * _projectileSpeed);

                yield return new WaitForSeconds(_fireRate);
            }
        }
    }
}