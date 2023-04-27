using ProjectUtils.Helpers;
using UnityEngine;

namespace Templates.TopDownMovement
{
    public class PlayerController : Mover
    {
        private Vector3 _direction;
        private Vector3 _lastValidDirection;

        [SerializeField] private float dashCoolDown;
        private float _lastDashTime;

        private void Awake()
        {
            _lastValidDirection = Vector3.right;
        }

        void Update()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            _direction = new Vector3(x, y, 0);
            if (_direction != Vector3.zero) _lastValidDirection = _direction;


            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time - _lastDashTime >= dashCoolDown)
            {
                _lastDashTime = Time.time;
                Dash(25f, _direction);
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Uncomment and change _direction for attackDirection if we want to aim with mouse

                // Vector3 attackDirection = Input.mousePosition;
                // attackDirection = Camera.main.ScreenToWorldPoint(attackDirection);
                // attackDirection.z = 0.0f;
                // attackDirection = (attackDirection-transform.position).normalized;

                // float attackAngle =Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                //if (attackAngle<0) attackAngle += 360;

                if (rangedAttack != null)
                {
                    Vector3 shootPoint = transform.position +
                                         new Vector3(_lastValidDirection.x * transform.localScale.x / 1.8f,
                                             _lastValidDirection.y * transform.localScale.y / 1.8f,
                                             _lastValidDirection.z * transform.localScale.z / 1.8f);

                    //rangedAttack.Attack(shootPoint, _lastValidDirection, 2, 2, 10, 1, 0.5f, 10, Vector3.one, 0.1f);
                    //rangedAttack.CircularAttack(10, 1, 2, 1, 10, 1, 0.5f, 10, Vector3.one, 0);
                    rangedAttack.SemiCircularAttack(shootPoint, _lastValidDirection,10, 1, 2, 2, 1, 0.5f, 10, Vector3.one, 0,  -45f, 45);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                // Uncomment and change _direction for attackDirection if we want to aim with mouse

                //Vector3 attackDirection = Input.mousePosition;
                //attackDirection = Camera.main.ScreenToWorldPoint(attackDirection);
                //attackDirection.z = 0.0f;
                //attackDirection = (attackDirection-transform.position).normalized;

                if (meleeAttack != null)
                {
                    meleeAttack.AttackCircle(_lastValidDirection, 2, 30, gameObject.GetInstanceID());
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateMotor(_direction);
        }
        
    }
}

