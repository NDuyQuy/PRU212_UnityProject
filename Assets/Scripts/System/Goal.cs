using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        int currentSenceIdx = SceneManager.GetActiveScene().buildIndex;
        if (collision.gameObject.CompareTag("Player"))
            SceneManager.LoadScene(++currentSenceIdx);
    }
}
