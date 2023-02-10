using ProjectUtils.Attacking;
using UnityEngine;

namespace ProjectUtils.TopDown2D
{
    public abstract class Mover : Fighter
    {
        private Vector3 _moveDelta;
        [SerializeField] protected float ySpeed = 3.75f;
        [SerializeField] protected float xSpeed = 4;
        [SerializeField] private LayerMask collisionLayer;
        [SerializeField] private GameObject moverDisplay;
        private RaycastHit2D _hit;
        protected MeleeAttack meleeAttack;
        protected RangedAttack rangedAttack;


        private BoxCollider2D _boxCollider;

        protected virtual void Start()
        {
            if (TryGetComponent(out MeleeAttack meleeAttack)) this.meleeAttack = meleeAttack;
            if (TryGetComponent(out RangedAttack rangedAttack)) this.rangedAttack = rangedAttack;

            _boxCollider = gameObject.GetComponent<BoxCollider2D>();
        }

        protected virtual void UpdateMotor(Vector3 input)
        {
            _moveDelta = new Vector3(input.x * xSpeed, input.y * ySpeed, 0);

            if (_moveDelta.x < 0)
            {
                moverDisplay.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (_moveDelta.x > 0)
            {
                moverDisplay.transform.localScale = new Vector3(1, 1, 1);
            }
            // else if (_moveDelta.y > 0)
            // {
            //     moverDisplay.transform.localScale = new Vector3(1, 1, 1);
            //     //change sprite to walking upwards
            // }
            // else if (_moveDelta.y < 0)
            // {
            //     moverDisplay.transform.localScale = new Vector3(1, 1, 1);
            //     //change sprite to walking downards
            // }

            //Pushes mover if gets hit or dashes
            _moveDelta += pushDirection;
            _moveDelta += dashDirection;
            //Lerps push and dash to 0
            pushDirection = Vector3.Lerp(pushDirection, Vector3.zero, pushRecoverySpeed);
            dashDirection = Vector3.Lerp(dashDirection, Vector3.zero, Time.fixedDeltaTime / 0.075f);

            //Disable ProjectSettings/Physics2D/QueriesStartInColliders to avoid colliding with yourself
            //Detect collision on y
            if (!Physics2D.BoxCast(transform.position, _boxCollider.size, 0, new Vector2(0, _moveDelta.y),
                    Mathf.Abs(_moveDelta.y * Time.fixedDeltaTime), collisionLayer))
                transform.Translate(0, _moveDelta.y * Time.fixedDeltaTime, 0);

            //Detect collision on x
            if (!Physics2D.BoxCast(transform.position, _boxCollider.size, 0, new Vector2(_moveDelta.x, 0),
                    Mathf.Abs(_moveDelta.x * Time.fixedDeltaTime), collisionLayer))
                transform.Translate(_moveDelta.x * Time.fixedDeltaTime, 0, 0);
        }

        protected void Dash(float dashForce, Vector3 direction)
        {
            if (direction != Vector3.zero) dashDirection = direction * dashForce;
            else
            {
                dashDirection = new Vector3(transform.localScale.x, 0, 0) * dashForce;
            }
        }

    }
}