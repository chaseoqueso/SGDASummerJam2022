using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;

    public void Play()
    {
        SceneManager.LoadScene(UIManager.GAME_SCENE_NAME);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToggleCredits(bool set)
    {
        creditsPanel.SetActive(set);
        mainMenuPanel.SetActive(!set);
    }
}
