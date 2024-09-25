using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum
//{
//    PLAYER_IDLE = "Player_idle";
//    PLAYER_RUN = "Player_run";
//    PLAYER_JUMP = "Player_jump";
//    PLAY_ATTACK = "Player_attack";
//    PLAYER_AIR_ATTACK = "Player_air_attack";
//}PLAYER_ANIMATIONS

public class PlayerScript : BaseCharacterScript
{
    [SerializeField]private float walkSpeed = 5f;
    [SerializeField]private float jumpForce = 750f;
    [SerializeField]private float dashingPower = 3f;

    private Animator animator;

    private float xAxis, yAxis;
    private bool isJumpPressed;

    public float castDistance;

    private bool grounded;
    private string currentAnimaton;
    private bool isAttackPressed;
    private bool isAttacking;
    private bool isFacingRight = true;  // For determining which way the player is currently facing.
    private bool slidePressed;

    private bool isSliding;
    private bool canDash = true;
    private float dashingTime = 0.3f;
    private float dashingCooldown = 2f;

    private TrailRenderer tr;

    private bool isCrouching;

    [SerializeField] private float crouchSlowdown = 0.4f;

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
    [SerializeField]private Vector2 wallJumpingPower = new Vector2(3f,7f);
    #endregion

    [SerializeField]private float attackDelay = 0.5f;

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
        if(isWallJumping) return;
        if(isSliding) return;
        CheckGrounded();
        xAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space)) isJumpPressed = true;

        if (Input.GetKeyDown(KeyCode.J)) isAttackPressed = true;

        if (Input.GetKeyDown(KeyCode.S)) isCrouching = true;
        else if (Input.GetKeyUp(KeyCode.S)) isCrouching = false;

        if(Input.GetKeyDown(KeyCode.L) && canDash) 
        {
            StartCoroutine(Dash());
        }
        WallSlide();    
        WallJumping();
    }

    void FixedUpdate()
    {
        if(isSliding) return;
        if(isWallJumping) return;
        if(isWallSliding)
        {
            ChangeAnimationState(PLAYER_WALL_SLIDING);
        }
        Vector2 vel = new Vector2(0, rb2d.velocity.y);
        
        if (xAxis < 0)
        {
            vel.x = (isCrouching) ? (-walkSpeed * crouchSlowdown): - walkSpeed ;
        }
        else if (xAxis > 0)
        {
            vel.x = (isCrouching) ? walkSpeed * crouchSlowdown:walkSpeed;
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
                if(isCrouching) ChangeAnimationState(PLAYER_CROUCHING);
                else 
                {
                    if(Mathf.Abs(walkSpeed)>9) ChangeAnimationState(PLAYER_RUN);
                    else ChangeAnimationState(PLAYER_WALK);
                }
            }
            else
            {
                ChangeAnimationState(PLAYER_IDLE);
            }
        }



        //check if trying jump
        if (isJumpPressed && grounded)
        {
            rb2d.AddForce(new Vector2(0, jumpForce));
            isJumpPressed = false;
            ChangeAnimationState(PLAYER_JUMP);
        }

        if(slidePressed && grounded)
        {
            slidePressed = false;
            ChangeAnimationState(PLAYER_SLIDE);
            isSliding = true;
            Invoke("SlideComplete",0.3f);
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
                }
                else
                {
                    ChangeAnimationState(PLAYER_AIR_ATTACK);
                }

                Invoke("AttackComplete", attackDelay);

            }
        }
        
    }

    #region  wall_jumping_related_method
    private bool IsOnWall()
    {
        var hit = Physics2D.OverlapCircle(wallCheck.position,wallDistance, wallLayer);
        return (hit && !grounded);
    }
    private void WallSlide()
    {
        if(IsOnWall() && xAxis != 0f) 
        {
            isWallSliding = true;
            rb2d.velocity = new Vector2(rb2d.velocity.x,Mathf.Clamp(rb2d.velocity.y,-wallSlidingSpeed,float.MaxValue));
        }
        else isWallSliding = false;
    }
    private void WallJumping()
    {
        if(isWallSliding)
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

        if(Input.GetButtonDown("Jump")&&wallJumpingCounter>0f)
        {
            isWallJumping = true;
            rb2d.velocity = new Vector2(WallJumpingDirection*wallJumpingPower.x,wallJumpingPower.y);
            wallJumpingCounter = 0;
            Flip();
            ChangeAnimationState(PLAYER_WALL_JUMP);

            Invoke(nameof(StopWallJumping),wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

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

    public bool CheckGrounded()
    {
        var hit = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer);

        if (hit) grounded = true;
        else grounded = false;

        return grounded;
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
        rb2d.velocity = new Vector2(transform.localScale.x*dashingPower,0f);
        if(grounded)
            ChangeAnimationState(PLAYER_SLIDE);
        else ChangeAnimationState("");

        yield return new WaitForSeconds(dashingTime);

        if(grounded)
            ChangeAnimationState(PLAYER_GETUP_AFTER_SLIDE);
        yield return new WaitForSeconds(0.2f);
        rb2d.gravityScale = originalGravity;
        isSliding = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
