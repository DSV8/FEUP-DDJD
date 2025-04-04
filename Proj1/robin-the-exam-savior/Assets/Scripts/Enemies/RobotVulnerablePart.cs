using UnityEngine;

public class RobotPart : MonoBehaviour
{
    [Header("Visual Feedback")]
    public float flashRate = 1.5f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1.0f;
    public Color highlightColor = new Color(1f, 0.7f, 0.7f, 1f);

    private RobotController enemyController;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float timer;

    private void Start()
    {
        enemyController = GetComponentInParent<RobotController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(
                highlightColor.r, 
                highlightColor.g, 
                highlightColor.b, 
                originalColor.a
            );
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on vulnerable part. Flashing effect won't work.");
        }
    }

    private void Update()
    {
        if (spriteRenderer != null)
        {
            timer += Time.deltaTime * flashRate * Mathf.PI;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(timer) + 1) / 2);
            
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject);
            
            enemyController.Die();
        }
    }
}