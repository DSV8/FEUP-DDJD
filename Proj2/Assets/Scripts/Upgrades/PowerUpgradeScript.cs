using UnityEngine;
using TMPro;

public class PowerUpgradeScript : MonoBehaviour
{
    private enum UpgradeType
    {
        IncreasePistolDamage,
        IncreaseFireRate,
        IncreasePistolCriticalChance,
        IncreasePistolCriticalDamage,
        IncreasePistolDamageReduceHealth,
        IncreaseEMPDamage,
        IncreaseEMPRadius,
        ReduceEMPCooldown
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
            UpgradeType.IncreasePistolDamage => "Press [E] to increase max pistol damagage by a small value",
            UpgradeType.IncreaseFireRate => "Press [E] to increase fire rate",
            UpgradeType.IncreasePistolCriticalChance => "Press [E] to increase pistol critical chance",
            UpgradeType.IncreasePistolCriticalDamage => "Press [E] to increase pistol critical damage",
            UpgradeType.IncreasePistolDamageReduceHealth => "Press [E] to double pistol damage  but reduce max health in half",
            UpgradeType.IncreaseEMPDamage => "Press [E] to increase EMP damage",
            UpgradeType.IncreaseEMPRadius => "Press [E] to increase EMP Radius",
            UpgradeType.ReduceEMPCooldown => "Press [E] to reduce EMP cooldown",
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

        Pistol pistol = player.GetComponentInChildren<Pistol>();
        if (pistol == null)
        {
            Debug.LogWarning("pistol component not found under player!");
            return;
        }

        EMPAttack empAttack = player.GetComponent<EMPAttack>();
        if (empAttack == null)
        {
            Debug.LogWarning("EMPAttack component not found on player!");
            return;
        }

        FMODUnity.RuntimeManager.PlayOneShot("event:/Low Priority/Power Up/Power Up 1");


        switch (upgradeType)
        {
            case UpgradeType.IncreasePistolDamage:
                Bullet.damagePowerUp += 5;
                PlayerPrefs.SetInt("BulletDamage", Bullet.damagePowerUp);
                Debug.Log("Player Pistol Damage Increased by 5!");
                break;

            case UpgradeType.IncreaseFireRate:
                pistol.IncreaseFireRate(1.5f);
                Debug.Log("Player Pistol Fire rate Increased by 1.5!");
                break;

            case UpgradeType.IncreasePistolCriticalChance:
                Bullet.criticalChance += 5;
                PlayerPrefs.SetInt("BulletCriticalChance", Bullet.criticalChance);
                Debug.Log("Player Pistol Increased Critical Chance by 5%!");
                break;

            case UpgradeType.IncreasePistolCriticalDamage:
                Bullet.criticalDamage += 0.1f;
                PlayerPrefs.SetFloat("BulletCriticalDamage", Bullet.criticalDamage);
                Debug.Log("Player Pistol Increased Critical Damage by 10%!");
                break;

            case UpgradeType.IncreasePistolDamageReduceHealth:
                Bullet.damageMultiplier *= 2.0f;
                PlayerPrefs.SetFloat("DoublePowerReduceHealthPowerPart", Bullet.damageMultiplier);
                healthSystem.HalveMaxHealth();
                healthBar.UpdateUpgrade();
                Debug.Log("Player Pistol Damage Doubled but reduced max health in half");
                break;

            case UpgradeType.IncreaseEMPDamage:
                EMPEffect.damagePowerUp += 10;
                PlayerPrefs.SetInt("EMPPowerUPDamage", EMPEffect.damagePowerUp);
                Debug.Log("Increased EMP Damage by 10");
                break;

            case UpgradeType.IncreaseEMPRadius:
                EMPEffect.RadiusPowerUp += 2f;
                PlayerPrefs.SetFloat("EMPPowerUPRadius", EMPEffect.RadiusPowerUp);
                Debug.Log("Increased EMP Radius");
                break;

            case UpgradeType.ReduceEMPCooldown:
                empAttack.ReduceCooldown(0.4f);
                Debug.Log("Increased EMP Radius");
                break;
        }
    }
}
