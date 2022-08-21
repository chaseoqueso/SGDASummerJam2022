using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button pauseTopButton;

    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private Button settingsTopButton;

    // TODO: call PauseGame() wherever input is handled when the input button is clicked

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

        UIManager.instance.RevealCounterUI(false);

        GameIsPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;

        ToggleSettings(false);
    }

    public void ResumeGame()
    {
        UIManager.instance.HideCounterUI(false);

        ToggleSettings(false);

        GameIsPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void ToggleSettings(bool set)
    {
        settingsMenuPanel.SetActive(set);
        pauseMenuPanel.SetActive(!set);

        if(set){
            settingsTopButton.GetComponent<UIButtonFixer>().SelectOnMenuSwitch();
        }
        else{
            pauseTopButton.GetComponent<UIButtonFixer>().SelectOnMenuSwitch();
        }
    }
}
