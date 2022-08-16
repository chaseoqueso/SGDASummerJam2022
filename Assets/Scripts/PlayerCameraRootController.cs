using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraRootController : MonoBehaviour
{
    public Transform player;

    private Vector3 cameraOffset;

    void Start()
    {
        cameraOffset = transform.position - player.position;
        transform.parent = null;
    }

    void LateUpdate()
    {
        transform.position = player.position + cameraOffset;
    }
}
