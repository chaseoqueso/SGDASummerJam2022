using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Events;

public class Broom : MobileEnemyBase
{
    void OnTriggerEnter(Collider other)
    {
        // If the other collider belongs to the player
        if(!IsPossessed() && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Possess this object
            if(CurrentState == EnemyState.Attacking)
            {
                TransitionState(CurrentState, EnemyState.PossessedAttacking);
            }
            else
            {
                TransitionState(CurrentState, EnemyState.Possessed);
            }

            // Incapacitate the player
            ThirdPersonController playerScript = other.GetComponent<ThirdPersonController>();
            playerScript.IncapacitatePlayer();

            // Reroute input to this script
			StarterAssetsInputs.currentPlayerObject = gameObject;
            
            InputScript = playerScript.InputScript;
            InputScript.canUseAbilities = true;

            _controller.enabled = true;
            _agent.enabled = false;

            // Set this as the player's camera
            _cameraScript.TogglePlayerCamera(true);
        }
    }
}
