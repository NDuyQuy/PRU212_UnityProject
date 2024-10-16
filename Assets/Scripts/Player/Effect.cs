using System.Globalization;
using System.Linq;
using UnityEngine;

public class Effect : MonoBehaviour
{
    private Animator _animator;

    private SpriteRenderer _spriterd;
    public string EffectAnimation ;
    [SerializeField]private bool _loop; 
    private string _currentAnimation;
    private Vector2 effectHitBox;
    public enum EffectType
    {
        None,
        Damage,
        Stun,
        Another
    }
    private LayerMask enemy;
    public EffectType CurrentEffect;
    public bool Enable = true;//determine enable or disable the effect
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriterd = GetComponent<SpriteRenderer>();
        effectHitBox = transform.localScale;
    }
    void Start()
    {
        if(Enable) StartEffect(EffectAnimation);
    }
    void Update()
    {
        if(_currentAnimation!=EffectAnimation)
        {
            _currentAnimation = EffectAnimation;
            StartEffect(EffectAnimation);
        }
        if(!Enable)
        {
            //disable animator and sprite renderer
            _animator.enabled = false;
            _spriterd.enabled = false;
        }
        else
        {
            _animator.enabled = true;
            _spriterd.enabled = true;
        }
    }
    private void StartEffect(string effectAnimation)
    {
         _animator.Play(effectAnimation);
        if(_loop==true) return;
        
        float animationDuration = _animator.runtimeAnimatorController
                                .animationClips
                                .FirstOrDefault(c => c.name == effectAnimation).length;       
        Invoke(nameof(Complete), animationDuration);
    }
    private void Complete()
    {
        Destroy(gameObject);
    }

    private void ApplyEffect()
    {
        switch (CurrentEffect)
        {
            case EffectType.Damage:
                return;
        }
    }
    private bool HitEnemy
        => Physics2D.BoxCast(transform.position, effectHitBox, 0, transform.up, enemy);

}
