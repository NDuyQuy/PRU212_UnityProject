using UnityEngine;

public class Lazor : MonoBehaviour
{
    private GolemBoss golemBoss;
    private float timer;
    private bool isPlayerInLazor = false;

    public void SetGolomBoss(GolemBoss golemBoss) => this.golemBoss = golemBoss;

    private void Start()
    {
        timer = 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerControl>() != null)
        {
            Debug.Log("Get lazer player");
            isPlayerInLazor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerControl>() != null)
        {
            isPlayerInLazor = false;
        }
    }

    private void FixedUpdate()
    {
        if(isPlayerInLazor)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 1)
            {
                this.golemBoss.DeductHealthPlayer(golemBoss.LazerDamagePerSecond);
                timer = 0;
            }
        }
    }
}
