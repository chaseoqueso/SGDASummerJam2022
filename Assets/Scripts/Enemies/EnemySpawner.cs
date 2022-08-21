using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Tooltip("The enemy to spawn.")]
    public GameObject EnemyPrefab;
    [Tooltip("The transform to parent a newly spawned enemy to before unparenting it and enabling its AI.")]
    public Transform EnemyCreationParent;
    [Tooltip("A list of locations to spawn enemies for.")]
    public List<Transform> SpawnPoints;
    [Tooltip("The delay before spawning a replacement enemy.")]
    public float SpawnDelay = 2;
    [Tooltip("A reference to the animator on this spawner.")]
    public Animator Animator;

    private List<MobileEnemyBase> _enemies;
    private MobileEnemyBase _currentEnemy;
    private float _spawnDelayTimer;
    private bool _isSpawning;
    private bool _canSpawn;

    void Start()
    {
        _enemies = new List<MobileEnemyBase>();
        _canSpawn = true;

        if(Animator == null)
            Animator = GetComponent<Animator>();

        // Fill the enemy list with dummy data
        foreach(var _ in SpawnPoints)
            _enemies.Add(null);
    }

    void Update()
    {
        if(!_isSpawning)
        {
            for(int i = 0; i < _enemies.Count; i++)
            {
                if(_enemies[i] == null)
                {
                    if(_canSpawn)
                    {
                        StartCoroutine(SpawnRoutine(i));
                    }
                    else
                    {
                        if(_spawnDelayTimer > 0)
                        {
                            _spawnDelayTimer -= Time.deltaTime;
                        }
                        else
                        {
                            _canSpawn = true;
                        }
                    }
                    break;
                }
            }
        }
    }

    private IEnumerator SpawnRoutine(int enemyIndex)
    {
        _isSpawning = true;
        
        _currentEnemy = Instantiate(EnemyPrefab, EnemyCreationParent.transform.position, transform.rotation, EnemyCreationParent).GetComponent<MobileEnemyBase>();
        _currentEnemy.Initialize(SpawnPoints[enemyIndex], this);
        _currentEnemy.enabled = false;
        _enemies[enemyIndex] = _currentEnemy;

        Animator.SetTrigger("SpawnEnemy");

        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && !Animator.GetBool("SpawnEnemy"));

        _isSpawning = false;
        _canSpawn = false;
        _spawnDelayTimer = SpawnDelay;
    }

    public void EnableEnemy()
    {
        _currentEnemy.transform.parent = null;
        _currentEnemy.enabled = true;
    }

    public void RemoveEnemy(MobileEnemyBase enemy)
    {
        int index = _enemies.IndexOf(enemy);

        if(index == -1)
        {
            Debug.LogError("Tried to remove an enemy that wasn't in this spawner's enemy list.");
        }
        else
        {
            _enemies[index] = null;
        }
    }
}
