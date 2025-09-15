using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] int attackDamage = 25;
    [SerializeField] private float speed = 60f;
    [SerializeField] private float lifeTime = 5f;

    private Vector3 direction;
    private float timeAlive = 0f;
    private float damageMultiplier = 1f;

    public void Initialize(Vector3 targetDirection, float projectileSpeed)
    {
        direction = targetDirection.normalized;
        speed = projectileSpeed;

        // Optional: Rotate projectile to face movement direction
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        // Move projectile
        transform.position += direction * speed * Time.deltaTime;

        // Update lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(Mathf.RoundToInt(attackDamage * damageMultiplier));
        }

        Destroy(gameObject);
    }
    
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }
}