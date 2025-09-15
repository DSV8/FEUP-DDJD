using UnityEngine;
using TMPro;

public class LifeUpgradeScript : MonoBehaviour
{
    private enum UpgradeType
    {
        IncreaseHealth,
        IncreaseHealingSpeed,
        IncreaseMedkitHeal,
        IncreaseMaxHealthReduceMovementSpeed,
        HealOnEnemyHit
    }

    public Canvas upgradeCanvas;
    public TextMeshProUGUI upgradeText;

    private UpgradeType upgradeType;
    private bool playerInRange = false;

    void Start()
    {
        upgradeType = (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length);

        if (upgradeText != null)
        {
            upgradeText.text = GetUpgradeDescription();
        }

        if (upgradeCanvas != null)
        {
            upgradeCanvas.enabled = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ApplyUpgrade();
            upgradeCanvas.enabled = false;
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (upgradeCanvas != null)
                upgradeCanvas.enabled = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (upgradeCanvas != null)
                upgradeCanvas.enabled = false;
        }
    }

    string GetUpgradeDescription()
    {
        return upgradeType switch
        {
            UpgradeType.IncreaseHealth => "Press [E] to increase max health",
            UpgradeType.IncreaseHealingSpeed => "Press [E] to recover a percentage of health each second",
            UpgradeType.IncreaseMedkitHeal => "Press [E] to increase recovered health from medkits",
            UpgradeType.IncreaseMaxHealthReduceMovementSpeed => "Press [E] to double max health but reduce movement speed in half",
            UpgradeType.HealOnEnemyHit => "Press [E] to heal on enemy pistol hit",
            _ => "Press E for upgrade"
        };
    }

    void ApplyUpgrade()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found!");
            return;
        }

        PlayerHealth healthSystem = player.GetComponent<PlayerHealth>();
        if (healthSystem == null)
        {
            Debug.LogWarning("HealthSystem component not found on player!");
            return;
        }
        
        HealthBar healthBar = player.GetComponentInChildren<HealthBar>();
        if (healthBar == null)
        {
            Debug.LogWarning("HealthBar component not found under player!");
            return;
        }

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement component not found on player!");
            return;
        }

        FMODUnity.RuntimeManager.PlayOneShot("event:/Low Priority/Power Up/Power Up 2");

        switch (upgradeType)
        {
            case UpgradeType.IncreaseHealth:
                healthSystem.IncreaseMaxHealth(40);
                healthBar.UpdateUpgrade();
                Debug.Log("Player health increased by 40!");
                break;

            case UpgradeType.IncreaseMedkitHeal:
                HealthItem.healAmount += 30;
                PlayerPrefs.SetInt("medHeal",  HealthItem.healAmount);
                Debug.Log("Medkit heals 30 more health!");
                break;

            case UpgradeType.IncreaseHealingSpeed:
                healthSystem.IncreaseSelfHeal(1);
                Debug.Log("Player heals per second!");
                break;

            case UpgradeType.IncreaseMaxHealthReduceMovementSpeed:
                healthSystem.DoubleMaxHealth();
                healthBar.UpdateUpgrade();
                playerMovement.ReduceInHalfMovementSpeed();
                Debug.Log("Player health doubled but halved movement speed");
                break;

            case UpgradeType.HealOnEnemyHit:
                Bullet.healingFactorOnHit += 1;
                PlayerPrefs.SetInt("BulletHealing", Bullet.healingFactorOnHit);
                Debug.Log("Heal +1 on Enenmy Hit");
                break;
        }
}
}
