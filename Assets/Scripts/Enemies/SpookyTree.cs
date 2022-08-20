using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;

public class SpookyTree : StationaryEnemyBase
{
    [Header("Tree Properties")]
    [Tooltip("The object to parent grabbable objects to.")]
    [SerializeField] protected Transform handPosition;

    protected GameObject grabbedObject;

    protected override IEnumerator Ability2Routine(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Ability2");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability2"));

        if(grabbedObject == null)
        {
            // If we didn't grab anything, just end the attack
            abilityEndedCallback.Invoke();
        }
        else
        {
            if(IsPossessed())
            {
                // Let the player aim
            }
            else
            {
                attackRoutine = StartCoroutine(ThrowRoutine(abilityEndedCallback));
            }
        }
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
            ThirdPersonController playerScript = hits[0].GetComponent<ThirdPersonController>();

            // Force it into a roll and grab it
            playerScript.ForceRoll();
            playerScript.transform.parent = handPosition;
            grabbedObject = playerScript.gameObject;
        }
    }

    protected IEnumerator ThrowRoutine(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Throw");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability2"));

        abilityEndedCallback.Invoke();
    } 

    public override void Kill()
    {
        // Do nothing
    }
}
