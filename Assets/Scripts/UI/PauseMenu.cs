using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [SerializeField] private GameObject pausePanel;

    public void ReturnToMenu()
    {
        ResumeGame();
        SceneManager.LoadScene(UIManager.MAIN_MENU_SCENE_NAME);
    }

    public void PauseGame()
    {
        if(SceneManager.GetActiveScene().name != UIManager.GAME_SCENE_NAME){
            return;
        }

        GameIsPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        GameIsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }
}
