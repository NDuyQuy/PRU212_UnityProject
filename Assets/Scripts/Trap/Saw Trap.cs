using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawTrap : MonoBehaviour
{
    [SerializeField] private float movementDistance;
    [SerializeField] private float movementSpeed;
    
    private bool movingLeft;
    private float leftEdge;
    private float rightEdge;
    [SerializeField] private sbyte damage;
    private void Awake()
    {
        leftEdge = transform
            .position.x - movementDistance;
        rightEdge = transform
            .position.x + movementDistance;
    }

    private void Update()
    {
        if (movingLeft)
        {
            if (transform.position.x > leftEdge)
            {
                transform.position = new Vector3(transform.position.x - movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
            }
            else
            {
                movingLeft = false;
                Vector2 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
            }

        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                transform.position = new Vector3(transform.position.x + movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
            }
            else
                movingLeft = true;
                Vector2 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<BaseCharacterScript>();
        //    var playerRB2d = collision.gameObject.GetComponent<Rigidbody2D>();
            //Give damage for the player character when it hit this trap
            player.TakeDamage(damage);
        }
    }
}
