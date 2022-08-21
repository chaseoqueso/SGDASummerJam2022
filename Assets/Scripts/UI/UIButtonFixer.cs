using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UIButtonFixer : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    private PumpkinIcon pumpkinIcon;

    private void SetPumpkinIconIfNull()
    {
        if(!pumpkinIcon){
            pumpkinIcon = GetComponentInChildren<PumpkinIcon>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EventSystem.current.alreadySelecting){
            EventSystem.current.SetSelectedGameObject(this.gameObject);

            SetPumpkinIconIfNull();
            pumpkinIcon.TogglePumpkin(true);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetPumpkinIconIfNull();
        pumpkinIcon.TogglePumpkin(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        GetComponent<Selectable>().OnPointerExit(null);

        SetPumpkinIconIfNull();
        pumpkinIcon.TogglePumpkin(false);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        SetPumpkinIconIfNull();
        pumpkinIcon.TogglePumpkin(false);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        SetPumpkinIconIfNull();
        pumpkinIcon.TogglePumpkin(false);
    }

    public void SelectOnMenuSwitch()
    {
        GetComponent<Selectable>().Select();
        
        SetPumpkinIconIfNull();
        pumpkinIcon.TogglePumpkin(true);
    }
}
