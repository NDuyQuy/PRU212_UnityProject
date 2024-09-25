using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterScript : MonoBehaviour
{
    public sbyte maxHealth = 100;
    public sbyte currentHealth;

    private bool isInvincible = false;
    public float invincibilityDuration = 1f;

    protected Rigidbody2D rb2d;
    protected Vector2 boxSize = new Vector2(1.8f,0.2f);

    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(sbyte dmg)
    {
        currentHealth -= dmg;
        if(currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void CheckHit()
    {
        //implement later for detecting being hit
    }

    public virtual void Attack()
    {
        //implement later for detecting hit
    }

    public virtual void KnockBack()
    {

    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }


}
