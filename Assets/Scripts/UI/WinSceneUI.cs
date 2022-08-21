using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinSceneUI : MonoBehaviour
{
    [SerializeField] private Button returnButton;

    void Start()
    {
        returnButton.Select();
        returnButton.GetComponentInChildren<PumpkinIcon>().TogglePumpkin(true);
    }

    public void ReturnToMenuFromWinScene()
    {
        SceneManager.LoadScene(UIManager.MAIN_MENU_SCENE_NAME);
    }
}
