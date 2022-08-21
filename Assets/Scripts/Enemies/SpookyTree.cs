using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;

public class SpookyTree : StationaryEnemyBase
{
    [Header("Tree Properties")]
    [Tooltip("The horizontal force to throw objects.")]
    [SerializeField] protected float horizontalThrowForce = 10f;
    [Tooltip("The vertical force to throw objects.")]
    [SerializeField] protected float verticalThrowForce = 10f;
    [Tooltip("The force to throw objects while possessed.")]
    [SerializeField] protected float possessedThrowForce = 20f;
    [Tooltip("The object to parent grabbable objects to.")]
    [SerializeField] protected Transform handPosition;
    [Tooltip("The script that manages the followPoint of the aim camera.")]
    [SerializeField] protected CameraManager _aimCameraScript;

    protected GameObject _grabbedObject;
    protected UnityAction _callback;
    protected float _verticalRotationSpeed;

    protected override IEnumerator Ability2Routine(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Ability2");
        _callback = abilityEndedCallback;

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability2"));

        abilityEndedCallback.Invoke();
    }

    public override void TriggerAbility2()
    {
        // Find all players and enemies in the hitbox
        Collider[] hits = Physics.OverlapBox(Ability1ReferenceHitbox.transform.position + Ability1ReferenceHitbox.transform.rotation * Ability1ReferenceHitbox.center, 
                                            Ability1ReferenceHitbox.size, 
                                            Ability1ReferenceHitbox.transform.rotation, 
                                            LayerMask.GetMask("Player"), 
                                            QueryTriggerInteraction.Collide);

        if(hits.Length != 0)
        {
            _player = hits[0].GetComponent<ThirdPersonController>();

            // Stop the attack from formally ending
            StopCoroutine(attackRoutine);

            // Force the player into a roll
            _player.ForceRoll();
            _player.SetVelocity(Vector3.zero);
            _player.CurrentState = PlayerState.Immobile;

            // Grab the player
            Vector3 offset = _player.PlayerHead.transform.rotation * -_player.PlayerHead.transform.localPosition;
            _player.transform.position = handPosition.position + offset;
            _player.transform.parent = handPosition;
            _player.Grounded = false;
            _grabbedObject = _player.gameObject;

            if(IsPossessed())
            {
                // Switch to the player aim routine
                attackRoutine = StartCoroutine(AimRoutine(_callback));
            }
            else
            {
                
                // Initiate throw
                attackRoutine = StartCoroutine(ThrowRoutine(_callback));
            }
        }
    }

    protected IEnumerator AimRoutine(UnityAction abilityEndedCallback)
    {
        // Swap to aim camera
        _cameraScript.TogglePlayerCamera(false);
        _aimCameraScript.SetCameraRotation(transform.eulerAngles.y, 0);
        _aimCameraScript.TogglePlayerCamera(true);

        while(!InputScript.ability1 && !InputScript.ability2)
        {
            Move();

            float targetRotationSpeed = -InputScript.move.y * rotationSpeed;
            if(_verticalRotationSpeed < targetRotationSpeed)
            {
                _verticalRotationSpeed += rotationAccel * Time.deltaTime;

                if(_verticalRotationSpeed > targetRotationSpeed)
                    _verticalRotationSpeed = targetRotationSpeed;
            }
            else if(_verticalRotationSpeed > targetRotationSpeed)
            {
                _verticalRotationSpeed -= rotationAccel * Time.deltaTime;

                if(_verticalRotationSpeed < targetRotationSpeed)
                    _verticalRotationSpeed = targetRotationSpeed;
            }

            _aimCameraScript.CameraRotation(new Vector2(_currentRotationSpeed, _verticalRotationSpeed));
            yield return null;
        }

        attackRoutine = StartCoroutine(ThrowRoutine(_callback));
    } 

    protected IEnumerator ThrowRoutine(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Throw");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Throw"));

        abilityEndedCallback.Invoke();
    } 

    public void Throw()
    {
        _grabbedObject = null;
        _player.transform.parent = null;

        // Reset the roll timer
        _player.ForceRoll();

        if(IsPossessed())
        {
            Unpossess();
            _player.SetVelocity(_mainCamera.transform.forward * possessedThrowForce);
        }
        else
        {
            _player.SetVelocity(transform.forward * horizontalThrowForce + Vector3.up * verticalThrowForce);
        }
    }

    public override void Kill()
    {
        // Don't destroy the tree, unpossess it instead
        if(IsPossessed())
            Unpossess();
    }
}
