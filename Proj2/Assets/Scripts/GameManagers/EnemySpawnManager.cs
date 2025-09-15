using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject weaponSlashEnemyPrefab;
    [SerializeField] private GameObject groundPoundEnemyPrefab;
    [SerializeField] private GameObject flyingEnemyPrefab;
    
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Base Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 3f;
    [SerializeField] private float baseSpawnDistance = 20f;
    [SerializeField] private float minSpawnDistance = 8f;
    [SerializeField] private int baseEnemiesPerSpawn = 1;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyIncreaseInterval = 60f;
    [SerializeField] private float spawnIntervalDecreaseRate = 0.9f;
    [SerializeField] private float spawnDistanceDecreaseRate = 0.95f;
    [SerializeField] private float enemiesPerSpawnIncreaseRate = 0.3f;
    [SerializeField] private float healthMultiplierPerMinute = 0.15f;
    [SerializeField] private float damageMultiplierPerMinute = 0.1f;
    
    [Header("Spawn Limits")]
    [SerializeField] private int maxEnemiesAlive = 20;
    [SerializeField] private float minSpawnInterval = 0.5f;
    
    private float currentDifficultyLevel = 1f;
    private float currentSpawnInterval;
    private float currentSpawnDistance;
    private float currentEnemiesPerSpawn;
    private float currentHealthMultiplier = 1f;
    private float currentDamageMultiplier = 1f;
    
    private float timeSinceLastDifficultyIncrease = 0f;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    public static event System.Action<float> OnDifficultyChanged;
    public static event System.Action<float, float> OnDifficultyStatsChanged;
    
    private void Start()
    {
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                player = playerGO.transform;
        }

        InitializeFromSavedDifficulty();
        
        if (currentDifficultyLevel == 1f)
        {
            currentSpawnInterval = baseSpawnInterval;
            currentSpawnDistance = baseSpawnDistance;
            currentEnemiesPerSpawn = baseEnemiesPerSpawn;
        }
        
        currentSpawnInterval = baseSpawnInterval;
        currentSpawnDistance = baseSpawnDistance;
        currentEnemiesPerSpawn = baseEnemiesPerSpawn;
        
        StartCoroutine(SpawnLoop());
    }
    
    private void InitializeFromSavedDifficulty()
    {
        if (PlayerPrefs.HasKey("DifficultyLevel"))
        {
            currentDifficultyLevel = PlayerPrefs.GetFloat("DifficultyLevel");
            currentHealthMultiplier = PlayerPrefs.GetFloat("HealthMultiplier");
            currentDamageMultiplier = PlayerPrefs.GetFloat("DamageMultiplier");
            
            float minutesPassed = currentDifficultyLevel - 1;
            
            currentSpawnInterval = baseSpawnInterval;
            currentSpawnDistance = baseSpawnDistance;
            currentEnemiesPerSpawn = baseEnemiesPerSpawn;
            
            for (int i = 0; i < minutesPassed; i++)
            {
                currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval * spawnIntervalDecreaseRate);
                currentSpawnDistance = Mathf.Max(minSpawnDistance, currentSpawnDistance * spawnDistanceDecreaseRate);
                currentEnemiesPerSpawn += enemiesPerSpawnIncreaseRate;
            }
            
            Debug.Log($"Initialized spawn manager with difficulty level {currentDifficultyLevel}");
            
            OnDifficultyChanged?.Invoke(currentDifficultyLevel);
            OnDifficultyStatsChanged?.Invoke(currentHealthMultiplier, currentDamageMultiplier);
        }
    }
    
    private void Update()
    {
        timeSinceLastDifficultyIncrease += Time.deltaTime;

        if (timeSinceLastDifficultyIncrease >= difficultyIncreaseInterval)
        {
            IncreaseDifficulty();
            timeSinceLastDifficultyIncrease = 0f;
        }

        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    private void IncreaseDifficulty()
    {
        currentDifficultyLevel++;
        
        currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval * spawnIntervalDecreaseRate);
        currentSpawnDistance = Mathf.Max(minSpawnDistance, currentSpawnDistance * spawnDistanceDecreaseRate);
        currentEnemiesPerSpawn += enemiesPerSpawnIncreaseRate;
        
        currentHealthMultiplier = 1f + (healthMultiplierPerMinute * (currentDifficultyLevel - 1));
        currentDamageMultiplier = 1f + (damageMultiplierPerMinute * (currentDifficultyLevel - 1));
        
        OnDifficultyChanged?.Invoke(currentDifficultyLevel);
        OnDifficultyStatsChanged?.Invoke(currentHealthMultiplier, currentDamageMultiplier);
        
        Debug.Log($"Difficulty increased to level {currentDifficultyLevel}");
    }
    
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            
            if (activeEnemies.Count < maxEnemiesAlive && player != null)
            {
                SpawnEnemyWave();
            }
        }
    }
    
    private void SpawnEnemyWave()
    {
        int enemiesToSpawn = Mathf.RoundToInt(currentEnemiesPerSpawn);
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (activeEnemies.Count >= maxEnemiesAlive)
                break;
                
            SpawnRandomEnemy();
        }
    }
    
    private void SpawnRandomEnemy()
    {
        List<(GameObject prefab, bool isFlying)> availableEnemies = new List<(GameObject, bool)>();
        
        if (weaponSlashEnemyPrefab != null)
            availableEnemies.Add((weaponSlashEnemyPrefab, false));
        if (groundPoundEnemyPrefab != null)
            availableEnemies.Add((groundPoundEnemyPrefab, false));
        if (flyingEnemyPrefab != null)
            availableEnemies.Add((flyingEnemyPrefab, true));
        
        if (availableEnemies.Count == 0) return;
        
        int randomIndex = Random.Range(0, availableEnemies.Count);
        GameObject enemyPrefab = availableEnemies[randomIndex].prefab;
        bool isFlying = availableEnemies[randomIndex].isFlying;
        
        Vector3 spawnPosition = GetSpawnPosition(isFlying);
        if (spawnPosition == Vector3.zero) return;
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);
        
        ApplyDifficultyModifiers(enemy, isFlying);
    }
    
    private Vector3 GetSpawnPosition(bool isFlying)
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 30;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 randomPoint = player.position + new Vector3(randomDirection.x, 0, randomDirection.y) * currentSpawnDistance;
            
            if (isFlying)
            {
                spawnPosition = new Vector3(randomPoint.x, player.position.y + 10f, randomPoint.z);
                return spawnPosition;
            }
            else
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    spawnPosition = hit.position;
                    return spawnPosition;
                }
            }
        }
        
        Debug.LogWarning("Failed to find valid spawn position");
        return Vector3.zero;
    }
    
    private void ApplyDifficultyModifiers(GameObject enemy, bool isFlying)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            int currentMaxHealth = Mathf.RoundToInt(health.MaxHealth * currentHealthMultiplier);
            health.SetMaxHealth(currentMaxHealth);
        }
        
        if (isFlying)
        {
            FlyingEnemy flyingEnemy = enemy.GetComponent<FlyingEnemy>();
            if (flyingEnemy != null)
            {
                flyingEnemy.SetDamageMultiplier(currentDamageMultiplier);
            }
        }
        else
        {
            BaseWalkingEnemy walkingEnemy = enemy.GetComponent<BaseWalkingEnemy>();
            if (walkingEnemy != null)
            {
                if (enemy.GetComponent<WeaponSlashEnemy>() != null)
                {
                    WeaponSlashEnemy slashEnemy = enemy.GetComponent<WeaponSlashEnemy>();
                    slashEnemy.SetDamageMultiplier(currentDamageMultiplier);
                }
                else if (enemy.GetComponent<GroundPoundEnemy>() != null)
                {
                    GroundPoundEnemy poundEnemy = enemy.GetComponent<GroundPoundEnemy>();
                    poundEnemy.SetDamageMultiplier(currentDamageMultiplier);
                }
            }
        }
    }
    
    public static DifficultyData GetCurrentDifficultyData()
    {
        EnemySpawnManager instance = FindFirstObjectByType<EnemySpawnManager>();
        if (instance != null)
        {
            return new DifficultyData
            {
                difficultyLevel = instance.currentDifficultyLevel,
                healthMultiplier = instance.currentHealthMultiplier,
                damageMultiplier = instance.currentDamageMultiplier
            };
        }
        return new DifficultyData { difficultyLevel = 1, healthMultiplier = 1, damageMultiplier = 1 };
    }
}

[System.Serializable]
public class DifficultyData
{
    public float difficultyLevel;
    public float healthMultiplier;
    public float damageMultiplier;
}