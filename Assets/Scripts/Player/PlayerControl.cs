using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayerControl : BaseCharacterScript
{
    #region Player_Animation_Enum
    public enum PlayerAnimation
    {
        Jump,
        Attack,
        AirAttack,
        Slide,
        WallJump,
        Attack1,
        Attack2,
        Attack3,
        Idle,
        Fall,
        Walk,
        Run,
        Crouch,
        CrouchWalk2,
        WallSlide,
        Kick,
        Archery,
        AirArchery,
        SAttack1,
        SAttack2,
        SAttack3,
        Die,
        SuperDash
    }
    #endregion

    #region game component
    private GameObject _currentArrow;
    private Effect _bgEffects;
    private Transform _arrowStart;
    private PolygonCollider2D _plCol2d;
    private BoxCollider2D _boxCol2d;
    public LayerMask groundLayer;
    [SerializeField] private GameObject _arrow;
    private Animator _animator;

    #endregion

    private float _originalGravity;

    private BaseCharacterScript baseCharacterScript;
    private Vector3 _originalPosition;
    private float _outOfCameraTime = 1f; // Time to wait before returning
    private Coroutine _returnCoroutine;

    public int Currency { get; set; } = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        _boxCol2d = GetComponent<BoxCollider2D>();
        _plCol2d = GetComponent<PolygonCollider2D>();
        _audio = GetComponent<AudioSource>();
        _orgnBoxColSize = _boxCol2d.size;
        _arrowStart = transform.Find("ArrowPoint");
        _bgEffects = transform.Find("BgEffect").gameObject.GetComponent<Effect>();
        _bgEffects.Enable = false;
        baseCharacterScript = GetComponent<BaseCharacterScript>();
        _originalPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject == null) return;
        if (IsDead)
        {
            Die();
            return;
        }

        if (_isSuperDashing)
        {
            if (AnyKeyPressed) StopSuperDash();
            return;
        }

        if (isWallJumping) return;
        if (isDash) return;

        IsGrounded();
        if (grounded)
        {
            _airJumpCount = 0;
            _airState = 0;
            _isAirKick = false;
        }

        //update air state 1->jump up 2-> falling
        if (rb2d.velocity.y > 0)
        {
            _airState = 1;
        }
        else if (rb2d.velocity.y < 0 && !isWallSliding && !grounded)
        {
            _airState = 2;
        }

        xAxis = Input.GetAxisRaw("Horizontal");

        if (JumpPressed) isJumpPressed = true;
        if (MainAttackPressed) isAttackPressed = true;
        if (CrouchPressed) isCrouching = true;
        else if (CrounchReleased)
        {
            if (!HasCeiling) isCrouching = false;
        }
        if (SideAttackPressed) _isSideAttackPressed = true;
        if (DashPressed)
        {
            _dashPressed = true;
            PlayAudio(Audios.holding, loop: true);
        }
        else if (DashReleased)
        {
            _dashPressed = false;
            StopAudio();
            PerformDash();
            _bgEffects.Enable = false;
            _dashTiming = 0;
        }
        if (_dashPressed)
        {
            _dashTiming += Time.deltaTime;
            _bgEffects.Enable = true;
            _bgEffects.EffectAnimation = _dashTiming > _superDashCpltInterval ? "holding2" : "holding1";
        }
        WallJumping();
        WallSlide();
        Crouch();
        UpdateAnimation();
        AirKick();

        DetectMovingPlatform();
        CheckOutOfCameraBounds();
    }
    private void CheckOutOfCameraBounds()
    {
        if (IsOutOfCamera())
        {
            if (_returnCoroutine == null)
            {
                _returnCoroutine = StartCoroutine(ReturnToOriginalPosition());
            }
        }
        else
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }
    }

    private bool IsOutOfCamera()
    {
        Camera camera = Camera.main;
        Vector3 screenPoint = camera.WorldToViewportPoint(transform.position);
        return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        yield return new WaitForSeconds(_outOfCameraTime);
        baseCharacterScript.TakeDamage(30);
        transform.position = _originalPosition; // Return to the original position
        _returnCoroutine = null; // Reset the coroutine reference
    }
    void FixedUpdate()
    {
        if (isDash) return;
        if (isWallJumping) return;
        if (_isAirKick) return;
        if (_isSuperDashing)
        {
            if (AnyKeyPressed) StopSuperDash();
            return;
        }
        if (previousXAxis != xAxis && xAxis == 0)
        {
            MoveReleased = true;
        }
        if (previousXAxis != xAxis) previousXAxis = xAxis;
        //asign new velocity to the rigid body
        Vector2 vel = MovementVelocity();
        rb2d.velocity = vel;

        if (xAxis > 0 && !isFacingRight) Flip();
        if (xAxis < 0 && isFacingRight) Flip();


        //check and perform attack if attacking
        Attack();
        SideAttackAction();

        //asign platform velocity to the palyer rigid body
        if(_platformRb2d!=null)
        {
            rb2d.velocity += _platformRb2d.velocity;
        }
    }
    #region  wall jump and slide
    public LayerMask wallLayer;
    public Transform wallCheck;
    public float wallDistance = 0.2f;
    private bool isWallSliding = false;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float WallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpingPower = new(3f, 7f);
    private bool IsOnWall()
    {
        var hit = Physics2D.OverlapCircle(wallCheck.position, wallDistance, wallLayer);
        return hit && !grounded;
    }
    private void WallSlide()
    {
        if (IsOnWall() && xAxis != 0f)
        {
            isWallSliding = true;
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Clamp(rb2d.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else isWallSliding = false;
    }
    private void WallJumping()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            WallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;//allow wall jump for a brief moment
        }

        if (isJumpPressed && wallJumpingCounter > 0f)
        {
            isJumpPressed = false;
            isWallJumping = true;
            rb2d.velocity = new Vector2(WallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0;
            Flip();
            ChangeAnimationState(nameof(PlayerAnimation.WallJump));

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }
    #endregion
    #region gizmos_drawing
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * groundCastDistance, boxSize);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AirKickPoint.transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(AttackPoint.transform.position, SWORD_HITBOX);
        // Gizmos.DrawCube(transform.position + transform.up * ceilingCastDistance, boxSize);
    }
    #endregion
    #region attack
    private bool SideAttackPressed => Input.GetKeyDown(KeyCode.U);
    private bool MainAttackPressed => Input.GetKeyDown(KeyCode.J);
    public bool Weapon = false;//weapon equiped(hand-sword)
    public bool SideWeapon = false;//side weapon equiped(kick-bow)
    private bool isAttackPressed;
    private bool _isAirShooting;
    private bool _isSideAttackPressed;
    [SerializeField] private Vector2 _airKickForce = new(500, -500);
    [SerializeField] private float attackDelay = 0.3f;
    [SerializeField] private float sideAttackDelay = 0.5f;
    private bool isAttacking;
    public Transform AttackPoint;
    public Transform AirKickPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    [SerializeField] private sbyte _attackDamage = 5;
    private sbyte _attackCount = 0;
    private bool _isAirKick = false;
    [SerializeField] private sbyte _airKickDamage = 5;
    private readonly Vector2 SWORD_HITBOX = new(1.5f, 3);
    private bool isFacingRight = true;  // For determining which way the player is currently facing.
    private void Attack()
    {
        if (isAttackPressed)
        {
            isAttackPressed = false;
            if (!isAttacking)
            {
                isAttacking = true;
                _attackCount++;
                _attackCount = (sbyte)((_attackCount > 3) ? 1 : _attackCount);//hand attack only has 3 step 
                if (grounded)
                {
                    //Weapon = true -> has sword
                    string attackAnimation = (Weapon ? "SAttack" : "Attack") + _attackCount.ToString();
                    PlayAudio(Weapon ? Audios.sword_slash : Audios.punch);
                    ChangeAnimationState(attackAnimation);
                }
                else
                {
                    _isAirKick = true;
                    Vector2 multiplyForce = isFacingRight ? new(1, 1) : new(-1, 1);
                    rb2d.AddForce(_airKickForce * multiplyForce);
                    ChangeAnimationState(nameof(PlayerAnimation.AirAttack));
                }
                HitDamage(_attackDamage);
                Invoke(nameof(AttackComplete), attackDelay);
            }
        }
    }
    private void SideAttackAction()
    {
        if (_isSideAttackPressed)
        {
            _isSideAttackPressed = false;
            if (!isAttacking)
            {
                isAttacking = true;
                if (!SideWeapon)
                {
                    ChangeAnimationState(nameof(PlayerAnimation.Kick));
                    HitDamage();
                }
                else
                {
                    if (
                        (!isFacingRight && _arrow.transform.localScale.x > 0)
                        || (isFacingRight && _arrow.transform.localScale.x < 0)
                        )
                    {
                        Vector2 newScale = _arrow.transform.localScale;
                        newScale.x *= -1;
                        _arrow.transform.localScale = newScale;
                    }

                    _currentArrow = Instantiate(_arrow, _arrowStart.position, Quaternion.identity);
                    ChangeAnimationState(grounded ?
                                        nameof(PlayerAnimation.Archery) :
                                        nameof(PlayerAnimation.AirArchery));
                    PlayAudio(Audios.arrow);
                    _isAirShooting = !grounded;
                    if (_isAirShooting)
                    {
                        _originalGravity = rb2d.gravityScale;
                        rb2d.gravityScale = 0;
                        rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
                    }
                }
                Invoke(nameof(AttackComplete), sideAttackDelay);
            }
        }
    }

    void AttackComplete()
    {
        isAttacking = false;
        if (_currentArrow != null)
        {
            var arrowController = _currentArrow.GetComponent<Arrow>();
            arrowController.Shoot(xDirection: isFacingRight ? 1 : -1);
        }
        if (_isAirShooting)
        {
            _isAirShooting = false;
            rb2d.gravityScale = _originalGravity;
        }
    }
    private void HitDamage(sbyte damage = 0)
    {
        Collider2D[] hitEnemies = !Weapon ?
            Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, enemyLayer)
            : Physics2D.OverlapBoxAll(AttackPoint.position, SWORD_HITBOX, 0, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            var baseScript = enemy.GetComponent<BaseCharacterScript>();
            baseScript.TakeDamage(damage);
            PlayAudio(Weapon ? Audios.sword_slash : Audios.kick);
        }
    }

    private void AirKick()
    {
        if (_isAirKick)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(AirKickPoint.position, attackRange, enemyLayer);
            foreach (var enemy in hitEnemies)
            {
                var baseScript = enemy.GetComponent<BaseCharacterScript>();
                baseScript.TakeDamage(_airKickDamage);
            }

        }
    }
    #endregion
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector2 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    #region dash 
    private bool DashPressed => Input.GetKeyDown(KeyCode.L);
    private bool DashReleased => Input.GetKeyUp(KeyCode.L);
    private bool isDash;
    private bool canDash = true;
    [SerializeField] private float _dashInterval = 0.3f;
    [SerializeField] private float _dashCooldown = 2f;
    private bool _canAirDash = false;
    [SerializeField] private bool _canSuperDash = false;
    [SerializeField] private float _superDashVel = 10f;//super dash velocity
    [SerializeField] private float _dashingPower = 3f;
    private readonly float _superDashCpltInterval = 1.0f;//1.0 sec to complete dash
    private float _dashTiming;//to cal the time holding the dash(L) key
    private bool _isSuperDashing;
    private bool _dashPressed;//true = press else is release;
    private Vector2 _orgnBoxColSize;//original box colider size
    private IEnumerator Dash()
    {
        //allow player only dash when on ground or air dash avaiable
        if (grounded || _canAirDash)
        {
            canDash = false;
            isDash = true;
            float originalGravity = rb2d.gravityScale;
            rb2d.gravityScale = 0f;
            rb2d.velocity = new Vector2(transform.localScale.x * _dashingPower, 0f);
            PlayAudio(Audios.dash);
            _boxCol2d.enabled = false;
            if (grounded)
                ChangeAnimationState(nameof(PlayerAnimation.Slide));
            else ChangeAnimationState("");

            yield return new WaitForSeconds(_dashInterval);

            // if (grounded)
            //     ChangeAnimationState(nameof(PlayerAnimation.));
            // yield return new WaitForSeconds(0.2f);
            rb2d.gravityScale = originalGravity;
            _boxCol2d.enabled = true;
            isDash = false;
            yield return new WaitForSeconds(_dashCooldown);
            canDash = true;
        }
    }

    private void PerformDash()
    {
        if (!_canSuperDash)
        {
            if (canDash) StartCoroutine(Dash());
            else return;
        }
        else
        {
            if (_dashTiming < _superDashCpltInterval)
                StartCoroutine(Dash());
            else SuperDash();
        }

    }
    private void SuperDash()
    {
        ChangeAnimationState(nameof(PlayerAnimation.SuperDash));
        _plCol2d.enabled = false;//disable polygon collider to unwanted collision
        spriteRenderer.flipX = true;//flip the sprite
        _isSuperDashing = true;
        _originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0;
        rb2d.velocity = new Vector2(isFacingRight
            ? _superDashVel
            : _superDashVel * -1
            , 0);
        PlayAudio(Audios.super_dash);
    }
    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (_isSuperDashing)
        {
            StopSuperDash();
        }
    }
    private void StopSuperDash()
    {
        _isSuperDashing = false;
        spriteRenderer.flipX = false;//return normal
        _plCol2d.enabled = true;//enable polygon colider
        rb2d.gravityScale = _originalGravity;
        _boxCol2d.size = _orgnBoxColSize;//change back the size to original one
        StopAudio();
    }
    private bool AnyKeyPressed => Input.anyKeyDown;
    #endregion
    #region movement
    //(xAxis/Walk-Run-CrouchWalk,yAxis/Jump)
    private bool CrouchPressed => Input.GetKeyDown(KeyCode.S);
    private bool CrounchReleased => Input.GetKeyUp(KeyCode.S);
    private bool JumpPressed => Input.GetKeyDown(KeyCode.Space);
    private float previousXAxis;
    private bool MoveReleased;
    private float xAxis;
    private bool isJumpPressed;
    public float groundCastDistance = 1.3f;
    public float ceilingCastDistance = 1.0f;
    private bool grounded;
    [SerializeField, Range(0, 5)] private byte _maxAirJump = 1;
    private byte _airJumpCount;
    [SerializeField] private float jumpHeight = 15f;//height of the jump
    private byte _airState = 0;//0 mean grounded, 1 mean up to air, 2 mean is falling
    private bool isCrouching;
    [SerializeField] private float crouchSlowdown = 0.4f;
    [SerializeField] private float walkSpeed = 5f;
    private Vector2 MovementVelocity()
    {
        Vector2 vel = new(rb2d.velocity.x, rb2d.velocity.y);
        if (MoveReleased)
        {
            vel.x = 0;
        }
        if (xAxis != 0)
        {
            vel.x += isCrouching ? (xAxis * walkSpeed * crouchSlowdown) : (xAxis * walkSpeed);
        }

        if (isJumpPressed)
        {
            isJumpPressed = false;
            if (grounded || _airJumpCount < _maxAirJump)
            {
                _airJumpCount++;
                vel.y = jumpHeight;
                ChangeAnimationState(grounded ? nameof(PlayerAnimation.Jump) : nameof(PlayerAnimation.WallJump));
            }
        }
        return vel;
    }
    private void Crouch()
    {
        _boxCol2d.enabled = !isCrouching;
        if (isCrouching)
        {
            Sprite crouchSprite = Resources.Load<Sprite>("Sprites/Player/crouch");
            spriteRenderer.sprite = crouchSprite;
        }
    }
    private bool HasCeiling
        => Physics2D.BoxCast(transform.position, boxSize, 0, transform.up, ceilingCastDistance, groundLayer);
    public bool IsGrounded()
    {
        grounded = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, groundCastDistance, groundLayer);
        return grounded;
    }

    #endregion
    #region Animations
    private string currentAnimaton;
    private void UpdateAnimation()
    {
        if (isWallSliding)
        {
            ChangeAnimationState(nameof(PlayerAnimation.WallSlide));
        }
        if (grounded && !isAttacking && !isDash)
        {
            if (xAxis != 0)
            {
                if (isCrouching)
                {
                    ChangeAnimationState(nameof(PlayerAnimation.Crouch));
                }
                else
                {
                    if (Mathf.Abs(walkSpeed) > 9)
                        ChangeAnimationState(nameof(PlayerAnimation.Run));
                    else
                        ChangeAnimationState(nameof(PlayerAnimation.Walk));
                }
            }
            else
            {
                var idleAnimation = Weapon ?
                nameof(PlayerAnimation.Idle) + "2" :
                nameof(PlayerAnimation.Idle);
                ChangeAnimationState(idleAnimation);
            }
        }
        if (_airState == 2 && !isAttacking && !_isAirKick)
        {
            ChangeAnimationState(nameof(PlayerAnimation.Fall));
        }
    }
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        _animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }
    #endregion
    #region Die
    protected override void Die(float delayTime = 0)
    {
        float animationLength = _animator.runtimeAnimatorController
                                .animationClips
                                .FirstOrDefault(c => c.name == nameof(PlayerAnimation.Die)).length;
        ChangeAnimationState(nameof(PlayerAnimation.Die));
        base.Die(animationLength);
    }
    private bool IsDead => currentHealth <= 0;
    #endregion
    #region audio
    private AudioSource _audio;
    private readonly string AUDIO_FOLDER = "Audios/";
    enum Audios
    {
        arrow
        , holding
        , punch
        , sword_slash
        , dash
        , super_dash
        , kick
    }
    private void PlayAudio(Audios audioName, bool loop = false)
    {
        var audioClip = Resources.Load<AudioClip>(AUDIO_FOLDER + audioName.ToString());
        _audio.clip = audioClip;
        _audio.loop = loop;
        _audio.Play();
        // StartCoroutine(StopAudio(audioClip.length));
    }
    private void StopAudio() => _audio.Stop();
    #endregion
    #region Detect Moving Platform
    private Rigidbody2D _platformRb2d;
    private void DetectMovingPlatform()
    {
        if(grounded)
        {
            var detection = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, groundCastDistance, groundLayer);
            if(detection.collider.CompareTag("MovingPlatform"))
            {
                _platformRb2d = detection.collider.GetComponent<Rigidbody2D>();
            }
            else
            {
                _platformRb2d = null;
            }
        }
        else
            transform.parent = null;
    }
    #endregion
}
