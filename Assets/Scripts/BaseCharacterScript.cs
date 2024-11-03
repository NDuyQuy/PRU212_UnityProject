using System;
using System.Collections;
using UnityEngine;

public class BaseCharacterScript : MonoBehaviour
{
    public sbyte maxHealth = 100;
    public sbyte currentHealth;

    private bool isInvincible;
    public float invincibilityDuration = 5f;
    public float blinkInterval = 0.1f;

    protected Rigidbody2D rb2d;
    protected SpriteRenderer spriteRenderer;

    public EventHandler<HeathUpdateEventArgs> OnHealthUpdate;
    public Action OnFlip;

    public class HeathUpdateEventArgs: EventArgs
    {
        public float HealthPersent;
    }

    [SerializeField]protected Vector2 boxSize;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxSize = new(1f,0.2f);
        currentHealth = maxHealth;
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void TakeDamage(sbyte dmg)
    {
        if(isInvincible) return;
        currentHealth -= dmg;

        OnHealthUpdate?.Invoke(this, new HeathUpdateEventArgs
        {
            HealthPersent = (float)currentHealth/maxHealth
        });

        if(currentHealth <= 0) Die();
        CheckHit();
        StartCoroutine(InvincibilityCoroutine());
    }

    protected virtual void Die(float delayTime=0)
    {
        StartCoroutine(DestroyAfterDelay(delayTime));
    }
    private IEnumerator DestroyAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Destroy(gameObject);
    } 

    protected virtual void CheckHit()
    {
        StartCoroutine(BlinkEffect());
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

    private IEnumerator BlinkEffect()
    {
        float elapsedTime = 0f;
        while (elapsedTime < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle the sprite visibility
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }
        spriteRenderer.enabled = true;
    }

    public void AddHearth(sbyte value)
    {
        currentHealth += value;
        currentHealth = (sbyte)Mathf.Min(currentHealth+value, maxHealth);
    }
}
