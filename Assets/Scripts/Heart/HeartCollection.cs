using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartCollection : MonoBehaviour
{
    [SerializeField] private sbyte healthValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            var player = collision.gameObject.GetComponent<BaseCharacterScript>();
            player.AddHearth(healthValue);
            gameObject.SetActive(false);
        }
    }
}
