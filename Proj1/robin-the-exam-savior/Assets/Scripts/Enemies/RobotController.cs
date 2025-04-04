using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;
    private Vector2 startPosition;
    private bool movingRight = true;

    [Header("Detection Settings")]
    public float detectionRadius = 5f;
    public LayerMask playerLayer;

    [Header("Chase Settings")]
    public float chaseSpeed = 3f;

    [Header("Enemy Parts")]
    public GameObject head;

    private bool isPlayerDetected = false;
    private Transform player;
    private Rigidbody2D rb;
    private GameManager gameManager;

    [Header("Effects Settings")]
    [SerializeField] private GameObject blockade;
    [SerializeField] private float blockadeFadeDuration = 1.0f;

    // Track the direction of last movement during chase
    private bool wasMovingRightDuringChase = true;

    private void Start()
    {
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        bool currentPlayerDetection = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        
        if (currentPlayerDetection)
        {
            isPlayerDetected = true;
            ChasePlayer();
        }
        else if (isPlayerDetected)
        {
            // Player has just escaped detection
            isPlayerDetected = false;
            // Resume patrol in the direction of last chase movement
            movingRight = wasMovingRightDuringChase;
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        // Ensure the robot is facing the correct direction while patrolling
        Vector3 localScale = transform.localScale;
        localScale.x = movingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        if (movingRight)
        {
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);
            if (transform.position.x >= startPosition.x + patrolDistance)
            {
                movingRight = false;
            }
        }
        else
        {
            transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime);
            if (transform.position.x <= startPosition.x - patrolDistance)
            {
                movingRight = true;
            }
        }
    }

    private void ChasePlayer()
    {
        // Update the chase direction
        wasMovingRightDuringChase = player.position.x > transform.position.x;

        // Face the player
        Vector3 localScale = transform.localScale;
        localScale.x = wasMovingRightDuringChase ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        // Move towards the player
        transform.position = Vector2.MoveTowards(
            transform.position, 
            player.position, 
            chaseSpeed * Time.deltaTime
        );
    }

    public void Die()
    {
        if (blockade != null)
        {
            StartCoroutine(FadeOutBlockadeAndDeactivate());
        }
        else
        {
            // If no blockade, just deactivate the robot
            gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutBlockadeAndDeactivate()
    {
        if (blockade == null) yield break;

        SpriteRenderer blockadeRenderer = blockade.GetComponent<SpriteRenderer>();
        
        if (blockadeRenderer != null)
        {
            Color originalColor = blockadeRenderer.color;
            float elapsedTime = 0f;
            
            while (elapsedTime < blockadeFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / blockadeFadeDuration;
                
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(originalColor.a, 0f, normalizedTime);
                blockadeRenderer.color = newColor;
                
                yield return null;
            }

            // Destroy the blockade
            Destroy(blockade);
        }

        // Deactivate the robot after blockade is destroyed
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameManager.GameOver(false, "The robot caught you!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(
                new Vector3(startPosition.x - patrolDistance, startPosition.y, 0),
                new Vector3(startPosition.x + patrolDistance, startPosition.y, 0)
            );
        }
        else
        {
            Gizmos.DrawLine(
                new Vector3(transform.position.x - patrolDistance, transform.position.y, 0),
                new Vector3(transform.position.x + patrolDistance, transform.position.y, 0)
            );
        }
    }
}