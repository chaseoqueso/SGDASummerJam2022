using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

public abstract class MobileEnemyBase : EnemyBase
{
    [Header("Movement Properties")]
    [Tooltip("The speed at which the enemy moves while aggroed.")]
    public float aggroSpeed = 10f;
    [Tooltip("The speed at which the enemy moves while leashed.")]
    public float leashSpeed = 10f;
    [Tooltip("The speed at which the enemy moves while wandering.")]
    public float wanderSpeed = 4f;
    
    [Header("Mobile Attack Properties")]
    [Tooltip("The range that the player will need to move away from the enemy before it tries to reposition.")]
    public float maxAttackRange = 3f;
    [Tooltip("The range that the enemy will try to reach before attacking the player.")]
    public float idealAttackRange = 1.5f;

    [Header("Wander Properties")]
    [Tooltip("The range at which the enemy will wander around its spawnpoint.")]
    public float wanderRadius = 2f;
    [Tooltip("The minimum frequency with which the enemy will choose a new wander destination.")]
    public float minWanderFrequency = 2f;
    [Tooltip("The maximum frequency with which the enemy will choose a new wander destination.")]
    public float maxWanderFrequency = 4f;
    [Tooltip("The minimum distance at which the enemy will choose a new wander destination.")]
    public float minWanderPointDistance = 2f;
    [Tooltip("The maximum distance at which the enemy will choose a new wander destination.")]
    public float maxWanderPointDistance = 4f;

    [Header("Player Controls")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.125f;
    [Tooltip("The radius of the grounded check. Should generally match the radius of the CharacterController")]
    public float GroundedRadius = 0.25f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    protected CharacterController _controller;
    protected NavMeshAgent _agent;
    protected Coroutine _wanderRoutine;
    protected Vector3 _horizontalSpeed;
    
    // player
    protected float _targetRotation = 0.0f;
    protected float _rotationVelocity;
    protected float _verticalVelocity;
    protected float _terminalVelocity = 53.0f;

    protected override void Awake()
    {
        base.Awake();

        // The default state for mobile enemies is to wander
        CurrentState = EnemyState.Wander;

        _agent = GetComponent<NavMeshAgent>();
        _controller = GetComponent<CharacterController>();
        _controller.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if(CurrentState == EnemyState.Possessed)
        {
            GroundedCheck();
            Move();
        }
        else if(CurrentState == EnemyState.Aggro)
        {
            _agent.SetDestination(StarterAssetsInputs.currentPlayerObject.transform.position);
        }
        else if(CurrentState == EnemyState.Attacking)
        {
            _agent.SetDestination(transform.position);
        }
    }

    void LateUpdate()
    {
        if(CurrentState == EnemyState.Possessed)
        {
            _cameraScript.CameraRotation(InputScript.look);
        }
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    protected void Move()
    {
        // set target speed based on move speed
        float targetSpeed = MoveSpeed;

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (InputScript.move == Vector2.zero || CurrentState == EnemyState.PossessedAttacking) 
        {
            targetSpeed = 0.0f;
        }

        float inputMagnitude = InputScript.analogMovement ? InputScript.move.magnitude : 1f;

        // move towards target input direction and magnitude
        Vector3 input = new Vector3(InputScript.move.x, 0.0f, InputScript.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (InputScript.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothSpeed);

            // rotate to face input direction relative to camera position
            // transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // accelerate or decelerate to target speed based on how much movement we have left
        _horizontalSpeed = Vector3.MoveTowards(_horizontalSpeed, targetSpeed * targetDirection, SpeedChangeRate * Time.deltaTime);

        // move the player
        _controller.Move(_horizontalSpeed * Time.deltaTime + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // if the player is moving, face the player in the direction they're moving
        if(_horizontalSpeed != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_horizontalSpeed, Vector3.up);
        }

        // // update animator if using character
        // if (_hasAnimator)
        // {
        //     _animator.SetFloat(_animIDSpeed, _animationBlend);
        //     _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        // }
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && Vector3.Distance(StarterAssetsInputs.currentPlayerObject.transform.position, transform.position) < maxAttackRange;
    }
}
