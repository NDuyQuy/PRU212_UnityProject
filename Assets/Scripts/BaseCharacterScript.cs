using System.Collections;
using UnityEngine;

public class BaseCharacterScript : MonoBehaviour
{
    public sbyte maxHealth = 100;
    public sbyte currentHealth;

    private bool isInvincible;
    public float invincibilityDuration = 5f;

    protected Rigidbody2D rb2d;
    protected Vector2 boxSize = new Vector2(1.8f,0.2f);
    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb2d = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(sbyte dmg)
    {
        if(isInvincible) return;
        currentHealth -= dmg;
        if(currentHealth <= 0) Die();
        StartCoroutine(InvincibilityCoroutine());
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void CheckHit()
    {
        //implement later for detecting being hit
    }

    public virtual void KnockBack(Vector2 knockBackDirection, float knockbackForce)
    {
        rb2d.AddForce(knockBackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }


}
