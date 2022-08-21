using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public abstract class StationaryEnemyBase : EnemyBase
{
    [Header("Stationary Enemy Properties")]
    [Tooltip("The speed at which the enemy rotates in place in degrees/second")]
    [SerializeField] protected float rotationSpeed = 180f;
    [Tooltip("The maximum angle the player can be from the enemy's forward direction before attacking.")]
    [SerializeField] protected float maxAttackAngle = 20f;
    [Tooltip("The maximum distance the player can be from the enemy before attacking.")]
    [SerializeField] protected float maxAttackRange = 3f;

    protected override void Update()
    {
        base.Update();

        switch (CurrentState)
        {
            case EnemyState.Possessed:
                break;
            
            case EnemyState.Aggro: // If aggro'd
                Transform player = StarterAssetsInputs.currentPlayerObject.transform;
                Vector3 towardsPlayer = player.position - transform.position;
                
                // Look towards the player
                towardsPlayer.y = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(towardsPlayer, Vector3.up), rotationSpeed * Time.deltaTime);
                break;
        }
    }

    protected override bool CanAttack()
    {
        Transform player = StarterAssetsInputs.currentPlayerObject.transform;
        Vector3 towardsPlayer = player.position - transform.position;
        
        return base.CanAttack() && towardsPlayer.magnitude < maxAttackRange && Vector3.Angle(transform.forward, towardsPlayer) < maxAttackAngle;
    }
}
