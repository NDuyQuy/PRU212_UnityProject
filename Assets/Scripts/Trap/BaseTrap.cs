using UnityEngine;

public class BaseTrap : MonoBehaviour
{
    [SerializeField] protected sbyte damage;

    //Collision Damage
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            collision.GetComponent<BaseCharacterScript>().TakeDamage(damage);
    }
}
