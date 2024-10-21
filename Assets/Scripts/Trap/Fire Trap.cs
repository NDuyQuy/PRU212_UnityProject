using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [Header("Firetrap Timers")]
    [SerializeField] private sbyte damage;
    [SerializeField] private sbyte activationDelay;
    [SerializeField] private sbyte activationTime;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool triggered; //when the trap gets trigger
    private bool active; //when the trap is active and can hurt the player

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if (!triggered)
            {
                StartCoroutine(ActivateFireTrap());
            }
            if (active)
            {
                collision.GetComponent<BaseCharacterScript>().TakeDamage(damage);
            }
        }
    }
    private IEnumerator ActivateFireTrap()
    {
        //turn the sprite red to notify the player and trigger the trap

        triggered = true;
        spriteRenderer.color = Color.red;  

        //wait for delay, activatte trap, turn on animation, return color back to normal
        yield return new WaitForSeconds(activationDelay);
        spriteRenderer.color = Color.white;  //turn the sprite back to its initial color
        active = true;
        animator.SetBool("activated", true);

        //wait until X seconds, deactivate trap and reset all variables and animator
        yield return new WaitForSeconds(activationTime);
        active = false;
        triggered = false;
        animator.SetBool("activated", false);
    }
}
