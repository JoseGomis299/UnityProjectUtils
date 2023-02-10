using ProjectUtils.ObjectPooling;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectUtils.Attacking
{
    public class Bullet : MonoBehaviour
    {
        private Vector3 _direction;
        private int _damage;
        private float _pushForce;
        private float _bulletSpeed;
        private int _areaDamage;
        private float _damageRadius;
        private int _attackerID;
        private GameObject _explosionDisplay;
        private int _pierceCount;

        public void SetProperties(Vector3 direction, int damage, float pushForce, float bulletSpeed, int pierceCount,
            int areaDamage, float damageRadius, GameObject explosionDisplay, int attackerID)
        {
            _direction = direction;
            _damage = damage;
            _pushForce = pushForce;
            _bulletSpeed = bulletSpeed;
            _areaDamage = areaDamage;
            _damageRadius = damageRadius;
            _attackerID = attackerID;
            _explosionDisplay = explosionDisplay;
            _pierceCount = pierceCount;
        }

        private void FixedUpdate()
        {
            transform.Translate(_direction * (_bulletSpeed * Time.fixedDeltaTime));
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.GetInstanceID() == _attackerID || col.gameObject.GetComponent<Bullet>() != null) return;
            var damageable = col.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.ReceiveDamage(new Damage(transform.position, _damage, _pushForce));
                _pierceCount--;
                if (_pierceCount > 0) return;

                if (_damageRadius > 0)
                {
                    Explode(col.gameObject.GetInstanceID());
                }

                gameObject.SetActive(false);
            }
            else
            {
                if (_damageRadius > 0)
                {
                    Explode(col.gameObject.GetInstanceID());
                }

                gameObject.SetActive(false);
            }
        }

        private void Explode(int damagedID)
        {
            if (_explosionDisplay != null)
            {
                ObjectPool.Instance.InstantiateFromPool(_explosionDisplay, transform.position, quaternion.identity, true)
                    .transform.localScale = -Vector3.one * _damageRadius * 2;
            }

            var hits = Physics2D.OverlapCircleAll(transform.position, _damageRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable iDamageable) && hit.gameObject.GetInstanceID() != damagedID)
                {
                    iDamageable.ReceiveDamage(new Damage(transform.position, _areaDamage, _pushForce));
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _damageRadius);
        }
    }
}
