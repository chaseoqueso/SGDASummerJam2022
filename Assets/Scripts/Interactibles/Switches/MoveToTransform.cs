using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTransform : SwitchTarget
{
    [Tooltip("The position to translate toward when activated (leave empty if no translation is desired).")]
    public Transform TargetPosition;
    [Tooltip("The rotation to rotate toward when activated (leave empty if no rotation is desired).")]
    public Transform TargetRotation;
    [Tooltip("The scale to grow/shrink toward when activated (leave empty if no scaling is desired).")]
    public Transform TargetScale;
    [Tooltip("The duration in seconds for the transformation to take place.")]
    public float TransformationTime = 2f;
    [Tooltip("Whether or not the switch can reverse the transformation if pressed again.")]
    public bool Reversible;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;

    private bool _isAtDestination;

    void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _originalScale = transform.localScale;
        _isAtDestination = false;
    }

    public override void OnSwitchActivate(Switch switchScript)
    {
        StartCoroutine(TransformationRoutine(switchScript));
    }

    private IEnumerator TransformationRoutine(Switch switchScript)
    {
        float progress = 0;
        while(progress < 1)
        {
            // Invert our progress value if we're moving from the destination to the original position
            float lerpProgress;
            if(_isAtDestination)
            {
                lerpProgress = 1 - progress;
            }
            else
            {
                lerpProgress = progress;
            }

            // Move if we have a target to move toward
            if(TargetPosition != null)
            {
                transform.position = Vector3.Lerp(_originalPosition, TargetPosition.position, lerpProgress);
            }

            // Rotate if we have a target to rotate toward
            if(TargetRotation != null)
            {
                transform.rotation = Quaternion.Lerp(_originalRotation, TargetRotation.rotation, lerpProgress);
            }

            // Scale if we have a target to scale toward
            if(TargetScale != null)
            {
                transform.localScale = Vector3.Lerp(_originalScale, TargetScale.localScale, lerpProgress);
            }

            progress += Time.deltaTime / TransformationTime;

            yield return null;
        }

        // Reset the switch if this script is reversible
        if(Reversible)
        {
            switchScript.Reset();
        }

        _isAtDestination = !_isAtDestination;
    }
}
