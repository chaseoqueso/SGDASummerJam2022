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
    [Tooltip("A BoxCollider to reference when simulating the ability 1 hitbox.")]
    [SerializeField] protected BoxCollider Ability1ReferenceHitbox;
    [Tooltip("A BoxCollider to reference when simulating the ability 2 hitbox.")]
    [SerializeField] protected BoxCollider Ability2ReferenceHitbox;
    [Tooltip("The minimum time between enemy attacks.")]
    public float minAttackDelay = 1f;
    [Tooltip("The maximum time between enemy attacks.")]
    public float maxAttackDelay = 2f;
    [Tooltip("The likelihood that this enemy will use Ability 1 rather than Ability 2.")]
    public float abilityRatio = 0.5f;

    [Header("Player Input")]
    [Tooltip("The script that provides the movement inputs to this controller.")]
    public StarterAssetsInputs InputScript;
    [Tooltip("The script that manages the followPoint of the virtual camera.")]
    [SerializeField] protected CameraManager _cameraScript;

    [HideInInspector] public EnemyState CurrentState;
    protected GameObject _mainCamera;
    protected ThirdPersonController _player;
    protected Animator _animator;
    protected Coroutine attackRoutine;
    protected float attackTimer;
    protected int lastAttackUsed;

    public virtual void TriggerAbility2() {}

    protected virtual void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        if(_cameraScript == null)
        {
            _cameraScript = GetComponent<CameraManager>();
        }
        
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
                attackRoutine = StartCoroutine(Ability1Routine(AttackEnded)); // use the attack.
            }
            else if(InputScript.ability2 && TransitionState(CurrentState, EnemyState.PossessedAttacking))// If the player is pressing ability 2, try to transition, and if successful, 
            {
                InputScript.ability2 = false;
                attackRoutine = StartCoroutine(Ability2Routine(AttackEnded)); // use the attack.
            }
            else if(InputScript.roll)
            {
                InputScript.roll = false;
                Kill();
            }
        }
    }

    protected virtual void LateUpdate()
    {
        if(IsPossessed())
        {
            _cameraScript.CameraRotation(InputScript.look);
        }
        else
        {
            _cameraScript.SetCameraRotation(transform.eulerAngles.y, transform.eulerAngles.x);
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
        float randomValue = Random.value;

        // Reroll if we roll the same attack as last time
        if(lastAttackUsed == 1 && randomValue < abilityRatio || lastAttackUsed == 2 && randomValue >= abilityRatio)
            randomValue = Random.value;

        if(randomValue < abilityRatio) // roll a random number and choose either ability 1
        {
            attackRoutine = StartCoroutine(Ability1Routine(AttackEnded));
            lastAttackUsed = 1;
        }
        else // or ability 2.
        {
            attackRoutine = StartCoroutine(Ability2Routine(AttackEnded));
            lastAttackUsed = 2;
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

    protected virtual IEnumerator Ability1Routine(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Ability1");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability1"));

        abilityEndedCallback.Invoke();
    }

    protected virtual IEnumerator Ability2Routine(UnityAction abilityEndedCallback)
    {
        _animator.SetTrigger("Ability2");

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !_animator.GetBool("Ability2"));

        abilityEndedCallback.Invoke();
    }

    public virtual void TriggerAbility1()
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

            if(hitObject.layer == LayerMask.NameToLayer("Player") && hitObject == StarterAssetsInputs.currentPlayerObject) // If one of the hit objects was the player
            {
                Possess(StarterAssetsInputs.currentPlayerObject.GetComponent<ThirdPersonController>()); // Get possessed
            }
            else if(hitObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyBase enemyScript = hitObject.GetComponent<EnemyBase>();
                if(hitObject.TryGetComponent<EnemyBase>(out enemyScript))
                {
                    enemyScript.Kill();
                }
                else
                {
                    Debug.LogError("An object with the Enemy layer did not have a script that inherits from EnemyBase.");
                }
            }
            else if(hitObject.layer == LayerMask.NameToLayer("Interactible"))
            {
                IInteractible interactScript;
                if(hitObject.TryGetComponent<IInteractible>(out interactScript))
                {
                    interactScript.OnInteract(gameObject);
                }
                else
                {
                    Debug.LogError("An object with the Interactible layer did not have a script that inherits from IInteractible.");
                }
            }
        }
    }

    protected virtual void Possess(ThirdPersonController playerScript)
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
        _player = playerScript;
        playerScript.TogglePlayerControl(false);

        // Reroute input to this script
        StarterAssetsInputs.currentPlayerObject = gameObject;
        
        InputScript = playerScript.InputScript;
        InputScript.canUseAbilities = true;

        // Set this as the player's camera
        _cameraScript.TogglePlayerCamera(true);
    }

    protected virtual void Unpossess()
    {
        // Set player camera direction
        Vector3 cameraEulers = _mainCamera.transform.eulerAngles;
        _player._cameraScript.SetCameraRotation(cameraEulers.y, 20);

        // Give the player control
        _player.TogglePlayerControl(true);

        // Update the current player
        StarterAssetsInputs.currentPlayerObject = _player.gameObject;
        InputScript.canUseAbilities = false;

        // Reset enemy
        TransitionState(CurrentState, EnemyState.Idle);
        _cameraScript.TogglePlayerCamera(false);
    }

    public virtual void Kill()
    {
        if(IsPossessed())
            Unpossess();

        // This for now
        Destroy(gameObject);
    }
}
