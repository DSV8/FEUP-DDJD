using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SecurityController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachThreshold = 0.1f;

    [Header("Player Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string defeatMessage = "CAUGHT BY GUARD!";
    [SerializeField] private float pathMarginX = 0.5f;
    [SerializeField] private float pathMarginY = 1.0f;

    [Header("Patrol Area Visualization")]
    [SerializeField] private SpriteRenderer patrolAreaIndicator;
    [SerializeField] private Color safeColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color dangerColor = new Color(1, 1, 0, 0.3f);
    [SerializeField] private Color caughtColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private float indicatorYOffset = -0.5f;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private Vector3 originalPosition;
    private Transform playerTransform;
    private bool playerCaught = false;
    private bool isFacingPlayer = false;

    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

        currentTarget = pointB;
        originalPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform.parent.gameObject.transform;
        }

        if (patrolAreaIndicator == null)
        {
            Debug.LogWarning("Patrol Area Indicator not assigned. Create and assign a child GameObject with a SpriteRenderer.");
        }
        else
        {
            UpdatePatrolAreaIndicator();
        }
    }

    private void Update()
    {
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || playerCaught)
        {
            rb.linearVelocity = Vector2.zero;
            if (patrolAreaIndicator != null)
            {
                patrolAreaIndicator.color = caughtColor;
            }
            return;
        }

        Patrol();
        isFacingPlayer = IsFacingPlayer();
        DetectPlayer();

        UpdatePatrolAreaColor();

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
    }

    private void Patrol()
    {
        float direction = Mathf.Sign(currentTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (Mathf.Abs(direction) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction > 0 ? 1 : -1);
            transform.localScale = scale;
        }

        if (Mathf.Abs(transform.position.x - currentTarget.position.x) <= reachThreshold)
        {
            currentTarget = (currentTarget == pointB) ? pointA : pointB;
            Flip();
        }

        if (Mathf.Abs(transform.position.y - originalPosition.y) > 0.01f)
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = originalPosition.y;
            transform.position = fixedPos;
        }
    }

    private void DetectPlayer()
    {
        if (playerTransform == null) return;

        if (playerTransform.GetComponent<PlayerInteractionController>().isHidden) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange && IsPlayerInPatrolPath() && isFacingPlayer)
        {
            CatchPlayer();
        }
    }

    private bool IsPlayerInPatrolPath()
    {
        float minX = Mathf.Min(pointA.position.x, pointB.position.x) - pathMarginX;
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x) + pathMarginX;
        float minY = Mathf.Min(pointA.position.y, pointB.position.y) - pathMarginY;
        float maxY = Mathf.Max(pointA.position.y, pointB.position.y) + pathMarginY;

        Vector3 playerPos = playerTransform.position;
        return playerPos.x >= minX && playerPos.x <= maxX && playerPos.y >= minY && playerPos.y <= maxY;
    }

    private bool IsFacingPlayer()
    {
        if (playerTransform == null) return false;
        
        bool enemyFacingRight = transform.localScale.x > 0;
        bool playerIsRight = playerTransform.position.x > transform.position.x;
        return enemyFacingRight == playerIsRight;
    }

    private void CatchPlayer()
    {
        if (playerCaught) return;

        playerCaught = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false, defeatMessage);
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void UpdatePatrolAreaIndicator()
    {
        if (patrolAreaIndicator == null || pointA == null || pointB == null) return;

        float width = Mathf.Abs(pointB.position.x - pointA.position.x) + (pathMarginX * 2);
        
        Vector3 midPoint = (pointA.position + pointB.position) / 2;
        midPoint.y += indicatorYOffset;
        patrolAreaIndicator.transform.position = midPoint;
        
        Vector3 localScale = patrolAreaIndicator.transform.localScale;
        localScale.x = width;
        patrolAreaIndicator.transform.localScale = localScale;
    }

    private void UpdatePatrolAreaColor()
    {
        if (patrolAreaIndicator == null) return;

        if (playerCaught)
        {
            patrolAreaIndicator.color = caughtColor;
        }
        else if (isFacingPlayer)
        {
            patrolAreaIndicator.color = dangerColor;
        }
        else
        {
            patrolAreaIndicator.color = safeColor;
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, reachThreshold);
            Gizmos.DrawWireSphere(pointB.position, reachThreshold);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}