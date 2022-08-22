using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public abstract class StationaryEnemyBase : EnemyBase
{
    [Header("Stationary Enemy Properties")]
    [Tooltip("The speed at which the enemy rotates in place in degrees/second")]
    [SerializeField] protected float rotationSpeed = 90f;
    [Tooltip("The speed at which the enemy's rotation accelerates in degrees/second^2")]
    [SerializeField] protected float rotationAccel = 360f;
    [Tooltip("The maximum angle the player can be from the enemy's forward direction before attacking.")]
    [SerializeField] protected float maxAttackAngle = 20f;
    [Tooltip("The maximum distance the player can be from the enemy before attacking.")]
    [SerializeField] protected float maxAttackRange = 3f;

    protected float _currentRotationSpeed;

    protected override void Update()
    {
        base.Update();

        switch (CurrentState)
        {
            case EnemyState.Possessed:
                Move();
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

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if(IsPossessed())
        {
            _cameraScript.CameraRotation(new Vector2(_currentRotationSpeed, 0));
        }
    }

    protected virtual void Move()
    {
        float targetRotationSpeed = InputScript.move.x * rotationSpeed;
        if(_currentRotationSpeed < targetRotationSpeed)
        {
            _currentRotationSpeed += rotationAccel * Time.deltaTime;

            if(_currentRotationSpeed > targetRotationSpeed)
                _currentRotationSpeed = targetRotationSpeed;
        }
        else if(_currentRotationSpeed > targetRotationSpeed)
        {
            _currentRotationSpeed -= rotationAccel * Time.deltaTime;
            
            if(_currentRotationSpeed < targetRotationSpeed)
                _currentRotationSpeed = targetRotationSpeed;
        }

        transform.Rotate(new Vector3(0, _currentRotationSpeed * Time.deltaTime, 0));
    }

    protected override bool CanAttack()
    {
        Transform player = StarterAssetsInputs.currentPlayerObject.transform;
        Vector3 towardsPlayer = player.position - transform.position;

        return base.CanAttack() && towardsPlayer.magnitude < maxAttackRange && Vector3.Angle(transform.forward, towardsPlayer) < maxAttackAngle;
    }

    protected override bool TransitionState(EnemyState currentState, EnemyState newState)
    {
        if(newState == EnemyState.Leash)
            return base.TransitionState(currentState, EnemyState.Idle); 
        
        return base.TransitionState(currentState, newState); 
    }
}
