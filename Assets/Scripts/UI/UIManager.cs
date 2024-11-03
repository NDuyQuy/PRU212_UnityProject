using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;

    private void Awake()
    {
        gameOverScreen.SetActive(false);
    }

    #region Game Over Functions
    //Game over function
    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        }

   //Restart level
   public void Restart()
   {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
 }

   //Activate game over screen
    public void MainMenu()
   {
        SceneManager.LoadScene(0);
    }

   //Quit game/exit play mode if in Editor
    public void Quit()
    {       Application.Quit(); 
        UnityEditor.EditorApplication.isPlaying = false;       
    }
    #endregion
}
