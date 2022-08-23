using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private Button mainMenuTopButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Button settingsBackButton;

    void Start()
    {
        mainMenuTopButton.Select();
        mainMenuTopButton.GetComponentInChildren<PumpkinIcon>().TogglePumpkin(true);
    }

    public void Play()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if(set){
            creditsBackButton.GetComponent<UIButtonFixer>().SelectOnMenuSwitch();
        }
        else{
            mainMenuTopButton.GetComponent<UIButtonFixer>().SelectOnMenuSwitch();
        }
    }

    public void ToggleSettings(bool set)
    {
        settingsPanel.SetActive(set);
        mainMenuPanel.SetActive(!set);

        if(set){
            settingsBackButton.GetComponent<UIButtonFixer>().SelectOnMenuSwitch();
        }
        else{
            mainMenuTopButton.GetComponent<UIButtonFixer>().SelectOnMenuSwitch();
        }
    }
}
