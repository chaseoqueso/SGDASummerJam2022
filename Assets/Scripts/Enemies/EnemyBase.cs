using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Events;

public enum EnemyState
{
    Idle,
    Wander,
    Aggro,
    Leash,
    Attacking,
    Possessed,
    PossessedAttacking
}

public abstract class EnemyBase : MonoBehaviour
{
    private const float _threshold = 0.01f;

    [Header("General Properties")]
    [Tooltip("The range at which the enemy will begin to target the player.")]
    public float aggroRadius = 10f;
    [Tooltip("The range at which the enemy will stop targeting the player.")]
    public float leashRadius = 20f;
    
    [Header("Attack Properties")]
    [Tooltip("The minimum time between enemy attacks.")]
    public float minAttackDelay = 1f;
    [Tooltip("The maximum time between enemy attacks.")]
    public float maxAttackDelay = 2f;
    [Tooltip("The likelihood that this enemy will use Ability 1 rather than Ability 2.")]
    public float abilityRatio = 0.5f;

    [Header("Player Input")]
    [Tooltip("The script that provides the movement inputs to this controller.")]
    public StarterAssetsInputs InputScript;

    [HideInInspector] public EnemyState CurrentState;

    protected CameraManager _cameraScript;
    protected GameObject _mainCamera;
    protected Animator _animator;
    protected Coroutine attackRoutine;
    protected float attackTimer;

    protected virtual void Awake()
    {
        _cameraScript = GetComponent<CameraManager>();
        _animator = GetComponentInChildren<Animator>();
        
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    protected virtual void Update()
    {
        UpdateAttackTimer();
        
        if(CanAggro()) // If we are in a state that can transition to aggro and the player is within aggro radius
        {
            TransitionState(CurrentState, EnemyState.Aggro); // transition to aggro state.
        }
        
        if(CurrentState == EnemyState.Aggro) // Otherwise, if the enemy is aggro'd 
        {
            if(ShouldLeash()) // and is outside of leash radius, 
            {
                TransitionState(CurrentState, EnemyState.Leash); // transition to leash state.
            }
            else if(CanAttack()) // Otherwise, if the enemy can attack,
            {
                if(TransitionState(CurrentState, EnemyState.Attacking)) // attempt to transition state to Attacking, and if successful, 
                {
                    StartAttack(); // Start an attack
                }
            }
        }

        if(CurrentState == EnemyState.Possessed) // If this enemy is possessed
        {
            if(InputScript.ability1 && TransitionState(CurrentState, EnemyState.PossessedAttacking)) // and the player is pressing ability 1, try to transition, and if successful, 
            {
                InputScript.ability1 = false;
                attackRoutine = StartCoroutine(UseAbility1(AttackEnded)); // use the attack.
            }
            else if(InputScript.ability2 && TransitionState(CurrentState, EnemyState.PossessedAttacking))// If the player is pressing ability 2, try to transition, and if successful, 
            {
                InputScript.ability2 = false;
                attackRoutine = StartCoroutine(UseAbility2(AttackEnded)); // use the attack.
            }
        }
    }

    protected virtual void UpdateAttackTimer()
    {
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            if(attackTimer < 0)
            {
                attackTimer = 0;
            }
        }
    }

    protected virtual bool CanAggro()
    {
        return (CurrentState == EnemyState.Idle || CurrentState == EnemyState.Wander) && Vector3.Distance(StarterAssetsInputs.currentPlayerObject.transform.position, transform.position) < aggroRadius;
    }

    protected virtual bool ShouldLeash()
    {
        return Vector3.Distance(StarterAssetsInputs.currentPlayerObject.transform.position, transform.position) > leashRadius;
    }

    protected virtual bool CanAttack()
    {
        return CurrentState == EnemyState.Aggro && attackTimer <= 0;
    }

    protected virtual bool IsPossessed()
    {
        return (CurrentState == EnemyState.Possessed || CurrentState == EnemyState.PossessedAttacking);
    }

    protected virtual void StartAttack()
    {
        if(Random.value < abilityRatio) // roll a random number and choose either ability 1
        {
            attackRoutine = StartCoroutine(UseAbility1(AttackEnded));
        }
        else // or ability 2.
        {
            attackRoutine = StartCoroutine(UseAbility2(AttackEnded));
        }
    }

    protected virtual void AttackEnded()
    {
        if(IsPossessed()) // If the enemy is currently possessed,
        {
            TransitionState(CurrentState, EnemyState.Possessed); // enter the normal possessed state.
        }
        else
        {
            TransitionState(CurrentState, EnemyState.Aggro); // Otherwise enter the aggro state.
            attackTimer = Random.Range(minAttackDelay, maxAttackDelay);
        }

        attackRoutine = null;
    }

    // <summary> Attempts to transition to a new state and returns whether it was successful. </summary>
    // <returns> Whether the transition was successful. <\returns>
    protected virtual bool TransitionState(EnemyState previousState, EnemyState newState)
    {
        CurrentState = newState;
        return true;
    }

    protected virtual IEnumerator UseAbility1(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Ability1");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability1"));

        abilityEndedCallback.Invoke();
    }

    protected virtual IEnumerator UseAbility2(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Ability2");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability2"));

        abilityEndedCallback.Invoke();
    }
}
