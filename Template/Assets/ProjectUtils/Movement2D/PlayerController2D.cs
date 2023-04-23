using ProjectUtils.Attacking;
using ProjectUtils.Movement2D;
using ProjectUtils.SavingSystem;
using UnityEngine;
    public class PlayerController2D : Mover
    {
        private Vector3 _direction;
        private Vector3 _lastValidDirection;

        [Header("Cooldown")]
        [SerializeField] private float dashCoolDown = 0.2f;

        private void Awake()
        {
            _lastValidDirection = Vector3.right;
        }

        void Update()
        {
            _direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);
            if (_direction != Vector3.zero) _lastValidDirection = _direction;


            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time - lastDashTime >= dashCoolDown)
            {
                Dash(_lastValidDirection);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (rangedAttack != null)
                {  
                    Vector3 attackDirection = Input.mousePosition;
                    attackDirection = Camera.main.ScreenToWorldPoint(attackDirection);
                    attackDirection.z = 0.0f;
                    attackDirection = (attackDirection-transform.position).normalized;

                   // float attackAngle =Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                   // if (attackAngle<0) attackAngle += 360;
          
                    //rangedAttack.Attack(shootPoint, _lastValidDirection, 2, 2, 10, 1, 0.5f, 10, Vector3.one, 0.1f);
                    //rangedAttack.CircularAttack(10, 1, 2, 1, 10, 1, 0.5f, 10, Vector3.one, 0);
                    rangedAttack.SemiCircularAttack(transform.position+attackDirection, attackDirection,10, 1, 2, 2, 1, 0.5f, 10, Vector3.one, 0,  -45f, 45);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                ReceiveDamage(new Damage(transform.position+Vector3.right, 0, 30));
                if (meleeAttack != null)
                {
                    meleeAttack.AttackCircle(_lastValidDirection, 2, 30, gameObject.GetInstanceID());
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            if (Input.GetKey(KeyCode.Space) && !grounded)
            {
                HoldJumping();
            }
        }

        private void FixedUpdate()
        {
            UpdateMotor(_direction);
        }
        
    }

