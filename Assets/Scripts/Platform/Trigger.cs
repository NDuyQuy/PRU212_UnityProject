using System.Collections;
using UnityEngine;
public class Trigger : PlatformControl
{
    //delay before start platform effect
    [SerializeField] private float _beforeEffectDelay = 0.5f;
    //delay after platform effect has start
    [SerializeField] private float _afterEffectDelay = 0.5f;
    [SerializeField] private TriggerEffect _platformEffect;

    [SerializeField] private float yVelocity;
    [SerializeField] private float xVelocity;

    public enum TriggerEffect
    {
        MoveOnYAxis,
        MoveOnXAxis,
        DiagnalMove,
        DisableCollider,
        Flip
    }
    private void FixedUpdate()
    {
        if (PlayerTouched)
            StartCoroutine(StartEffect());
    }

    private IEnumerator StartEffect()
    {
        PlayerTouched = false;
        yield return new WaitForSeconds(_beforeEffectDelay);
        switch (_platformEffect)
        {
            case TriggerEffect.MoveOnXAxis:
                MoveOnXAxis();
                break;
            case TriggerEffect.MoveOnYAxis:
                MoveOnYAxis();
                break;
            case TriggerEffect.DiagnalMove:
                CustomMove();
                break;
            case TriggerEffect.DisableCollider:
                DisableCollider();
                StartCoroutine(AfterEffect());
                break;
            case TriggerEffect.Flip:
                break;
        }
        Updated = true;
    }

    private void DisableCollider() => _enableColider = false;
    private void MoveOnYAxis() => Moving(0, yVel: yVelocity);
    private void MoveOnXAxis() => Moving(xVel: xVelocity, 0);
    private void CustomMove() => Moving(xVelocity, yVelocity);

    private IEnumerator AfterEffect()
    {
        yield return new WaitForSeconds(_afterEffectDelay);
        //implement logic for after effect here
        switch (_platformEffect)
        {
            case TriggerEffect.DisableCollider:
                EnableCollider();
                break;
            default:
            //implement later for other logic
                break;
        }
        Updated = true;
    }

    private void EnableCollider() => _enableColider = true;

    private void MoveBack()
    {
        //implement for move the platform back to start position if needed
    }

}