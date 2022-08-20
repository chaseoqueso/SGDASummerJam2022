using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cobweb : MonoBehaviour, IInteractible
{
    public void OnInteract(GameObject interactor)
    {
        Destroy(gameObject);
    }
}
