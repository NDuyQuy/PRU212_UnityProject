using UnityEngine;

public class BaseTrap : MonoBehaviour
{
    [SerializeField] protected float thrust = 10f;
    [SerializeField] private sbyte damage;
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<BaseCharacterScript>();
            var playerRB2d = collision.gameObject.GetComponent<Rigidbody2D>();
            player.TakeDamage(damage);
            //check if the velocity of the player character to implement the knockback direction
            //velocity > 0 -> going right -> knockback xAxis direction = left
            player.KnockBack(new Vector2((playerRB2d.velocity.x > 0) ? -1 : 3, 1), thrust);
        }
    }
}
