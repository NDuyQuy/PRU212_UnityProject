using UnityEngine;

using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDownPlatform : MonoBehaviour
{
    private float fallDelay = 1f;
    [SerializeField] private float destroyDelay = 2f;
    private float currentTime;
    [SerializeField] private Rigidbody2D rb;
    private bool playerTouched = false;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private Vector2 _velocity = new(0, 1);
    private Vector2 startPosition;
    private Vector2 addRange = new(0, 5f);

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {


            // StartCoroutine(Fall());
            playerTouched = true;

        }

    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravity;
        //Destroy(gameObject, destroyDelay);
    }
    private void Update()
    {
        currentTime += Time.deltaTime;
        if (playerTouched && currentTime > 2f)
        {
            //   StartCoroutine(Fall());

        }
        var currentPosition = transform.position;
        if (currentPosition.y >= startPosition.y + addRange.y)
        {
            rb.velocity = _velocity * -1;
        }
        if (currentPosition.y <= startPosition.y - addRange.y)
        {
            rb.velocity = _velocity;
        }

    }
    private void Start()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.velocity = _velocity;
        startPosition = transform.position;

    }

}