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

public class PlayerScript : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float jumpForce = 850f;
    private Animator animator;

    private float xAxis, yAxis;
    private Rigidbody2D rb2d;
    private bool isJumpPressed;

    public float castDistance;

    private bool grounded;
    private string currentAnimaton;
    private bool isAttackPressed;
    private bool isAttacking;
    private bool isFacingRight = true;  // For determining which way the player is currently facing.

    private bool isCrouching;
    [SerializeField] private float crouchSlowdown = 0.4f;

    public Vector2 boxSize;
    public LayerMask groundLayer;

    [SerializeField]
    private float attackDelay = 0.3f;

    //Animation States
    const string PLAYER_IDLE = "Player_idle";
    const string PLAYER_WALK = "Player_walk";
    const string PLAYER_JUMP = "Player_jump";
    const string PLAYER_ATTACK = "Player_attack";
    const string PLAYER_AIR_ATTACK = "Player_air_attack";
    const string PLAYER_CROUCHING = "Player_crouching";

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        xAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space)) isJumpPressed = true;

        if (Input.GetKeyDown(KeyCode.J)) isAttackPressed = true;

        if (Input.GetKeyDown(KeyCode.S)) isCrouching = true;
        else if (Input.GetKeyUp(KeyCode.S)) isCrouching = false;
    }

    void FixedUpdate()
    {

        Vector2 vel = new Vector2(0, rb2d.velocity.y);

        if (xAxis < 0)
        {
            vel.x = (isCrouching) ? (-walkSpeed * crouchSlowdown): -walkSpeed ;
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

        if (grounded && !isAttacking)
        {
            if (xAxis != 0)
            {
                ChangeAnimationState(PLAYER_WALK);
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

        if (isCrouching && xAxis != 0)
        {
            ChangeAnimationState(PLAYER_WALK);
        }
    }

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

    //private void OnCollisionEnter2D(Collision2D other)
    //{
    //    if (other.gameObject.CompareTag("Ground"))
    //    {
    //        grounded = true;
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D other)
    //{
    //    if (other.gameObject.CompareTag("Ground"))
    //    {
    //        grounded = false;
    //    }
    //}
}
