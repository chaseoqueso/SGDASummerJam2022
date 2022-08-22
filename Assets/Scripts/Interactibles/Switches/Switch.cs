using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour, IInteractible
{
    [Tooltip("The script(s) to activate with this switch.")]
    public List<SwitchTarget> SwitchTargets;

    public bool IsPressed { get; private set; }

    public void OnInteract(GameObject interactor)
    {
        if(IsPressed)
            return;

        IsPressed = true;

        foreach(SwitchTarget target in SwitchTargets)
        {
            target.OnSwitchActivate(this);
        }
    }

    public void Reset()
    {
        IsPressed = false;
    }
}
