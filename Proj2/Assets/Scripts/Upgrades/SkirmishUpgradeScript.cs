using UnityEngine;
using TMPro;

public class SkirmishUpgradeScript : MonoBehaviour
{
    private enum UpgradeType
    {
        IncreaseMovementSpeed,
        IncreasePlayerJumpNumber,
        IncreaseAirSpeed,
        ImproveDashCooldown,
        ImproveDashDistance,
        IncreaseMaxSpeedReducePlayerDamage
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
            UpgradeType.IncreaseMovementSpeed => "Press [E] to increase movement speed",
            UpgradeType.IncreasePlayerJumpNumber => "Press [E] to get an extra jump",
            UpgradeType.IncreaseAirSpeed => "Press [E] to increase air maneuverability",
            UpgradeType.ImproveDashCooldown => "Press [E] to reduce dash cooldown",
            UpgradeType.ImproveDashDistance => "Press [E] to  increase dash distance",
            UpgradeType.IncreaseMaxSpeedReducePlayerDamage => "Press [E] to double movement spedd but reduce pistol damage in half",
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

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement component not found on player!");
            return;
        }

        FMODUnity.RuntimeManager.PlayOneShot("event:/Low Priority/Power Up/Power Up 3");

        switch (upgradeType)
        {
            case UpgradeType.IncreaseMovementSpeed:
                playerMovement.IncreaseMovementSpeed(0.5f);
                Debug.Log("Player movement speed increased by 0.5!");
                break;

            case UpgradeType.IncreasePlayerJumpNumber:
                playerMovement.IncreaseJumpNumber(1);
                Debug.Log("Player number of jumps increased by 1");
                break;

            case UpgradeType.IncreaseAirSpeed:
                playerMovement.IncreaseAirSpeed(0.3f);
                Debug.Log("Player maneuverability in air increased by 0.3");
                break;

            case UpgradeType.ImproveDashCooldown:
                playerMovement.ReduceDashCooldown(0.1f);
                Debug.Log("Player dash cooldown reduced by 0.1");
                break;

            case UpgradeType.ImproveDashDistance:
                playerMovement.IncreaseDashDistance(3f);
                Debug.Log("Player dash speed increased by big 3");
                break;

            case UpgradeType.IncreaseMaxSpeedReducePlayerDamage:
                playerMovement.DoubleMovementSpeed();
                Bullet.damageMultiplier /= 2.0f;
                PlayerPrefs.SetFloat("DoublePowerReduceHealthPowerPart", Bullet.damageMultiplier);
                Debug.Log("Player movement speed doubled but halved pistol damage");
                break;
        }
}
}
