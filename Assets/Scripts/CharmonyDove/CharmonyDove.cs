using System.Collections;
using UnityEngine;

public class CharmonyDove : BaseCharacterScript
{
    private enum DoveState
    {
        Normal,
        Angry,
        AngryWithGun
    }

    private DoveState _currentState;

    public GameObject Bullet;
    public GameObject Wave;
    public GameObject Ball;
    private Transform _gunPoint;

    private bool _hittedOnce;
    private bool _attackPhase;
    private bool _isAttacking;
    private GameObject _angryMask;
    private GameObject _dialog;

    private bool _isFacingRight;
    private readonly string SPRITES_PATH = "Sprites/CharmonyDove/";
    private readonly string AUDIOS_PATH = "Audios/";
    private int _attackType = 0;
    private AudioClip _audioClip;
    private AudioSource _audio;
    private Transform _player;
    private Vector2 _initialPosition;
    private CircleCollider2D _circleCollider2d;
    public float moveSpeed = 2f;
    public float attackSpeed = 10f;
    public float chargeDistance = 5f;
    public float hoverHeight = 10f;
    public sbyte damage = 10;
    public float stuckDelay = 1f;
    public float attackDelay = 1f;
    private float _attackTimeCounter;
    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _circleCollider2d = GetComponent<CircleCollider2D>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _gunPoint = transform.Find("GunPoint");
        _currentState = DoveState.Normal;
        _angryMask = transform.Find("AngryMask").gameObject;
        _dialog = transform.Find("Dialog").gameObject;
        _angryMask.SetActive(_hittedOnce);
        _dialog.SetActive(!_hittedOnce);
    }

    protected override void Start()
    {
        base.Start();
        _initialPosition = rb2d.position;
    }
    private void Update()
    {
        if(_player==null) return;
        if (IsDead && !_deadCalled)
        {
            Die();
        }
        if (_deadCalled) return;

        if (!_isAttacking && _currentState != DoveState.Normal)
        {
            _attackTimeCounter += Time.deltaTime;
        }
        if (_hittedOnce && !_attackPhase)
        {
            _attackPhase = true;
            _currentState = DoveState.Angry;
            ChangePhase();
        }

        if (_currentState != DoveState.AngryWithGun && currentHealth < 50)
        {
            _currentState = DoveState.AngryWithGun;
            ChangePhase();
        }

        FaceTowardPlayer();
        HitPlayer();
    }

    private void FixedUpdate()
    {
        if(_player==null) return;
        if (_deadCalled) return;
        HandleMovement();
        PerformAttackMove();
    }

    private void HandleMovement()
    {
        switch (_currentState)
        {
            case DoveState.Normal:
                FollowPlayer();
                break;
            case DoveState.Angry:
            case DoveState.AngryWithGun:
                if (!_isAttacking)
                {
                    KeepDistanceWithPlayer();
                    break;
                }
                return;
        }
    }

    private void FollowPlayer()
    {
        float distance = Vector2.Distance(rb2d.position, _player.position);

        if (distance > chargeDistance)
        {
            Vector2 direction = (_player.position - (Vector3)rb2d.position).normalized;
            rb2d.velocity = direction * moveSpeed;
        }
        else
        {
            rb2d.velocity = Vector2.zero;  // Stop if within close range
        }
    }

    private void KeepDistanceWithPlayer()
    {
        float targetDistanceX = 12f;
        float tolerance = 0.5f;  // Tolerance range to avoid jittering
        Vector2 playerPosition = (Vector2)_player.position;
        Vector2 bossPosition = rb2d.position;

        float currentDistanceX = Mathf.Abs(bossPosition.x - playerPosition.x);

        // Set the target y position slightly above the player
        float targetY = playerPosition.y + 1.5f;

        // Check if the current distance on x-axis is within the tolerance range
        if (Mathf.Abs(currentDistanceX - targetDistanceX) < tolerance)
        {
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);  // Stop x movement within tolerance range
        }
        else
        {
            // Determine direction to move only on the x-axis
            Vector2 finalDir = (currentDistanceX < targetDistanceX) ?
                Vector2.left :
                Vector2.right;
            rb2d.velocity = new Vector2(finalDir.x * moveSpeed, rb2d.velocity.y);
        }

        // Smoothly adjust the y position to stay above the player
        rb2d.position = new Vector2(rb2d.position.x, Mathf.Lerp(bossPosition.y, targetY, Time.deltaTime * 5f));
    }



    private void PerformAttackMove()
    {
        if (!(_attackTimeCounter > attackDelay))
            return;
        _attackTimeCounter = 0;
        int range = (_currentState == DoveState.Angry) ? 3 : 4;
        _attackType++;
        if (_attackType >= range)
            _attackType = 0;
        switch (_attackType)
        {
            case 0:
                StartCoroutine(ChargeAttack());
                break;
            case 1:
                StartCoroutine(DiveAttack());
                break;
            case 2:
                StartCoroutine(CreateBall());
                break;
            case 3:
                StartCoroutine(ShootProjectile());
                break;
            default: break;
        }
    }

    private IEnumerator ShootProjectile()
    {
        _isAttacking = true;
        PlayAudio("gun");
        for (int i = 0; i < 4; i++)
        {
            var bulletScript = Instantiate(Bullet, _gunPoint.position, Quaternion.identity)
                                .GetComponent<DoveProjectile>();
            bulletScript.MoveProjectile(rightDirection: !_isFacingRight);
            yield return new WaitForSeconds(0.2f);
        }
        _isAttacking = false;
    }
    private IEnumerator ChargeAttack()
    {
        PlayAudio("angry");
        _isAttacking = true;
        rb2d.velocity = Vector2.up * attackSpeed;
        yield return new WaitForSeconds(0.3f);
        Vector2 chargeDirection = (Vector2)(_player.position - transform.position).normalized;
        rb2d.velocity = chargeDirection * attackSpeed;
    }

    private IEnumerator DiveAttack()
    {
        // Move above the player
        PlayAudio("angry");
        _isAttacking = true;
        Vector2 hoverPosition = new(_player.position.x, _player.position.y + hoverHeight);
        transform.position = (Vector3)hoverPosition;
        // Wait for 1 sec
        yield return new WaitForSeconds(0.5f);
        // Drop down
        rb2d.gravityScale = 20f;
    }

    private void PlayAudio(string audio)
    {
        _audioClip = Resources.Load<AudioClip>(AUDIOS_PATH + audio);
        _audio.clip = _audioClip;
        _audio.Play();
    }

    protected override void CheckHit()
    {
        base.CheckHit();
        if (!_hittedOnce) _hittedOnce = true;
    }

    private void ChangePhase()
    {
        switch (_currentState)
        {
            case DoveState.Angry:
                rb2d.excludeLayers = LayerMask.GetMask("Player");
                break;
            case DoveState.AngryWithGun:
                Sprite angryDove = Resources.Load<Sprite>(SPRITES_PATH + DoveState.Angry.ToString().ToLower());
                spriteRenderer.sprite = angryDove;
                stuckDelay = 0.5f;
                attackDelay = 0.5f;
                break;
        }
        _audio.Stop();
        _audio.loop = false;
        PlayAudio("Angry");
        _dialog.SetActive(false);
        _angryMask.SetActive(true);
    }

    private void Attack() => _isAttacking = true;

    private void FaceTowardPlayer()
    {
        //xDir is a fck random name tho so dont think much about it 
        //purpose is cal the current dove if it belong to the left or the right side with the player
        //xDir > 0 -> its self is on the right else on the left
        var xDir = transform.position.x - _player.position.x;
        if (xDir > 0 && !_isFacingRight)
            Flip();
        if (xDir < 0 && _isFacingRight)
            Flip();
    }
    private void Flip()
    {
        _isFacingRight = !_isFacingRight;

        Vector2 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private IEnumerator CreateBall()
    {
        _isAttacking = true;
        Instantiate(Ball, transform.position + (Vector3)Vector2.right * 10
                                , Quaternion.identity, transform);
        yield return new WaitForSeconds(0.2f);
        Instantiate(Ball, transform.position + (Vector3)Vector2.left * 10
                                , Quaternion.identity, transform);
        yield return new WaitForSeconds(0.2f);
        Instantiate(Ball, transform.position + (Vector3)Vector2.up * 10
                                , Quaternion.identity, transform);
        yield return new WaitForSeconds(0.2f);
        Instantiate(Ball, transform.position + (Vector3)Vector2.down * 10
                                , Quaternion.identity, transform);

        _isAttacking = false;

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_currentState == DoveState.Normal)
            return;
        if (_isAttacking)
        {
            rb2d.gravityScale = 0;
            rb2d.velocity = Vector2.zero;
            Invoke(nameof(StopAttack), 2f);//return normal after 2 sec
        }

        if (collision.gameObject.CompareTag("Ground")
            && _currentState == DoveState.AngryWithGun
            && _attackType == 1
            )
        {
            ContactPoint2D contactPoint = collision.GetContact(0);
            var waveObject = Instantiate(Wave, contactPoint.point, Quaternion.identity);
            waveObject.GetComponent<DoveProjectile>().MoveProjectile(rightDirection: !_isFacingRight);
            if (_isFacingRight)
            {
                Vector2 newWaveScale = (Vector2)waveObject.transform.localScale * new Vector2(-1, 1);
                waveObject.transform.localScale = newWaveScale;
            }

        }
    }

    private void HitPlayer()
    {
        if (_currentState == DoveState.Normal)
            return;
        Vector2 overlapCircleCenter = (Vector2)transform.position + _circleCollider2d.offset;
        float overlapRadius = _circleCollider2d.radius;
        Collider2D hitPlayer =
            Physics2D.OverlapCircle(overlapCircleCenter, overlapRadius, LayerMask.GetMask("Player"));
        if (hitPlayer != null)
        {
            var playerScript = _player.GetComponent<BaseCharacterScript>();
            playerScript.TakeDamage(damage);
        }
    }
    private void StopAttack() => _isAttacking = false;

    protected override void Die(float delayTime = 2f)
    {
        _deadCalled = true;
        rb2d.excludeLayers = LayerMask.GetMask("Ground");
        rb2d.velocity = Vector2.zero;
        var dieScale = (Vector2)transform.localScale * new Vector2(1, -1);
        transform.localScale = dieScale;
        rb2d.gravityScale = 1f;
        base.Die(delayTime);
    }

    private bool IsDead => currentHealth <= 0;
    private bool _deadCalled;
}
