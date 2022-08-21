using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Events;

public class Broom : MobileEnemyBase
{
    [Header("Broom Properties")]
    [Tooltip("The strength of horizontal knockback when hitting the player.")]
    [SerializeField] private float KnockbackForce = 10f;
    [Tooltip("The strength of vertical knockback when hitting the player.")]
    [SerializeField] private float KnockupForce = 5f;
    [Tooltip("The strength of horizontal knockback when hitting things while possessed.")]
    [SerializeField] private float PossessedKnockbackForce = 10f;
    [Tooltip("The strength of vertical knockback when hitting things while possessed.")]
    [SerializeField] private float PossessedKnockupForce = 5f;

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

            // Force it into a roll and knock it away
            playerScript.ForceRoll();
            float currentKnockbackForce = IsPossessed() ? PossessedKnockbackForce : KnockbackForce;
            float currentKnockupForce = IsPossessed() ? PossessedKnockupForce : KnockupForce;
            playerScript.AddVelocity(transform.forward * currentKnockbackForce + Vector3.up * currentKnockupForce);
        }
    }
}
