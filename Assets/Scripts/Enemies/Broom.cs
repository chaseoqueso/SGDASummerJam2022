using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Events;

public class Broom : MobileEnemyBase
{
    [Header("Broom Properties")]
    [Tooltip("A BoxCollider to reference when simulating the ability 1 hitbox.")]
    [SerializeField] private BoxCollider Ability1ReferenceHitbox;
    [Tooltip("A BoxCollider to reference when simulating the ability 2 hitbox.")]
    [SerializeField] private BoxCollider Ability2ReferenceHitbox;
    [Tooltip("The strength of horizontal knockback when hitting the player.")]
    [SerializeField] private float KnockbackForce = 10f;
    [Tooltip("The strength of vertical knockback when hitting the player.")]
    [SerializeField] private float KnockupForce = 5f;
    [Tooltip("The strength of horizontal knockback when hitting things while possessed.")]
    [SerializeField] private float PossessedKnockbackForce = 10f;
    [Tooltip("The strength of vertical knockback when hitting things while possessed.")]
    [SerializeField] private float PossessedKnockupForce = 5f;

    public override void TriggerAbility1()
    {
        // Find all players and enemies in the hitbox
        List<Collider> hits = new List<Collider>(Physics.OverlapBox(Ability1ReferenceHitbox.transform.position + Ability1ReferenceHitbox.transform.rotation * Ability1ReferenceHitbox.center, 
                                                                    Ability1ReferenceHitbox.size, 
                                                                    Ability1ReferenceHitbox.transform.rotation, 
                                                                    LayerMask.GetMask("Player", "Enemy", "Interactible"), 
                                                                    QueryTriggerInteraction.Collide));

        List<GameObject> hitObjects = hits.ConvertAll<GameObject>((Collider c) => c.gameObject);
        foreach(GameObject hitObject in hitObjects)
        {
            if(hitObject == gameObject) // Don't do anything if the enemy hits itself
            {
                continue;
            }

            if(hitObject.layer == LayerMask.NameToLayer("Player")) // If one of the hit objects was the player
            {
                Possess(StarterAssetsInputs.currentPlayerObject.GetComponent<ThirdPersonController>()); // Get possessed
            }
            else if(hitObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // Kill any enemies
            }
            else if(hitObject.layer == LayerMask.NameToLayer("Interactible"))
            {
                IInteractible interactScript = hitObject.GetComponent<IInteractible>();
                if(interactScript == null)
                {
                    Debug.LogError("An object with the Interactible layer did not have a script that inherits from IInteractible.");
                }
                else
                {
                    interactScript.OnInteract(gameObject);
                }
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

            // Force it into a roll and knock it away
            playerScript.ForceRoll();
            float currentKnockbackForce = IsPossessed() ? PossessedKnockbackForce : KnockbackForce;
            float currentKnockupForce = IsPossessed() ? PossessedKnockupForce : KnockupForce;
            playerScript.AddVelocity(transform.forward * currentKnockbackForce + Vector3.up * currentKnockupForce);
        }
    }
}
