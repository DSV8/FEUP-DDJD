using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EMPEffect : MonoBehaviour
{
    public Material rayMaterial;
    public int rayCount = 50;
    public float rayLength = 5f;
    public float rayWidth = 0.02f;
    public float duration = 0.5f;
    public GameObject sphereVisual;

    public float effectRadius = 5f;
    public int baseDamage = 15;
    public static int damagePowerUp = 0;
    public static float RadiusPowerUp = 0f;
    public LayerMask enemyLayers;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private Material dynamicMaterial;
    private GameObject sphereInstance;

    private void Start()
    {

        effectRadius += RadiusPowerUp + PlayerPrefs.GetFloat("EMPPowerUPRadius", 0f);

        FMODUnity.RuntimeManager.PlayOneShot("event:/Medium Priority/Choque elétrico (escudo)");
        
        dynamicMaterial = new Material(rayMaterial);
        
        sphereInstance = Instantiate(sphereVisual, transform.position, Quaternion.identity, transform);
        sphereInstance.transform.localScale = Vector3.one * effectRadius * 2f;

        for (int i = 0; i < rayCount; i++)
        {
            CreateRay();
        }

        DamageEnemiesInRadius();
        StartCoroutine(FadeAndDestroy());
    }

    void CreateRay()
    {
        GameObject rayObj = new GameObject("EMPRay");
        rayObj.transform.parent = transform;
        rayObj.transform.localPosition = Vector3.zero;

        LineRenderer lr = rayObj.AddComponent<LineRenderer>();
        lr.material = dynamicMaterial;
        lr.positionCount = 2;
        lr.useWorldSpace = false;

        Vector3 direction = Random.onUnitSphere;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, direction * rayLength);

        lr.startWidth = rayWidth;
        lr.endWidth = 0f;

        lineRenderers.Add(lr);
    }

    void DamageEnemiesInRadius()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, effectRadius, enemyLayers);

        foreach (var hit in hits)
        {
            int damageAmount = baseDamage + damagePowerUp + PlayerPrefs.GetInt("EMPPowerUPDamage", 0);
            FinnTheFrogBoss bossController = hit.GetComponent<FinnTheFrogBoss>();
            if (bossController != null)
            {
                bossController.TakeDamage(damageAmount);
                Debug.Log("EMP hit boss: " + damageAmount);
                continue;
            }

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                enemyHealth = hit.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
                Debug.Log("EMP hit enemy: " + damageAmount);
            }

            PlayerRunStats.playerTotalDmg += damageAmount;
        }
    }

    IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        Color startColor = dynamicMaterial.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            Color faded = new Color(startColor.r, startColor.g, startColor.b, alpha);
            dynamicMaterial.color = faded;

            if (sphereInstance.TryGetComponent<Renderer>(out var sphereRenderer))
            {
                Material sphereMat = sphereRenderer.material;
                Color sphereColor = sphereMat.color;
                sphereColor.a = alpha;
                sphereMat.color = sphereColor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
