using System;
using System.Collections;
using UnityEngine;

public class PlayerScript : BaseCharacterScript
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
        WallSlide
    }
    #endregion

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float dashingPower = 3f;
    private Animator animator;

    private float xAxis;
    private string currentAnimaton;

    #region jump_related_property

    private bool isJumpPressed;
    public float castDistance = 1.3f;
    private bool grounded;
    [SerializeField, Range(0, 5)] private byte _maxAirJump = 1;
    private byte _airJumpCount;
    [SerializeField] private float jumpHeight = 15f;
    private byte _airState = 0;//0 mean grounded, 1 mean up to air, 2 mean is falling
    private readonly float _coyoteTime = 0.1f;

    #endregion

    #region AtackProperty
    private bool isAttackPressed;
    [SerializeField] private float attackDelay = 0.5f;
    private bool isAttacking;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    [SerializeField] private sbyte attackDamage = 20;
    #endregion
    private bool isFacingRight = true;  // For determining which way the player is currently facing.
    #region Slide/DashProperty
    private bool slidePressed;
    private bool isSliding;
    private bool canDash = true;
    private float dashingTime = 0.3f;
    private float dashingCooldown = 2f;
    private bool _canAirDash = false;
    #endregion
    #region crouch_property
    private bool isCrouching;
    [SerializeField] private float crouchSlowdown = 0.4f;
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

    #region PLAYER_ANIMATION
    //Animation States
    const string PLAYER_IDLE = "Player_idle";
    const string PLAYER_WALK = "Player_walk";
    const string PLAYER_JUMP = "Player_jump";
    const string PLAYER_ATTACK = "Player_attack";
    const string PLAYER_AIR_ATTACK = "Player_air_attack";
    const string PLAYER_CROUCHING = "Player_crouch";
    const string PLAYER_RUN = "Player_run";
    const string PLAYER_SLIDE = "Player_slide";
    const string PLAYER_GETUP_AFTER_SLIDE = "Player_slgup";
    const string PLAYER_WALL_SLIDING = "Player_wallslide";
    const string PLAYER_WALL_JUMP = "Player_walljump";
    const string PLAYER_FALL = "Player_fall";
    #endregion
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isWallJumping) return;
        if (isSliding) return;
        IsGrounded();
        if (grounded)
        {
            _airJumpCount = 0;
            _airState = 0;
        }

        if (rb2d.velocity.y > 0)
        {
            _airState = 1;
        }
        else if (rb2d.velocity.y < 0 && !isWallSliding)
        {
            _airState = 2;
        }
        xAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space)) isJumpPressed = true;

        if (Input.GetKeyDown(KeyCode.J)) isAttackPressed = true;

        if (Input.GetKeyDown(KeyCode.S)) isCrouching = true;
        else if (Input.GetKeyUp(KeyCode.S)) isCrouching = false;

        if (Input.GetKeyDown(KeyCode.L) && canDash)
        {
            StartCoroutine(Dash());
        }
        WallJumping();
        WallSlide();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (IsKnockBack) return;
        if (currentHealth == 0) return;

        if (isSliding) return;

        if (isWallJumping) return;



        Vector2 vel = MovementVelocity();

        if (xAxis > 0 && !isFacingRight) Flip();
        if (xAxis < 0 && isFacingRight) Flip();

        //asign new velocity to the rigid body
        rb2d.velocity = vel;
        //check and perform attack if attacking
        Attack();

    }

    #region jump_related_method
    public bool IsGrounded()
    {
        grounded = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer);
        return grounded;
    }
    #endregion
    #region  wall_jumping_related_method
    private bool IsOnWall()
    {
        var hit = Physics2D.OverlapCircle(wallCheck.position, wallDistance, wallLayer);
        return (hit && !grounded);
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
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
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

                if (grounded)
                {
                    ChangeAnimationState(nameof(PlayerAnimation.Attack));
                    HitDamage();
                }
                else
                {
                    rb2d.AddForce(new Vector2(300, -500));
                    ChangeAnimationState(nameof(PlayerAnimation.AirAttack));
                }
                Invoke(nameof(AttackComplete), attackDelay);
            }
        }
    }
    void AttackComplete()
    {
        isAttacking = false;
    }
    private void HitDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            var baseScript = enemy.GetComponent<BaseCharacterScript>();
            baseScript.TakeDamage(attackDamage);
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

            if (grounded)
                ChangeAnimationState(PLAYER_GETUP_AFTER_SLIDE);
            yield return new WaitForSeconds(0.2f);
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
        if (isJumpPressed && (grounded || _airJumpCount < _maxAirJump))
        {
            _airJumpCount++;
            isJumpPressed = false;
            vel.y = jumpHeight;
            ChangeAnimationState(grounded ? nameof(PlayerAnimation.Jump) : nameof(PlayerAnimation.WallJump));
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
        if (_airState == 2)
        {
            ChangeAnimationState(nameof(PlayerAnimation.Fall));
        }
    }

    
}
