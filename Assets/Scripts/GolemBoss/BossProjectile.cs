using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f;

    [SerializeField]
    private Projectile projectile;

    private Vector2 moveDirection = Vector2.zero;

    public void SetGolemBoss(GolemBoss golemBoss)
    {
        projectile.SetGolemBoss(golemBoss);
    }

    public void SetPosition(Vector2 position) => transform.position = position;
    public void SetMoveDirection(Vector2 moveDirection)
    {
        if(moveDirection.x < 0)
        {
            transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        this.moveDirection = moveDirection.normalized;
    }

    public void FixedUpdate()
    {
        if (moveDirection == Vector2.zero) return;
        transform.Translate(moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
}
