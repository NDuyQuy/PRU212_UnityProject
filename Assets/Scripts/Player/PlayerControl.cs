using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayerControl : BaseCharacterScript
{
    #region Player_Animation_Enum
    public enum PlayerAnimation
    {
        Jump,
        Attack,
        AirAttack,
        Slide,
        WallJump,
        Attack1,
        Attack2,
        Attack3,
        Idle,
        Fall,
        Walk,
        Run,
        Crouch,
        WallSlide,
        Kick,
        Archery,
        AirArchery,
        SAttack1,
        SAttack2,
        SAttack3,
        Die
    }
    #endregion
    public bool Weapon = false;//weapon equiped(hand-sword)
    public bool SideWeapon = false;//side weapon equiped(kick-bow)

    private GameObject _currentArrow;
    private Transform _arrowStart;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float dashingPower = 3f;
    private Animator animator;

    private float _originalGravity;
    private string currentAnimaton;

    #region movement_ralated_property
    private float xAxis;
    private bool _isMoving;
    private bool isJumpPressed;
    public float groundCastDistance = 1.3f;
    public float ceilingCastDistance = 1.0f;
    private bool grounded;
    [SerializeField, Range(0, 5)] private byte _maxAirJump = 1;
    private byte _airJumpCount;
    [SerializeField] private float jumpHeight = 15f;//height of the jump
    private byte _airState = 0;//0 mean grounded, 1 mean up to air, 2 mean is falling
    private readonly float _coyoteTime = 0.1f;

    #endregion

    #region AtackProperty
    private bool isAttackPressed;
    private bool _isAirShooting;
    private bool _isSideAttackPressed;
    [SerializeField] private Vector2 _airKickForce = new (500,-500);
    [SerializeField] private float attackDelay = 0.3f;
    [SerializeField] private float sideAttackDelay = 0.5f;
    private bool isAttacking;
    public Transform AttackPoint;
    public Transform AirKickPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    [SerializeField] private sbyte attackDamage = 20;
    private sbyte _attackCount = 0;
    private bool _isAirKick = false;
    #endregion
    private bool isFacingRight = true;  // For determining which way the player is currently facing.
    #region Slide/DashProperty
    private bool isSliding;
    private bool canDash = true;
    [SerializeField] private float dashingTime = 0.3f;
    [SerializeField] private float dashingCooldown = 2f;
    private bool _canAirDash = false;
    #endregion
    #region crouch_property
    private bool isCrouching;
    [SerializeField] private float crouchSlowdown = 0.4f;
    private BoxCollider2D boxCollider2d;
    #endregion

    public LayerMask groundLayer;

    #region wall_related_property
    public LayerMask wallLayer;
    public Transform wallCheck;
    public float wallDistance = 0.2f;
    private bool isWallSliding = false;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float WallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(3f, 7f);
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        _arrowStart = transform.Find("ArrowPoint");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDead)
        {
            this.Die();
            currentHealth++;
            return;
        }
        if (isWallJumping) return;
        if (isSliding) return;
        IsGrounded();
        if (grounded)
        {
            _airJumpCount = 0;
            _airState = 0;
            _isAirKick = false;
        }

        if (rb2d.velocity.y > 0)
        {
            _airState = 1;
        }
        else if (rb2d.velocity.y < 0 && !isWallSliding && !grounded)
        {
            _airState = 2;
        }
        xAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space)) isJumpPressed = true;

        if (Input.GetKeyDown(KeyCode.J)) isAttackPressed = true;

        if (Input.GetKeyDown(KeyCode.S)) isCrouching = true;
        else if (Input.GetKeyUp(KeyCode.S) ||!Input.GetKeyDown(KeyCode.S)) 
        {
            if(!HasCeiling) isCrouching = false;
        }
        if (SideAttackPressed()) _isSideAttackPressed = true;
        if (Input.GetKeyDown(KeyCode.L) && canDash)
        {
            StartCoroutine(Dash());
        }
        _isMoving = xAxis != 0 || isJumpPressed;
        WallJumping();
        WallSlide();
        Crouch();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isSliding) return;
        if (isWallJumping) return;
        if(_isAirKick) return;

        Vector2 vel = MovementVelocity();
        //asign new velocity to the rigid body
        rb2d.velocity = vel;
        if (xAxis > 0 && !isFacingRight) Flip();
        if (xAxis < 0 && isFacingRight) Flip();


        //check and perform attack if attacking
        Attack();
        SideAttack();

    }

    #region crouch
    private void Crouch()
    {
        boxCollider2d.enabled = !isCrouching;
    }
    private bool HasCeiling
    {
        get => Physics2D.BoxCast(transform.position,boxSize,0,transform.up,ceilingCastDistance,groundLayer);
    }
         
    #endregion
    #region jump_related_method
    public bool IsGrounded()
    {
        grounded = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, groundCastDistance, groundLayer);
        return grounded;
    }
    #endregion
    #region  wall_jumping_related_method
    private bool IsOnWall()
    {
        var hit = Physics2D.OverlapCircle(wallCheck.position, wallDistance, wallLayer);
        return hit && !grounded;
    }
    private void WallSlide()
    {
        if (IsOnWall() && xAxis != 0f)
        {
            isWallSliding = true;
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Clamp(rb2d.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else isWallSliding = false;
    }
    private void WallJumping()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            WallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;//allow wall jump for a brief moment
        }

        if (isJumpPressed && wallJumpingCounter > 0f)
        {
            isJumpPressed = false;
            isWallJumping = true;
            rb2d.velocity = new Vector2(WallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0;
            Flip();
            ChangeAnimationState(nameof(PlayerAnimation.WallJump));

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }
    #endregion
    #region gizmos_drawing
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * groundCastDistance, boxSize);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + transform.up*ceilingCastDistance,boxSize);
    }
    #endregion
    #region attack_related_method
    private void Attack()
    {
        if (isAttackPressed)
        {
            isAttackPressed = false;
            if (!isAttacking)
            {
                isAttacking = true;
                _attackCount++;
                _attackCount = (sbyte)((_attackCount > 3) ? 1 : _attackCount);//hand attack only has 3 step 
                if (grounded)
                {
                    //Weapon = true -> has sword
                    string attackAnimation = (Weapon ? "SAttack" : "Attack") + _attackCount.ToString();
                    ChangeAnimationState(attackAnimation);
                }
                else
                {
                    _isAirKick = true;
                    Vector2 multiplyForce = isFacingRight?new(1,1):new(-1,1);
                    rb2d.AddForce(_airKickForce*multiplyForce);
                    ChangeAnimationState(nameof(PlayerAnimation.AirAttack));
                }
                HitDamage(10);
                Invoke(nameof(AttackComplete), attackDelay);
            }
        }
    }
    private void SideAttack()
    {
        if (_isSideAttackPressed)
        {
            currentHealth--;
            _isSideAttackPressed = false;
            if (!isAttacking)
            {
                isAttacking = true;
                if (!SideWeapon)
                {
                    ChangeAnimationState(nameof(PlayerAnimation.Kick));
                    HitDamage();
                }
                else
                {
                    if (
                        (!isFacingRight && _arrow.transform.localScale.x > 0)
                        || (isFacingRight && _arrow.transform.localScale.x < 0)
                        )
                    {
                        Vector2 newScale = _arrow.transform.localScale;
                        newScale.x *= -1;
                        _arrow.transform.localScale = newScale;
                    }

                    _currentArrow = Instantiate(_arrow, _arrowStart.position, Quaternion.identity);
                    ChangeAnimationState(grounded ?
                                        nameof(PlayerAnimation.Archery) :
                                        nameof(PlayerAnimation.AirArchery));
                    _isAirShooting = !grounded;
                    if (_isAirShooting)
                    {
                        _originalGravity = rb2d.gravityScale;
                        rb2d.gravityScale = 0;
                        rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
                    }
                }
                Invoke(nameof(AttackComplete), sideAttackDelay);
            }
        }
    }

    void AttackComplete()
    {
        isAttacking = false;
        if (_currentArrow != null)
        {
            var arrowController = _currentArrow.GetComponent<Arrow>();
            arrowController.Shoot(xDirection: isFacingRight ? 1 : -1);
        }
        if (_isAirShooting)
        {
            _isAirShooting = false;
            rb2d.gravityScale = _originalGravity;
        }
    }
    private void HitDamage(sbyte damage = 0)
    {
        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(_isAirKick==false ? AttackPoint.position : AirKickPoint.position
                                        , attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            var baseScript = enemy.GetComponent<BaseCharacterScript>();
            baseScript.TakeDamage(damage);
        }
    }
    #endregion
    //animation manager
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector2 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private IEnumerator Dash()
    {
        //allow player only dash when on ground or air dash avaiable
        if (grounded || _canAirDash)
        {
            canDash = false;
            isSliding = true;
            float originalGravity = rb2d.gravityScale;
            rb2d.gravityScale = 0f;
            rb2d.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
            if (grounded)
                ChangeAnimationState(nameof(PlayerAnimation.Slide));
            else ChangeAnimationState("");

            yield return new WaitForSeconds(dashingTime);

            // if (grounded)
            //     ChangeAnimationState(nameof(PlayerAnimation.));
            // yield return new WaitForSeconds(0.2f);
            rb2d.gravityScale = originalGravity;
            isSliding = false;
            yield return new WaitForSeconds(dashingCooldown);
            canDash = true;
        }
    }
    private Vector2 MovementVelocity()
    {
        Vector2 vel = new(rb2d.velocity.x, rb2d.velocity.y);

        if (xAxis != 0)
        {
            vel.x = isCrouching ? (xAxis * walkSpeed * crouchSlowdown) : (xAxis * walkSpeed);
        }
        else
        {
            vel.x = 0;
        }
        if (isJumpPressed)
        {
            isJumpPressed = false;
            if (grounded || _airJumpCount < _maxAirJump)
            {
                _airJumpCount++;
                vel.y = jumpHeight;
                ChangeAnimationState(grounded ? nameof(PlayerAnimation.Jump) : nameof(PlayerAnimation.WallJump));
            }
        }

        return vel;
    }

    private void UpdateAnimation()
    {
        if (isWallSliding)
        {
            ChangeAnimationState(nameof(PlayerAnimation.WallSlide));
        }
        if (grounded && !isAttacking && !isSliding)
        {
            if (xAxis != 0)
            {
                if (isCrouching)
                {
                    ChangeAnimationState(nameof(PlayerAnimation.Crouch));
                }
                else
                {
                    if (Mathf.Abs(walkSpeed) > 9)
                        ChangeAnimationState(nameof(PlayerAnimation.Run));
                    else
                        ChangeAnimationState(nameof(PlayerAnimation.Walk));
                }
            }
            else
            {
                ChangeAnimationState(nameof(PlayerAnimation.Idle));
            }
        }
        if (_airState == 2 && !isAttacking && !_isAirKick)
        {
            ChangeAnimationState(nameof(PlayerAnimation.Fall));
        }
    }
    private bool SideAttackPressed() => Input.GetKeyDown(KeyCode.U);

    protected override void Die()
    {
        float animationLength = animator.runtimeAnimatorController
                                .animationClips
                                .FirstOrDefault(c => c.name == nameof(PlayerAnimation.Die)).length;
        ChangeAnimationState(nameof(PlayerAnimation.Die));
        // Debug.Log(animationLength);
        float elapsedTime = 0f;
        while (elapsedTime < animationLength)
        {
            elapsedTime += Time.deltaTime;
        }
        base.Die();
    }

    private bool IsDead => currentHealth <= 0;

}
