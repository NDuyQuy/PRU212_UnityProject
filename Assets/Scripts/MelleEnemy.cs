using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MelleEnemy : BaseCharacterScript
{
    [SerializeField]
    private float speed = 2f;
    [SerializeField]
    private LayerMask playerLayerMask;
    [SerializeField]
    private float viewRange = 7f;

    [SerializeField]
    private Transform leftRange;
    [SerializeField]
    private Transform rightRange;

    [SerializeField]
    private int meleeDamage;
    [SerializeField]
    private float meleeRange;
    [SerializeField]
    private Transform meleePoint;

    [SerializeField]
    private Coins coinPrefab;

    private Animator animator;
    private bool movingRight = true;
    private bool isSeePlayer = false;
    private bool isAttacking = false;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            TrackPlayer();
            if (!isSeePlayer)
            {
                Move();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
    }

    public void TraceMelleDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(meleePoint.position, meleeRange, playerLayerMask);
        if (hit != null)
        {
            var playerBaseScripts = hit.GetComponent<BaseCharacterScript>();
            playerBaseScripts.TakeDamage((sbyte)meleeDamage);
        }
        else
        {
            Debug.Log("Don't hit player");
        }
    }


    private void TrackPlayer()
    {
        Collider2D hit = TrackPlayerWithRaycast();
        isSeePlayer = hit != null;

        if (isSeePlayer && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void Move()
    {
        float moveDistance = speed * Time.fixedDeltaTime; // Calculate movement distance for each frame

        if (movingRight)
        {
            transform.Translate(Vector2.right * moveDistance);
            if (transform.position.x >= rightRange.position.x)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * moveDistance);
            if (transform.position.x <= leftRange.position.x)
            {
                movingRight = true;
                Flip();
            }
        }

        animator.SetBool("IsWalking", true);
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        OnFlip?.Invoke();
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetBool("IsWalking", false); // Stop walking animation

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }

    private Collider2D TrackPlayerWithRaycast()
    {
        Vector2 direction = GetTrackDirection();

        Debug.DrawLine(transform.position, transform.position + (Vector3)direction * viewRange, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, viewRange, playerLayerMask);
        return hit.collider;
    }

    private Vector2 GetTrackDirection()
    {
        var scaleX = transform.localScale.x;
        Vector2 direction = new Vector2(scaleX, 0).normalized;
        return direction;
    }

    protected override void Die(float delayTime = 0)
    {
        Coins coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        base.Die(delayTime);
    }
}
