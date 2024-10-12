using System.Linq;
using UnityEngine;

public class Effect : MonoBehaviour
{
    private Animator _animator;
    public string EffectAnimation;
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
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        StartEffect(EffectAnimation);
        effectHitBox = transform.localScale;
    }
    private void StartEffect(string effectAnimation)
    {
        float animationDuration = _animator.runtimeAnimatorController
                                .animationClips
                                .FirstOrDefault(c => c.name == effectAnimation).length;
        _animator.Play(effectAnimation);
        Invoke(nameof(Complete),animationDuration);
    }
    private void Complete()
    {
        Destroy(gameObject);
    }

    private void ApplyEffect()
    {
        switch(CurrentEffect)
        {
            case EffectType.Damage:
            return;
        }
    }
    private bool HitEnemy
        => Physics2D.BoxCast(transform.position,effectHitBox,0,transform.up,enemy);    
    
}
