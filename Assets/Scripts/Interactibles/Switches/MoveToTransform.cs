using System.Collections;
using System.Collections.Generic;
using StarterAssets;
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

    private Rigidbody _rb;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;

    private bool _isAtDestination;
    private bool _isMoving;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
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
        _isMoving = true;
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
                _rb.MovePosition(Vector3.Lerp(_originalPosition, TargetPosition.position, lerpProgress));
            }

            // Rotate if we have a target to rotate toward
            if(TargetRotation != null)
            {
                _rb.MoveRotation(Quaternion.Lerp(_originalRotation, TargetRotation.rotation, lerpProgress));
            }

            // Scale if we have a target to scale toward
            if(TargetScale != null)
            {
                transform.localScale = Vector3.Lerp(_originalScale, TargetScale.localScale, lerpProgress);
            }

            progress += Time.fixedDeltaTime / TransformationTime;

            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForFixedUpdate();

        // Reset the switch if this script is reversible
        if(Reversible)
        {
            switchScript.Reset();
        }

        _isMoving = false;
        _isAtDestination = !_isAtDestination;
    }

    void OnTriggerStay(Collider other)
    {
        if(_isMoving)
            TryMovingObject(other.gameObject, Time.fixedDeltaTime);
    }

    void OnCollisionStay(Collision collision)
    {
        if(_isMoving)
            TryMovingObject(collision.gameObject, Time.fixedDeltaTime);
    }

    void OnRigidBodyPush(BasicRigidBodyPush other)
    {
        if(_isMoving)
            TryMovingObject(other.gameObject, (other._updateMode == RBPushUpdateMode.Update) ? Time.deltaTime : Time.fixedDeltaTime);
    }

    private void TryMovingObject(GameObject other, float timeStep)
    {
        ThirdPersonController playerScript;
        CharacterController controller;

        if(other.gameObject.TryGetComponent<ThirdPersonController>(out playerScript))
        {
            if(playerScript.CurrentState == PlayerState.Walking)
            {
                playerScript.GetComponent<CharacterController>().Move(_rb.velocity * timeStep);
                Debug.Log(_rb.velocity * timeStep);
            }
            else if(playerScript.CurrentState == PlayerState.Rolling)
            {
                playerScript.SetBonusVelocity(_rb.velocity);
            }
        }
        else if(other.TryGetComponent<CharacterController>(out controller))
        {
            controller.Move(_rb.velocity * timeStep);
        }
    }
}
