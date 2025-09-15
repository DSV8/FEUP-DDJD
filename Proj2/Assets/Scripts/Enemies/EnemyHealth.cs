using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] int maxHealth = 100;

    [Header("Power-Up Settings")]
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject medkitPrefab;
    [SerializeField] GameObject[] powerUpPrefabs;
    [SerializeField] float dropChance = 0.4f;
    
    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent OnEnemyDied;
    
    int currentHealth;
    bool isDead = false;
    
    BaseWalkingEnemy walkingEnemy;
    FlyingEnemy flyingEnemy;
    DamageFlashEffect flashEffect; // Add reference to flash effect
    
    void Awake()
    {
        walkingEnemy = GetComponent<BaseWalkingEnemy>();
        flyingEnemy = GetComponent<FlyingEnemy>();
        flashEffect = GetComponent<DamageFlashEffect>(); // Get flash effect component
        currentHealth = maxHealth;
    }
    
    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamageTaken?.Invoke(damage);

            // Trigger flash effect when damage is taken
            if (flashEffect != null)
            {
                flashEffect.TriggerFlash();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                TriggerHitReaction();
            }
        }
    }
    
    void TriggerHitReaction()
    {
        if (walkingEnemy != null)
        {
            walkingEnemy.TriggerHitReaction();
        }
        else if (flyingEnemy != null)
        {
            flyingEnemy.TriggerHitReaction();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnEnemyDied?.Invoke();

        if (walkingEnemy != null)
        {
            walkingEnemy.TriggerDeath();
            print("Die");
        }
        else if (flyingEnemy != null)
        {
            flyingEnemy.TriggerDeath();
        }

        // Increase the enemies player killed in stats
        PlayerRunStats.killedEnemies += 1;

        TrySpawnPowerUp();
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (currentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    void TrySpawnPowerUp()
    {
        if (powerUpPrefabs.Length == 0) return;

        if (Random.value <= dropChance)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 2.0f;

            float roll = Random.Range(0f, 100f);

            if (roll < 5f)
            {
                if (coinPrefab != null)
                    Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            }
            else if (roll < 10f)
            {
                if (medkitPrefab != null)
                    Instantiate(medkitPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                if (powerUpPrefabs.Length == 3)
                {
                    float powerUpRoll = roll - 10f;
                    int index = Mathf.FloorToInt(powerUpRoll / 30f);

                    Instantiate(powerUpPrefabs[index], spawnPosition, Quaternion.identity);
                }
            }
        }
    }

    
    public bool IsAlive => !isDead;
    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
}