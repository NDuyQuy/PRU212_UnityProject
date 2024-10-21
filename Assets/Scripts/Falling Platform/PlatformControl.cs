using UnityEngine;
using System.Collections;

public class PlatformControl : MonoBehaviour
{
    private readonly float fallDelay = 1f;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] protected Rigidbody2D rb2d;
    [SerializeField] protected Collider2D col2d;
    [SerializeField] protected float _gravity = 10f;
    [SerializeField] protected float _mass = 1f;
    [SerializeField] protected RigidbodyType2D _rbType;//rigid body type
    [SerializeField] protected bool _enableColider = true;
    protected bool Updated;//check if updated?
    protected bool PlayerTouched;//check if has player touched platform?
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerTouched = true;
        }
    }
    protected virtual void Awake()
    {
        //if not assign then get from component
        rb2d = rb2d != null ? rb2d : GetComponent<Rigidbody2D>();
        col2d = col2d != null ? col2d : GetComponent<Collider2D>();

        rb2d.bodyType = _rbType;
        rb2d.gravityScale = _gravity;
        rb2d.mass = _mass;
    }

    protected virtual void Update()
    {
        if (Updated)
        {
            Updated = false;
            UpdateAll();
        }
    }
    protected void Moving(float xVel, float yVel)
    {
        rb2d.velocity = new Vector2(xVel, yVel);
    }
    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.gravityScale = _gravity;
        Destroy(gameObject, destroyDelay);
    }

    private void UpdateAll()
    {
        //rigid body
        rb2d.mass = _mass;
        rb2d.gravityScale = _gravity;
        rb2d.bodyType = _rbType;
        //colider
        col2d.enabled = _enableColider;
    }
}