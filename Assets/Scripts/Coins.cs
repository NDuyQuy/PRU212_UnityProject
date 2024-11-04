using System.Collections;
using UnityEngine;
public class Coins : MonoBehaviour
{
    public enum CoinsValue
    {
        Bronze = 5,
        Gold = 35,
        Silver = 20
    }

    private CoinsValue _currentCoin = CoinsValue.Gold;
    
    private Animator _animator;
    private AudioSource _audio;
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (collision2D.transform.CompareTag("Player"))
        {
            var player = collision2D.transform.gameObject.GetComponent<PlayerControl>();
            player.Currency += (int)_currentCoin;
            Debug.Log($"Current Coin: {_currentCoin.ToString()} =" + _currentCoin);
            _spriteRenderer.enabled = false;
            _audio.Play();
            StartCoroutine(DestroyOvertime(1f));
        }
    }

    //hung add
    public void SetCoinValue(CoinsValue coinsValue)
    {
        this._currentCoin = coinsValue;
        StartAnimation();
    }

    private void StartAnimation()
    {
        _animator.Play(_currentCoin.ToString());
    }

    private IEnumerator DestroyOvertime(float time)
    {
        yield return new WaitForSeconds(time);
        _audio.Stop();
        Destroy(gameObject);
    }
}