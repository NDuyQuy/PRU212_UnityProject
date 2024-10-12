using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasedTrap : MonoBehaviour
{
    [SerializeField]protected float thrust = 10f;
    protected sbyte damage = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // called when the cube hits the floor
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            var player = col.gameObject.GetComponent<BaseCharacterScript>();
            var playerRB2d = col.gameObject.GetComponent<Rigidbody2D>();
            //Give damage for the player character when it hit this trap
            player.TakeDamage(0);
            //check if the velocity of the player character to implement the knockback direction
            //velocity > 0 -> going right -> knockback xAxis direction = left
            player.KnockBack(new Vector2((playerRB2d.velocity.x>0)?-1:1, 1) , thrust);
            Debug.Log("hit");
        }
    }
   
}
