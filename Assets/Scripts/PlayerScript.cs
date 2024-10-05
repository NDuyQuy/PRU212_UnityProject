using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : BaseCharacterScript
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float dashingPower = 3f;

    private Animator animator;

    private float xAxis, yAxis;
    private string currentAnimaton;
    
    #region jump_related_property

    private bool isJumpPressed;
    public float castDistance = 1.3f;
    private bool grounded;
    [SerializeField, Range(0, 5)] private byte _totalJumpAvailable = 2;
    private byte _jumpCount;
    [SerializeField] private float jumpForce = 750f;
    [SerializeField] private float doubleJumpForce = 600f;
    [SerializeField] private float gravityForce = 10f;
    private byte _airState = 0;//0 mean grounded, 1 mean up to air, 2 mean is falling
    
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
    #region SlideProperty
    private bool slidePressed;
    private bool isSliding;
    private bool canDash = true;
    private float dashingTime = 0.3f;
    private float dashingCooldown = 2f;
    #endregion

    private TrailRenderer tr;

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
        tr = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isWallJumping) return;
        if (isSliding) return;
        IsGrounded();
        if (grounded)
        {
            _jumpCount = 0;
            _airState = 0;
        }

        if (rb2d.velocity.y > 0)
        {
            _airState = 1;
        }
        else if (rb2d.velocity.y < 0)
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
        if (_airState != 0 ) gravityForce += Mathf.Abs(rb2d.mass * rb2d.velocity.y);
        else gravityForce = 0;
    }

    void FixedUpdate()
    {
        if(currentHealth==0) return;

        if (isSliding) return;

        if (isWallJumping) return;
        
        if (isWallSliding)
        {
            ChangeAnimationState(PLAYER_WALL_SLIDING);
        }

        Vector2 vel = new Vector2(0, rb2d.velocity.y);

        if (xAxis != 0)
        {
            vel.x = (isCrouching) ? (xAxis * walkSpeed * crouchSlowdown) : (xAxis * walkSpeed);
        }
        else
        {
            vel.x = 0;
        }

        if (xAxis > 0 && !isFacingRight) Flip();
        if (xAxis < 0 && isFacingRight) Flip();

        if (grounded && !isAttacking && !isSliding)
        {
            if (xAxis != 0)
            {
                if (isCrouching) ChangeAnimationState(PLAYER_CROUCHING);
                else
                {
                    if (Mathf.Abs(walkSpeed) > 9) ChangeAnimationState(PLAYER_RUN);
                    else ChangeAnimationState(PLAYER_WALK);
                }
            }
            else
            {
                ChangeAnimationState(PLAYER_IDLE);
            }
        }

        //call this function if perform jumping
        Jump();


        if (slidePressed && grounded)
        {
            slidePressed = false;
            ChangeAnimationState(PLAYER_SLIDE);
            isSliding = true;
            Invoke("SlideComplete", 0.3f);
        }

        //asign new velocity to the rigid body
        rb2d.velocity = vel;
        //check if attacking
        if (isAttackPressed)
        {
            isAttackPressed = false;

            if (!isAttacking)
            {
                isAttacking = true;

                if (grounded)
                {
                    ChangeAnimationState(PLAYER_ATTACK);
                    Attack();
                }
                else
                {
                    rb2d.AddForce(new Vector2(100,-500));
                    ChangeAnimationState(PLAYER_AIR_ATTACK);
                }

                Invoke("AttackComplete", attackDelay);

            }
        }

    }

    #region jump_related_method
    private void Jump()
    {
        if (isJumpPressed && (grounded || _jumpCount < _totalJumpAvailable))
        {
            var jumpForceApply = (grounded) ? jumpForce : doubleJumpForce;
            _jumpCount++;
             
            if (_airState == 2)
            {               
                jumpForceApply += gravityForce;
                Debug.Log(jumpForceApply);
            }
            rb2d.AddForce(Vector2.up*jumpForceApply);
            isJumpPressed = false;

            //the wall jump just has roll up and fall animation so we can reuse it
            if (!grounded) ChangeAnimationState(PLAYER_WALL_JUMP);
        }
    }
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
            ChangeAnimationState(PLAYER_WALL_JUMP);

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

    void AttackComplete()
    {
        isAttacking = false;
    }
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
        canDash = false;
        isSliding = true;
        float originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0f;
        rb2d.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        if (grounded)
            ChangeAnimationState(PLAYER_SLIDE);
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

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            var baseScript = enemy.GetComponent<BaseCharacterScript>();
            baseScript.TakeDamage(attackDamage);
            Vector2 knockBackDir;
            if (isFacingRight)
            {
                knockBackDir = new Vector2(2, 1);
            }
            else knockBackDir = new Vector2(-2, 1);
            baseScript.KnockBack(knockBackDir, 5f);
        }
    }
}
