using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public const string MAIN_MENU_SCENE_NAME = "Main Menu";
    public const string GAME_SCENE_NAME = "Playground";  // NOTE: UPDATE THIS AT THE END IF NECESSARY
    public const string WIN_SCENE_NAME = "WinScene";

    [SerializeField] private TMP_Text candleCount;
    [SerializeField] private TMP_Text sweetsCount;

    [SerializeField] private GameObject counterUIPanel;
    [SerializeField] private GameObject crosshairOverlay;

    [SerializeField] private PauseMenu pauseMenu;

    void Awake()
    {
        if(instance){
            Destroy(gameObject);
        }
        else{
            instance = this;
        }
    }

    public void ToggleCrosshairOverlay(bool set)
    {
        crosshairOverlay.SetActive(set);
    }

    public void IncrementCollectibleCount(CollectibleType type, int newValue)
    {
        if(type == CollectibleType.Candle){
            SetCandleCount(newValue);
        }
        else if(type == CollectibleType.Candy){
            SetSweetsCount(newValue);
        }
    }

    private void SetCandleCount(int newValue)
    {
        candleCount.text = "" + newValue;
        RevealCounterUI(true);
    }

    private void SetSweetsCount(int newValue)
    {
        sweetsCount.text = "" + newValue;
        RevealCounterUI(true);
    }

    public void RevealCounterUI(bool withAnimation)
    {
        // TODO: animate the panel to move into place for a moment, then slide back up off screen
    }

    public void HideCounterUI(bool withAnimation)
    {
        // TODO

        // if the coroutine is not complete, DON'T immediately hide it after pause -> play
    }

    public PauseMenu GetPauseMenu()
    {
        return pauseMenu;
    }

    public static void SetImageColorFromHex(Image img, string hexCode)
    {
        Color color;
        if(ColorUtility.TryParseHtmlString(hexCode, out color)){
            img.color = color;
        }
        else{
            Debug.LogWarning("Failed to set image color");
        }
    }
}
