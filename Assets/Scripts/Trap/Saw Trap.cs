using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawTrap : BaseTrap
{
    [SerializeField] private float movementDistance;
    [SerializeField] private float movementSpeed;
    
    private bool movingLeft;
    private float leftEdge;
    private float rightEdge;
    private void Awake()
    {
        leftEdge = transform
            .position.x - movementDistance;
        rightEdge = transform
            .position.x + movementDistance;
    }

    private void Update()
    {
        if (movingLeft)
        {
            if (transform.position.x > leftEdge)
            {
                transform.position = new Vector3(transform.position.x - movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
            }
            else
            {
                movingLeft = false;
                Vector2 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
            }

        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                transform.position = new Vector3(transform.position.x + movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
            }
            else
                movingLeft = true;
                Vector2 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
        }
    }
}
