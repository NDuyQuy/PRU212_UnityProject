using UnityEngine;

public class LoopRunEnemy : BaseCharacterScript
{
    [SerializeField]
    private float moveSpeed = 4f;
    [SerializeField]
    private float patrolDelay = 2f; 
    [SerializeField]
    private Transform leftLimitPatrolPoint, rightLimitPatrolPoint;

    [SerializeField]
    private int damageTouchPlayer = 10;
    [SerializeField]
    private LayerMask playerLayer;

    [SerializeField]
    private Coins coinPrefab;

    [SerializeField]
    private Coins.CoinsValue coinValue;

    private Animator animator;
    private Vector2 targetMovePosition;
    private float idleTimer;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        targetMovePosition = transform.position;
    }

    void FixedUpdate()
    {
        HandlePatrol();
    }

    private void HandlePatrol()
    {
        idleTimer += Time.fixedDeltaTime;
        if (idleTimer >= patrolDelay)
        {
            bool isMoveToTargetPos = Move(targetMovePosition, moveSpeed);
            animator.SetBool("IsRunning", true);

            if (isMoveToTargetPos)
            {
                idleTimer = 0;
            }
        }
        else
        {
            animator.SetBool("IsRunning", false);
            float xRandom = Random.Range(leftLimitPatrolPoint.position.x, rightLimitPatrolPoint.position.x);
            targetMovePosition.x = xRandom;
        }
    }

    private bool Move(Vector2 position, float moveSpeed)
    {
        if (Vector2.Distance((Vector2)transform.position, position) <= 0.2f) return true;
        Vector2 direction = (position - (Vector2)transform.position).normalized;

        transform.position = Vector2.MoveTowards(transform.position, position, moveSpeed * Time.fixedDeltaTime);

        if (direction.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
        return false;
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        OnFlip?.Invoke();
    }

    protected override void Die(float delayTime = 0)
    {
        Coins coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        coin.SetCoinValue(coinValue);
        base.Die(delayTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer & (1 << collision.gameObject.layer)) != 0)
        {
            BaseCharacterScript player = collision.GetComponent<BaseCharacterScript>();
            if (player != null)
            {
                player.TakeDamage((sbyte)damageTouchPlayer);
            }
        }
    }
}
