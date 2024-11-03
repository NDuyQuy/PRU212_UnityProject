using UnityEngine;

public class DoveProjectile : MonoBehaviour
{
    private Rigidbody2D _rb2d;
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _destroyTime = 10f;
    public string AnimationStr;
    private float _timeCounter = 0;
    private Animator _animator;
    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        _animator.Play(AnimationStr);
    }

    public void MoveProjectile(bool rightDirection)
    {
        _rb2d.velocity = rightDirection ?
            Vector2.right * _projectileSpeed :
            Vector2.left * _projectileSpeed;
    }
    private void Update()
    {
        _timeCounter += Time.deltaTime;
        if (_timeCounter > _destroyTime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        var collisionObject = collision2D.gameObject;
        if (collision2D.transform.CompareTag("Player"))
        {
            var playerScript = collisionObject.GetComponent<BaseCharacterScript>();
            playerScript.TakeDamage(5);
        }
        Destroy(gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision2D)
    {
        Destroy(gameObject);
    }
}
