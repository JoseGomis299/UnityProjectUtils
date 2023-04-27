using ProjectUtils.ObjectPooling;
using Templates.Attacking;
using UnityEngine;

namespace Templates.Movement2D
{
    public abstract class Mover : Fighter
    {
        [Header("Movement")]
        [SerializeField] protected float speed = 8;
        [SerializeField] protected float airControl = 0.25f;
        [SerializeField] private LayerMask collisionLayer;
        [SerializeField] public bool canClimb = true;
        [SerializeField] public bool canDash = true;
        private bool _canDash;
        [SerializeField] private float dashForce = 35;
        private Vector3 _dashDirection;
        [SerializeField] private GameObject dashEcho;
        private Vector3 _moveDelta;
        //Recommended GravityScale = 8
        private Rigidbody2D _rb;
        private RaycastHit2D _hit;
        private float _climbingDirection;
        protected float lastDashTime;


        [Header("Attacking")]
        protected MeleeAttack meleeAttack;
        protected RangedAttack rangedAttack;
        

        [Header("Jumping")]
        [SerializeField] protected float jumpForce = 21;
        private float _coyoteTime;
        private float _jumpBufferTime;
        protected bool grounded;
        [SerializeField] private float jumpStartTime = 0.15f;
        private float _jumpTime;
        private bool _jumping;

        private float _initialGravityScale;
        
        private CapsuleCollider2D _capsuleCollider;
        private Vector3 _collisionCenter;

        [Header("States")]
        public MovementState state;
        public enum MovementState
        {
            walking,
            onSlope,
            dashing,
            climbing,
            air
        }
        
        protected virtual void Start()
        {
            base.Start();
            
            if (TryGetComponent(out MeleeAttack meleeAttack)) this.meleeAttack = meleeAttack;
            if (TryGetComponent(out RangedAttack rangedAttack)) this.rangedAttack = rangedAttack;

            _rb = GetComponent<Rigidbody2D>();
            _initialGravityScale = _rb.gravityScale;
            _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();    
            
            _canDash = canDash;
        }
        protected void UpdateMotor(Vector3 input)
        {
            if (state == MovementState.dashing)
            {
                lastImmune = Mathf.Infinity;
                DoDash();
                return;
            }

            //set variables
            _collisionCenter = new Vector3(transform.position.x + _capsuleCollider.offset.x, transform.position.y + _capsuleCollider.offset.y, 0);
            var slopeDirection = GetSlopeDirection();
            
            //Perform movement
            CheckState(slopeDirection);
            CheckGround();
            Move(input, slopeDirection);
            
            if (canClimb && !grounded)
            {
                Climb(input);
            }

            //Resetting dash and jump
            if (!grounded && _climbingDirection == 0) return;
            //Check if JumpBuffer has input
            if (Time.time - _jumpBufferTime < 0.1f) Jump();
            _canDash = true;
            _jumping = false;
        }

        #region Movement
        private void Move(Vector3 input, Vector3 slopeDirection)
        {
            //Movement taking into account if it is on a slope
            if (state == MovementState.onSlope && !_jumping)
            {
                _moveDelta = new Vector3(slopeDirection.x * speed * -input.x, slopeDirection.y * speed * -input.x, 0);
            }
            else
            {
                _moveDelta = new Vector3(input.x * speed, _rb.velocity.y, 0);
            }

            //Changing GFX direction
            if (_moveDelta.x < 0)
            {
                fighterGFX.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (_moveDelta.x > 0)
            {
                fighterGFX.transform.localScale = new Vector3(1, 1, 1);
            }
            
            //Pushes mover if gets hit 
            _moveDelta += pushDirection;
            //Lerps push to 0
            pushDirection = Vector3.Lerp(pushDirection, Vector3.zero, Time.fixedDeltaTime / 0.1f);
            
            //Move position
            Vector2 targetVelocity = _moveDelta;
            if (!grounded)
            {
                var temp = Mathf.Lerp(_rb.velocity.x, _moveDelta.x, Time.deltaTime * 20f * airControl);
                targetVelocity = new Vector2(temp, _rb.velocity.y);
            }
            //Vector3 targetVelocity = grounded ? _moveDelta : Vector2.Lerp(_rb.velocity, _moveDelta, Time.deltaTime * 20f * airControl);
            _rb.velocity = (Vector2)targetVelocity;
            //change gravity if onSlope
            _rb.gravityScale = targetVelocity.y <= 0.05f && state == MovementState.onSlope ? 0 : _initialGravityScale;
        }
        
        private void Climb(Vector3 input)
        {
            //Check if colliding with wall
            RaycastHit2D hit = Physics2D.CapsuleCast(_collisionCenter, _capsuleCollider.size, _capsuleCollider.direction, 0, input, 0.1f, collisionLayer);

            if (hit && hit.transform.CompareTag("Climbable"))
            {
                _climbingDirection = input.x; 
                if(_rb.velocity.y < 0) _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * 0.7f);
            }
            else
            {
                _climbingDirection = 0;
            }
        }
        
        protected void Dash(Vector3 direction)
        {
            if(!canDash || !_canDash) return;
            _dashDirection = direction;
            
            state = MovementState.dashing;
            if (GetSlopeDirection() != Vector3.right) _canDash = false;
            _rb.velocity = Vector2.zero;
            lastDashTime = Time.time;
            _rb.gravityScale = 0;
        }
        
        private void DoDash()
        {
            if (Time.time - lastDashTime >= 0.1f)
            {
                CheckState(GetSlopeDirection());
                
                _rb.gravityScale = _initialGravityScale;
                if (!grounded) _canDash = false;
                lastImmune = 0;
                return;
            }

            _rb.MovePosition(transform.position+_dashDirection * (dashForce*Time.deltaTime));
            if (dashEcho != null)
            {
                var echo = ObjectPool.Instance.InstantiateFromPool(dashEcho, transform.position, Quaternion.identity, true);
                echo.transform.rotation = fighterGFX.transform.rotation;
                echo.transform.localScale = fighterGFX.transform.localScale;
                echo.GetComponent<SpriteRenderer>().sprite = fighterGFX.GetComponent<SpriteRenderer>().sprite;
            }
        }
        
        protected void Jump()
        {
            if (!grounded && Time.time - _coyoteTime > 0.15f && _climbingDirection == 0)
            {
                _jumpBufferTime = Time.time;
                return;
            }

            _jumping = true;
            _rb.velocity = _climbingDirection != 0 ? new Vector2(-_climbingDirection*jumpForce, jumpForce*0.55f) : new Vector2(_rb.velocity.x, jumpForce);
            _coyoteTime = float.MinValue;
            _jumpTime = jumpStartTime;
        }

        protected void HoldJumping()
        {
            if (_rb.velocity.y <= 0) {_jumpTime = 0; return;}
            if (_jumpTime <= 0) return;
            
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y+ jumpForce/100);
            _jumpTime -= Time.deltaTime;
        }
        
        #endregion

        #region Checks

        private Vector3 GetSlopeDirection()
        {   
            RaycastHit2D hit = Physics2D.Raycast(_collisionCenter,  Vector3.down, _capsuleCollider.size.y/2+0.5f, collisionLayer);
            if (hit && Vector3.Angle(transform.up, hit.normal) != 0)
            {
                //Debug.Log(-Vector3.RotateTowards(hit.normal, hit.transform.right, -Mathf.PI/2, 1));
                return Vector3.RotateTowards(hit.normal, hit.transform.right, -Mathf.PI/2, 1);
            }
            return Vector3.right;
        }
        
        private void CheckGround()
        {
            if (Physics2D.CapsuleCast(_collisionCenter, _capsuleCollider.size, _capsuleCollider.direction, 0, Vector2.down, 0.1f, collisionLayer))
            {
                grounded = true;
                if (_rb.velocity.y <= 0) _coyoteTime = Time.time;
            }
            else
            {
                grounded = false;
            }
        }

        private void CheckState(Vector3 slopeDirection)
        {
            if (grounded)
            {
                state = slopeDirection != Vector3.right ? MovementState.onSlope : MovementState.walking;
            }
            else
            {
                state = _climbingDirection != 0 ? MovementState.climbing : MovementState.air;
            }
        }

        #endregion
       

    }
}