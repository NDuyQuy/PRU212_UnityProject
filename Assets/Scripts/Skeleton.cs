using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
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
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
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
//public class Skeleton : MonoBehaviour
//{
//    [SerializeField]
//    private float speed = 5f;
//    private Rigidbody2D body;
//    private Animator animator;


//    private void Awake()
//    {
//        body = GetComponent<Rigidbody2D>();
//        animator = GetComponent<Animator>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        float moveInput = Input.GetAxis("Horizontal");
//        body.velocity = new Vector2(moveInput * speed, body.velocity.y);
//        if(moveInput == 0)
//        {
//            animator.SetBool("IsWalking", false);
//        }
//        else
//        {
//            animator.SetBool("IsWalking", true);
//        }


//        if (moveInput < 0)
//        {
//            transform.localScale = new Vector3(-1, 1, 1);
//        }
//        else if (moveInput > 0)
//        {
//            transform.localScale = new Vector3(1, 1, 1);
//        }

//        if (Input.GetKey(KeyCode.Space))
//        {
//            body.velocity = new Vector2(body.velocity.x, speed);
//        }

//        if (Input.GetKeyDown(KeyCode.J))
//        {
//            animator.SetTrigger("Attack");
//        }
//    }
//}
