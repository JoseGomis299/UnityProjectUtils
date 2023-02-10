using UnityEngine;

namespace ProjectUtils.Attacking
{
    public class MeleeAttack : MonoBehaviour
    {
        [SerializeField] private float attackCoolDown;
        private float _lastAttackTime;

        [SerializeField] private float distance;

        [Header("Circle Attack Parameters")] [SerializeField]
        private float radius;

        [Header("Square Attack Parameters")] [SerializeField]
        private Vector2 squareDimensions;


        public void AttackCircle(Vector3 direction, int attackDamage, float pushForce, int attackerID)
        {
            if (Time.time - _lastAttackTime < attackCoolDown) return;
            _lastAttackTime = Time.time;

            Vector3 attackPosition = transform.position + distance * direction;
            var hits = Physics2D.OverlapCircleAll(attackPosition, radius);
            foreach (var hit in hits)
            {
                if (hit.gameObject.GetInstanceID() == attackerID) continue;
                var damageable = hit.GetComponent<IDamageable>();
                damageable?.ReceiveDamage(new Damage(transform.position, attackDamage, pushForce));
            }
        }

        public void AttackSquare(Vector3 direction, int attackDamage, float pushForce, int attackerID)
        {
            if (Time.time - _lastAttackTime < attackCoolDown) return;
            _lastAttackTime = Time.time;

            Vector3 attackPosition = transform.position + distance * direction;
            float attackAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (attackAngle < 0) attackAngle += 360;

            Debug.Log(direction);
            Debug.Log(attackAngle);
            var hits = Physics2D.OverlapBoxAll(attackPosition, squareDimensions, attackAngle);
            foreach (var hit in hits)
            {
                if (hit.gameObject.GetInstanceID() == attackerID) continue;
                var damageable = hit.GetComponent<IDamageable>();
                damageable?.ReceiveDamage(new Damage(transform.position, attackDamage, pushForce));
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + distance * Vector3.right, radius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + distance * Vector3.right, squareDimensions);
        }
    }
}
