using UnityEngine;

public class BossDifficultyScaler : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private FinnTheFrogBoss boss;
    
    [Header("Scaling Settings")]
    [SerializeField] private float healthScalePerLevel = 0.2f; // 20% more health per difficulty level
    [SerializeField] private float damageScalePerLevel = 0.15f; // 15% more damage per difficulty level
    [SerializeField] private float speedScalePerLevel = 0.05f; // 5% faster per difficulty level
    [SerializeField] private float cooldownReductionPerLevel = 0.03f; // 3% faster attacks per level
    
    private void Start()
    {
        if (boss == null)
            boss = GetComponent<FinnTheFrogBoss>();
            
        ApplyDifficultyScaling();
    }
    
    private void ApplyDifficultyScaling()
    {
        DifficultyData difficultyData = GetStoredDifficultyData();
        
        if (difficultyData == null || difficultyData.difficultyLevel <= 1)
            return;
            
        float levelMultiplier = difficultyData.difficultyLevel - 1;
        
        ScaleBossHealth(levelMultiplier);
        ScaleBossDamage(levelMultiplier);
        ScaleBossSpeed(levelMultiplier);
        ScaleBossCooldowns(levelMultiplier);
        
        Debug.Log($"Boss scaled to difficulty level {difficultyData.difficultyLevel}");
    }
    
    private void ScaleBossHealth(float levelMultiplier)
    {
        if (boss == null) return;
        
        float healthMultiplier = 1f + (healthScalePerLevel * levelMultiplier);
        
        boss.SetHealthMultiplier(healthMultiplier);
    }
    
    private void ScaleBossDamage(float levelMultiplier)
    {
        if (boss == null) return;
        
        float damageMultiplier = 1f + (damageScalePerLevel * levelMultiplier);
        
        boss.SetDamageMultiplier(damageMultiplier);
    }
    
    private void ScaleBossSpeed(float levelMultiplier)
    {
        if (boss == null) return;
        
        float speedMultiplier = 1f + (speedScalePerLevel * levelMultiplier);
        
        boss.SetSpeedMultiplier(speedMultiplier);
    }
    
    private void ScaleBossCooldowns(float levelMultiplier)
    {
        if (boss == null) return;
        
        float cooldownMultiplier = 1f - (cooldownReductionPerLevel * levelMultiplier);
        cooldownMultiplier = Mathf.Max(0.3f, cooldownMultiplier); // Cap at 70% reduction
        
        boss.SetCooldownMultiplier(cooldownMultiplier);
    }
    
    private DifficultyData GetStoredDifficultyData()
    {
        return DifficultyPersistence.LoadDifficultyData();
    }
}

public static class DifficultyPersistence
{
    public static void SaveDifficultyData(DifficultyData data)
    {
        PlayerPrefs.SetFloat("DifficultyLevel", data.difficultyLevel);
        PlayerPrefs.SetFloat("HealthMultiplier", data.healthMultiplier);
        PlayerPrefs.SetFloat("DamageMultiplier", data.damageMultiplier);
        PlayerPrefs.SetFloat("TimeInGame", Time.time);
        PlayerPrefs.Save();
    }
    
    public static DifficultyData LoadDifficultyData()
    {
        if (PlayerPrefs.HasKey("DifficultyLevel"))
        {
            return new DifficultyData
            {
                difficultyLevel = PlayerPrefs.GetFloat("DifficultyLevel"),
                healthMultiplier = PlayerPrefs.GetFloat("HealthMultiplier"),
                damageMultiplier = PlayerPrefs.GetFloat("DamageMultiplier")
            };
        }
        return null;
    }
    
    public static void ClearDifficultyData()
    {
        PlayerPrefs.DeleteKey("DifficultyLevel");
        PlayerPrefs.DeleteKey("HealthMultiplier");
        PlayerPrefs.DeleteKey("DamageMultiplier");
        PlayerPrefs.DeleteKey("TimeInGame");
        PlayerPrefs.Save();
    }
}