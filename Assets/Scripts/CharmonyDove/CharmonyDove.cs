using UnityEngine;

public class CharmonyDove : BaseCharacterScript
{
    private bool _hittedOnce;
    private bool _attackPhase;
    private GameObject _angryMask;
    private GameObject _dialog; 
    private bool _isFacingRight;
    private readonly string DEFAULT_PATH = "Sprites/CharmonyDove/";
    private enum Phase
    {
        normal,
        angry
    }
    private void Awake()
    {
        _angryMask = transform.Find("AngryMask").gameObject;
        _dialog = transform.Find("Dialog").gameObject;
        _angryMask.SetActive(_hittedOnce);
        _dialog.SetActive(!_hittedOnce);
    }
    private void Update()
    {
        if(_hittedOnce&&!_attackPhase)
        {
            _attackPhase = true;
            ChangePhase();
        }
    }
    protected override void CheckHit()
    {
        base.CheckHit();
        if(!_hittedOnce) _hittedOnce = true;
    }
    private void ChangePhase()
    {
        Sprite angryDove = Resources.Load<Sprite>(DEFAULT_PATH+Phase.angry.ToString());
        spriteRenderer.sprite = angryDove;
        _dialog.SetActive(false);
        _angryMask.SetActive(true);
        Vector2 force = _isFacingRight? new(-400,100):new(400,100);
        rb2d.AddForce(force);
    }

}
