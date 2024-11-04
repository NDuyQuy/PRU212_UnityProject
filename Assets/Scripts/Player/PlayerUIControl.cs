using TMPro;
using UnityEngine;
public class PlayerUIController : MonoBehaviour
{
    public TextMeshProUGUI money;
    public TextMeshProUGUI health;
    public PlayerControl player;

    void Awake()
    {
        health.text = $"{player.currentHealth}";
        money.text = $"{player.Currency}";
    }
    void Update()
    {
        if(player==null) return;
        if (player.currentHealth != int.Parse(health.text))
            health.text = $"{player.currentHealth}";
        if (player.Currency != int.Parse(money.text))
            money.text = $"{player.Currency}";
    }
}