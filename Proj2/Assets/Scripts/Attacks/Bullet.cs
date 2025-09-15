using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 200f;
    public int damageAmount = 20;
    public static int damagePowerUp = 0;
    public static float damageMultiplier = 1;

    public LayerMask playerLayer;

    // Critical hit chance
    public TrailRenderer trailRenderer;
    public static int criticalChance = 5;
    public static float criticalDamage = 0.50f;
    public static int healingFactorOnHit = 0;
    private bool isCriticalHit = false;

    private void Start()
    {
        healingFactorOnHit = PlayerPrefs.GetInt("BulletHealing", 0);

        criticalDamage = PlayerPrefs.GetFloat("BulletCriticalDamage", 0f);
        criticalChance = PlayerPrefs.GetInt("BulletCriticalChance", 0);

        // Increase damage by menu
        int DamageLevel = PlayerPrefs.GetInt("DamageLevel", 1);

        damageAmount += 10 * (DamageLevel - 1) + PlayerPrefs.GetInt("BulletDamage", 0);

        isCriticalHit = Random.Range(0, 100) <= criticalChance;

        if (isCriticalHit)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.red, 0.0f),
                    new GradientColorKey(Color.red, 0.1f),
                    new GradientColorKey(Color.white, 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0.0f),
                    new GradientAlphaKey(0.1f, 0.6f),
                    new GradientAlphaKey(0f, 1.0f)
                }
            );

            trailRenderer.colorGradient = gradient;
        }

        Destroy(gameObject, 5f);
    }

    public void ResetPowerUpBullet()
    {
        damagePowerUp = 0;
        damageMultiplier = 1;
        criticalChance = 5;
        criticalDamage = 0.5f;
    }

    void CheckHealingOnHit()
    {
        if (healingFactorOnHit > 0)
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

            healthSystem.Heal(healingFactorOnHit);
        }
    }

    private void Update()
    {
        RaycastHit hit;
        if (speed != 0 && Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime, ~playerLayer))
        {
            // Increase damage by power up
            damageAmount += damagePowerUp;


            // Increase damage by powerup that multiplies damage but reduces health in max;
            damageAmount = (int)(damageAmount * (damageMultiplier + PlayerPrefs.GetFloat("DoublePowerReduceHealthPowerPart", 0f)));

            // Increase damage by critical hit
            if (isCriticalHit)
            {
                damageAmount += (int)(damageAmount * criticalDamage);
            }

            FinnTheFrogBoss bossController = hit.transform.GetComponent<FinnTheFrogBoss>();
            if (bossController != null)
            {
                bossController.TakeDamage(damageAmount);
                Debug.Log("Dealt " + damageAmount + " damage to " + bossController.gameObject.name);
                CheckHealingOnHit();
            }
            else
            {
                EnemyHealth regularEnemyHealth = hit.transform.GetComponent<EnemyHealth>();

                // If not found, try to get it from the parent
                if (regularEnemyHealth == null)
                {
                    regularEnemyHealth = hit.transform.GetComponentInParent<EnemyHealth>();
                }

                if (regularEnemyHealth != null)
                {
                    regularEnemyHealth.TakeDamage(damageAmount);
                    Debug.Log("Dealt " + damageAmount + " damage to " + regularEnemyHealth.gameObject.name);
                    CheckHealingOnHit();
                }
            }

            // Increase the total damage in player stats
            PlayerRunStats.playerTotalDmg += damageAmount;

            // Basically destroys the bullet without deleting the trail right away
            speed = 0;
        }

        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
