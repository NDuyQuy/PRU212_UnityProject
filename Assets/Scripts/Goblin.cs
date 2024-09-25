using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;
    [SerializeField]
    private float attackInterval = 2f; // Thời gian giữa các đợt tấn công
    [SerializeField]
    private float leftLimit = -2f; // Giới hạn bên trái
    [SerializeField]
    private float rightLimit = 2f; // Giới hạn bên phải

    private Rigidbody2D body;
    private Animator animator;

    private bool movingRight = true;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // Di chuyển qua lại
        if (movingRight)
        {
            body.velocity = new Vector2(speed, body.velocity.y);
            if (transform.position.x >= rightLimit)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            body.velocity = new Vector2(-speed, body.velocity.y);
            if (transform.position.x <= leftLimit)
            {
                movingRight = true;
                Flip();
            }
        }

        animator.SetBool("IsWalking", true);
    }

    private void Flip()
    {
        transform.localScale = new Vector3(movingRight ? 2 : -2, 2, 2);
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(1f);
            animator.SetTrigger("Attack_2");
        }
    }
}
