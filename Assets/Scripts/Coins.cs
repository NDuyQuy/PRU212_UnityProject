using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
public class Coins : MonoBehaviour
{
    public enum CoinsValue
    {
        Bronze = 5,
        Gold = 35,
        Silver = 20
    }
    [SerializeField] private CoinsValue _currentCoin;
    private Animator _animator;
    private AudioSource _audio;
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        StartAnimation();
    }
    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (collision2D.transform.CompareTag("Player"))
        {
            var player = collision2D.transform.gameObject.GetComponent<PlayerControl>();
            player.Currency += (int)_currentCoin;
            _spriteRenderer.enabled = false;
            _audio.Play();
            StartCoroutine(DestroyOvertime(1f));
        }
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