using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Events;

public class Broom : MobileEnemyBase
{
    [Tooltip("A BoxCollider to reference when simulating the ability 1 hitbox.")]
    [SerializeField] private BoxCollider Ability1ReferenceHitbox;
    [Tooltip("A BoxCollider to reference when simulating the ability 2 hitbox.")]
    [SerializeField] private BoxCollider Ability2ReferenceHitbox;

    public override void TriggerAbility1()
    {
        // Find all players and enemies in the hitbox
        List<Collider> hits = new List<Collider>(Physics.OverlapBox(Ability1ReferenceHitbox.transform.position + Ability1ReferenceHitbox.transform.rotation * Ability1ReferenceHitbox.center, 
                                                                    Ability1ReferenceHitbox.size, 
                                                                    Ability1ReferenceHitbox.transform.rotation, 
                                                                    LayerMask.GetMask("Player", "Enemy"), 
                                                                    QueryTriggerInteraction.Collide));

        List<GameObject> hitObjects = hits.ConvertAll<GameObject>((Collider c) => c.gameObject);
        foreach(GameObject hitObject in hitObjects)
        {
            Debug.Log(hitObject);
            if(hitObject == gameObject) // Don't do anything if the enemy hits itself
            {
                continue;
            }

            if(hitObject == StarterAssetsInputs.currentPlayerObject) // If one of the hit objects was the player
            {
                Possess(StarterAssetsInputs.currentPlayerObject.GetComponent<ThirdPersonController>()); // Get possessed
            }
            else
            {
                // Kill any enemies and interact with any objects
            }
        }
    }
}
