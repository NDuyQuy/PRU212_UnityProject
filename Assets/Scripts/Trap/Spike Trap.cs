using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] protected float thrust = 10f;
    [SerializeField] private sbyte damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            var player = collision.gameObject.GetComponent<BaseCharacterScript>();
            var playerRB2d = collision.gameObject.GetComponent<Rigidbody2D>();
            player.TakeDamage(damage);
            //check if the velocity of the player character to implement the knockback direction
            //velocity > 0 -> going right -> knockback xAxis direction = left
            player.KnockBack(new Vector2((playerRB2d.velocity.x > 0) ? -1 : 1, 1), thrust);
            Debug.Log("hit");
        }
    }
}
