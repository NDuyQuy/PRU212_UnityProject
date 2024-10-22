using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GolemBoss golemBoss;

    public void SetGolemBoss(GolemBoss golemBoss) => this.golemBoss = golemBoss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PlayerControl>()  != null)
        {
            golemBoss.DeductHealthPlayer(golemBoss.ProjectileDamge);
        }
    }
}
