using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public const string MAIN_MENU_SCENE_NAME = "Main Menu";
    public const string GAME_SCENE_NAME = "GameScene";  // NOTE: UPDATE THIS AT THE END IF NECESSARY
    public const string WIN_SCENE_NAME = "WinScene";

    [SerializeField] private TMP_Text candleCount;
    [SerializeField] private TMP_Text sweetsCount;

    [SerializeField] private PauseMenu pauseMenu;

    public void SetCandleCount(int newValue)
    {
        candleCount.text = "" + newValue;
    }

    public void SetSweetsCount(int newValue)
    {
        sweetsCount.text = "" + newValue;
    }

    public PauseMenu GetPauseMenu()
    {
        return pauseMenu;
    }

    public void ReturnToMenuFromWinScene()
    {
        // TODO: Reset counters!!!

        SceneManager.LoadScene(UIManager.MAIN_MENU_SCENE_NAME);
    }
}
