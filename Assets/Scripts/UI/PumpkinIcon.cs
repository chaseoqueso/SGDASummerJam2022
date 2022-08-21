using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PumpkinIcon : MonoBehaviour
{
    [SerializeField] private Image icon;
    public bool pumpkinIsActive {get; private set;}

    void Awake()
    {
        TogglePumpkin(false);
    }

    public void TogglePumpkin(bool set)
    {
        pumpkinIsActive = set;
        if(set){
            icon.color = new Color(255,255,255,255);
        }
        else{
            icon.color = new Color(255,255,255,0);
        }
    }
}
