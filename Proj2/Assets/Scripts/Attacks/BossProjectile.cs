using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private LayerMask playerLayer = -1;
    
    private Vector3 direction;
    private float speed;
    private int damage;
    private bool isExplosive;
    private float explosionRadius;
    private Rigidbody rb;
    private bool hasHit = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector3 direction, float speed, int damage, bool isExplosive, float explosionRadius = 0f)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.isExplosive = isExplosive;
        this.explosionRadius = explosionRadius;
        
        if (rb != null)
        {
            rb.linearVelocity = this.direction * speed;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (isExplosive)
                {
                    Explode();
                }
                else
                {
                    playerHealth.TakeDamage(damage);
                    Hit();
                }
            }
        }
        else if (!other.isTrigger)
        {
            if (isExplosive)
            {
                Explode();
            }
            else
            {
                Hit();
            }
        }
    }
    
    void Hit()
    {
        hasHit = true;
        Destroy(gameObject);
    }
    
    void Explode()
    {
        hasHit = true;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);
        
        foreach (Collider hitCollider in hitColliders)
        {
            PlayerMovement playerMovement = hitCollider.GetComponent<PlayerMovement>();
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();

            if (playerMovement != null && playerHealth != null)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                int finalDamage = (int)(damage * damageMultiplier);

                playerHealth.TakeDamage(finalDamage);

                Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                playerMovement.ApplyKnockback(knockbackDirection * (10f * damageMultiplier));
            }
        }
        
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ExplosionEffect explosionEffect = effect.GetComponent<ExplosionEffect>();
            if (explosionEffect != null)
            {
                explosionEffect.SetRadius(explosionRadius);
            }
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        if (isExplosive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}