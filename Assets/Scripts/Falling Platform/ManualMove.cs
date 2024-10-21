using System.Collections;
using UnityEngine;
public class ManualMove : PlatformControl
{
    [SerializeField] private bool _moveUp = false;
    [SerializeField] private float _delay = 0.5f;
    private void FixedUpdate()
    {
        if(PlayerTouched)
        StartCoroutine(StartMove());
    }

    private IEnumerator StartMove()
    {
        PlayerTouched = false;
        yield return new WaitForSeconds(_delay);
        //updated movement here
        // Moving();
        Updated = true;
    }
}