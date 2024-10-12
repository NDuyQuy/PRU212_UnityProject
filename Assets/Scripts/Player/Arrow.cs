using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    #region ArrowEffects
    public enum Effects
    {
        Explosion,
        SelfMultiply,
        SuperKnockback,
        FollowEnemy
    }
    public bool HasEffect = false;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject explosion;
    [SerializeField] private sbyte basedDamage = 10;
    public Effects CurrentEffect;
    #endregion
    private TrailRenderer tr;
    [SerializeField] private GameObject enemyTarget;
    private readonly float _rotateSpeed = 200f;

    private readonly string[] spriteNames = { "arrow", "arrow1", "arrow2", "arrow3", "arrow4", "light_arrow" };
    private readonly string[] trailColor = { "#00000", "#8c5b3e", "#787878", "#d8d8d8", "#73efe8", "#c7ef63" };
    private readonly string _parrentFolder = "Sprites/Player/Arrows/";
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BoxCollider2D colider2d;
    [Range(0, 5)] public sbyte Level = 0;
    public bool IsLevelUp = false;
    private float originalGravity;
    [SerializeField] private float shootForce = 200f;
    #region time
    //5 sec if object not interact with anything then it gonna self destroy
    private readonly float _destroyTime = 5f;
    private float _existingTime = 0;
    private readonly float _selfDestructionDelay = 0.5f;
    #endregion
    void Awake()
    {
        tr = GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        colider2d = GetComponent<BoxCollider2D>();
        originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        ChangeRender();
        colider2d.enabled = false;//disable when the arrow appear
    }
    void Update()
    {
        _existingTime += Time.deltaTime;
        if(_existingTime>_destroyTime)
            StartCoroutine(OvertimeDestroy());
        if (IsLevelUp)
        {
            IsLevelUp = false;
            Level++;
            if (Level > spriteNames.Length - 1) Level = 0;
            ChangeRender();
        }
    }
    
    private void ChangeRender()
    {
        Sprite sprite = Resources.Load<Sprite>(_parrentFolder + spriteNames[Level]);
        if (sprite != null)
        {
            //change arrow display image 
            sr.sprite = sprite;
            //change the colider based on the sprite border
            Vector4 border = sprite.border;
            Vector2 spriteSize = sprite.bounds.size;
            Vector2 newSize = new(
                spriteSize.x - (border.x + border.z) / sprite.pixelsPerUnit, // Subtract left and right border
                spriteSize.y - (border.y + border.w) / sprite.pixelsPerUnit  // Subtract bottom and top border
            );
            colider2d.size = newSize;
            colider2d.offset = sprite.bounds.center;
            //change trail start color 
            ColorUtility.TryParseHtmlString(trailColor[Level], out Color color);
            tr.startColor = color;
        }
        else Debug.Log("Sprite not found");
    }
    public void Shoot(float xDirection = 1)
    {
        if (HasEffect && CurrentEffect == Effects.FollowEnemy)
            return;
        if (HasEffect && CurrentEffect == Effects.SelfMultiply)
        {
            ShootWithDelay(xDirection);
        }
        colider2d.enabled = true;
        rb.AddForce(new Vector2(shootForce, 0) * xDirection);
        rb.gravityScale = originalGravity;
    }
    private void ShootWithDelay(float xDirection)
    {
        float verticalOffset = 0.2f;
        for (int i = 0; i < (Level + 1); i++)
        {
            var spawnPosition = transform.position + new Vector3(0,verticalOffset*(i+1),0);
            // Wait for the specified delay
            GameObject newArrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
            // Instantiate the new arrow
            // Apply force to the new arrow
            if (newArrow.TryGetComponent<Arrow>(out var newArrScript))
            {
                newArrScript.CloneShoot(xDirection);
            }
        }
    }
    public void CloneShoot(float xDirection)
    {
        colider2d.enabled = true;
        rb.gravityScale = originalGravity;
        rb.AddForce(new Vector2(shootForce,0) * xDirection);
        rb.gravityScale = originalGravity;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player")) return;
        if (col.gameObject.CompareTag("Enemy"))
        {
            var enemyBaseScript = col.gameObject.GetComponent<BaseCharacterScript>();
            enemyBaseScript.TakeDamage((sbyte)Damage);
            if (HasEffect && CurrentEffect == Effects.SuperKnockback)
            {
                Vector2 knockBackDirection = rb.velocity.x > 0 ? new(1, 1) : new(-1, 1);
                enemyBaseScript.KnockBack(knockBackDirection, 30f);
            }
        }
        if (HasEffect && CurrentEffect == Effects.Explosion)
        {
            GameObject obj = Instantiate(explosion, transform.localPosition, Quaternion.identity);
        }
        Destroy(gameObject);
    }
    private float Damage => basedDamage * (Level + 1) / 2;
    private void ChasingEnemy()
    {
        if (enemyTarget == null) return;
        colider2d.enabled = true;
        Vector2 direction = (Vector2)enemyTarget.transform.position - rb.position;
        direction.Normalize();
        // Calculate the amount to rotate
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        // Rotate the arrow towards the target
        rb.angularVelocity = -rotateAmount * _rotateSpeed;
        // Move the arrow forward
        rb.velocity = transform.right * 10f;
    }

    private IEnumerator OvertimeDestroy()
    {
        tr.enabled = false;
        yield return new WaitForSeconds(tr.time + _selfDestructionDelay);
        Destroy(gameObject);
    }
}
