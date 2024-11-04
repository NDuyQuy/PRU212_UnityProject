using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpoint;
    private Transform currentCheckpoint;
    private BaseCharacterScript playerHealth;
     private UIManager uiManager;

    private void Awake()
    {
        playerHealth = GetComponent<BaseCharacterScript>();
         uiManager = FindObjectOfType<UIManager>();
    }

    public void RespawnCheck()
    {
        //Check if check point available
        if (currentCheckpoint == null)
        {

            //show game over screen
            uiManager.GameOver();
            return;
        }
        transform.position = currentCheckpoint.position; //move player to checkpoint position
        playerHealth.Respawn(); //restore player health and reset animation

        //Move the camera to the checkpoint's room
        Camera.main.GetComponent<CameraControl>().MoveToNewRoom(currentCheckpoint.parent);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Checkpoint")
        {
            currentCheckpoint = collision.transform;
            collision.GetComponent<Collider2D>().enabled = false;
            collision.GetComponent<Animator>().SetTrigger("appear");
        }
    }
}