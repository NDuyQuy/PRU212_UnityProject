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

    [SerializeField] private CoinsValue _currentCoin = CoinsValue.Gold;

    private Animator _animator;
    private AudioSource _audio;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb2d;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb2d = GetComponent<Rigidbody2D>();
        StartAnimation();
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
            _rb2d.excludeLayers = LayerMask.GetMask("Player");
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