using System.Collections;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float attackMoveSpeed = 20f;
    [SerializeField] private float rotationSpeed = 7.5f;
    [SerializeField] private float playerFacingSpeed = 15f;
    [SerializeField] private float circleDuration = 10f;
    [SerializeField] private float minPauseTime = 1f;
    [SerializeField] private float maxPauseTime = 2f;

    [Header("Patrol Area Settings")]
    [SerializeField] private float patrolLength = 50f;
    [SerializeField] private float patrolWidth = 50f;
    [SerializeField] private float patrolHeight = 20f;
    [SerializeField] private int numberOfPatrolPoints = 50;
    [SerializeField] private float waypointDistanceThreshold = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 50f;
    [SerializeField] private float attackDistance = 10f;
    [SerializeField] private int shotsToFire = 5;
    [SerializeField] private float shootInterval = 0.5f;
    [SerializeField] private float tauntDuration = 1.5f;
    [SerializeField] private float postAttackPause = 0.5f;
    [SerializeField] private LayerMask obstacleLayerMask = -1;

    [Header("Predictive Targeting")]
    [SerializeField] private bool usePredictiveTargeting = true;
    [SerializeField, Range(0f, 1f)] private float predictionAccuracy = 0.7f;
    [SerializeField] private float maxPredictionTime = 2f;
    [SerializeField] private float aimInaccuracyDegrees = 5f;
    [SerializeField] private bool showPredictionGizmos = false;

    [Header("Movement Settings")]
    [SerializeField] private float bobbingAmplitude = 0.25f;
    [SerializeField] private float bobbingSpeed = 10f;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 60f;

    [Header("Animation Settings")]
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Hit Reaction Settings")]
    [SerializeField] private float hitStunDuration = 0.3f;
    [SerializeField] private float hitKnockbackForce = 2f;

    public enum PatrolPattern
    {
        Random
    }

    private Vector3 spawnPosition;
    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 currentTarget;
    private Vector3 lastPosition;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isInAttackMode = false;
    private Vector3 waypointToReturnTo;
    private float bobbingOffset;
    private Vector3 basePosition;

    private float sqrAttackRange;
    private float sqrAttackDistance;
    private float sqrWaypointThreshold;
    private float sqrMovementThreshold;

    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;
    private Vector3 lastPredictedPosition;

    private EnemyHealth enemyHealth;

    private float damageMultiplier = 1f;

    private static readonly string PARAM_IS_FLYING = "IsFlying";
    private static readonly string PARAM_IS_ATTACKING = "IsAttacking";
    private static readonly string PARAM_SHOOT_TRIGGER = "Shoot";
    private static readonly string PARAM_HIT_TRIGGER = "Hit";
    private static readonly string PARAM_DEATH_TRIGGER = "Death";
    private static readonly string PARAM_TAUNT_TRIGGER = "Taunt";

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        
        if (enemyHealth != null)
        {
            enemyHealth.OnDamageTaken.AddListener(OnDamageTaken);
            enemyHealth.OnEnemyDied.AddListener(OnEnemyDied);
        }
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                player = playerGO.transform;
        }

        spawnPosition = transform.position;
        basePosition = transform.position;
        bobbingOffset = Random.Range(0f, Mathf.PI * 2f);
        GeneratePatrolArea();

        lastPosition = transform.position;
        
        sqrAttackRange = attackRange * attackRange;
        sqrAttackDistance = attackDistance * attackDistance;
        sqrWaypointThreshold = waypointDistanceThreshold * waypointDistanceThreshold;
        sqrMovementThreshold = movementThreshold * movementThreshold;

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }

        PickNextPatrolPoint();

        StartCoroutine(StateMachine());
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDamageTaken.RemoveListener(OnDamageTaken);
            enemyHealth.OnEnemyDied.RemoveListener(OnEnemyDied);
        }
    }

    private void Update()
    {
        if (isDead) return;

        ApplyBobbing();
        UpdatePlayerVelocity();
        
        if (isInAttackMode && !isStunned)
        {
            FacePlayer();
        }
        
        UpdateMovementAnimation();
    }

    private void UpdatePlayerVelocity()
    {
        if (player == null) return;

        Vector3 currentPlayerPosition = player.position;
        playerVelocity = (currentPlayerPosition - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = currentPlayerPosition;
    }

    private Vector3 CalculatePredictedPlayerPosition()
    {
        if (!player || !usePredictiveTargeting) 
            return player ? player.position : Vector3.zero;

        Vector3 shooterPosition = projectileSpawnPoint ? projectileSpawnPoint.position : transform.position;
        Vector3 currentPlayerPosition = player.position;
        
        float distanceToPlayer = Vector3.Distance(shooterPosition, currentPlayerPosition);
        float timeToReach = distanceToPlayer / projectileSpeed;
        
        timeToReach = Mathf.Min(timeToReach, maxPredictionTime);
        
        Vector3 predictedPosition = currentPlayerPosition + (playerVelocity * timeToReach);
        
        Vector3 finalPrediction = Vector3.Lerp(currentPlayerPosition, predictedPosition, predictionAccuracy);
        
        if (aimInaccuracyDegrees > 0f)
        {
            Vector3 directionToTarget = (finalPrediction - shooterPosition).normalized;
            
            Vector3 randomOffset = Random.insideUnitSphere * Mathf.Tan(aimInaccuracyDegrees * Mathf.Deg2Rad);
            randomOffset = Vector3.ProjectOnPlane(randomOffset, directionToTarget);
            
            float distanceToPrediction = Vector3.Distance(shooterPosition, finalPrediction);
            finalPrediction += randomOffset * distanceToPrediction;
        }
        
        lastPredictedPosition = finalPrediction;
        
        return finalPrediction;
    }

    private void GeneratePatrolArea()
    {
        patrolPoints = new Vector3[numberOfPatrolPoints];
        GenerateRandomPatrolPoints();
    }

    private void GenerateRandomPatrolPoints()
    {
        float halfLength = patrolLength * 0.5f;
        float halfWidth = patrolWidth * 0.5f;
        
        for (int i = 0; i < numberOfPatrolPoints; i++)
        {
            Vector3 randomPoint = new Vector3(
                spawnPosition.x + Random.Range(-halfLength, halfLength),
                spawnPosition.y + Random.Range(0f, patrolHeight),
                spawnPosition.z + Random.Range(-halfWidth, halfWidth)
            );
            patrolPoints[i] = randomPoint;
        }
    }

    private void ApplyBobbing()
    {
        float bobbingY = Mathf.Sin((Time.time * bobbingSpeed) + bobbingOffset) * bobbingAmplitude;
        transform.position = new Vector3(basePosition.x, basePosition.y + bobbingY, basePosition.z);
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null) return;

        float sqrMovementDistance = (transform.position - lastPosition).sqrMagnitude;
        bool isMoving = sqrMovementDistance > sqrMovementThreshold * Time.deltaTime * Time.deltaTime;
        
        animator.SetBool(PARAM_IS_FLYING, isMoving);
        lastPosition = transform.position;
    }

    private void FacePlayer()
    {
        if (player == null || isDead || isStunned) return;
        
        Vector3 lookDir = (player.position - basePosition).normalized;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * playerFacingSpeed);
        }
    }

    private IEnumerator StateMachine()
    {
        while (!isDead)
        {
            if (!isStunned)
            {
                yield return StartCoroutine(CircleState(circleDuration));
                if (!isDead && !isStunned && CanAttackPlayer())
                {
                    waypointToReturnTo = currentTarget;
                    yield return StartCoroutine(AttackState());
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private bool CanAttackPlayer()
    {
        if (!player || isStunned) return false;
        
        float sqrDistanceToPlayer = SqrDistanceToPlayer();
        if (sqrDistanceToPlayer > sqrAttackRange) return false;
        
        return HasLineOfSight();
    }

    private bool HasLineOfSight()
    {
        if (!player) return false;
        
        Vector3 playerPosition = player.position;
        Vector3 directionToPlayer = (playerPosition - basePosition).normalized;
        float distanceToPlayer = Vector3.Distance(basePosition, playerPosition);
        
        RaycastHit hit;
        if (Physics.Raycast(basePosition, directionToPlayer, out hit, distanceToPlayer, obstacleLayerMask))
        {
            return hit.transform == player;
        }
        
        return true;
    }

    private IEnumerator CircleState(float duration)
    {
        if (animator != null)
            animator.SetBool(PARAM_IS_ATTACKING, false);

        isInAttackMode = false;

        float timer = 0;
        bool isPausing = false;

        while (timer < duration && !isDead && !isStunned)
        {
            timer += Time.deltaTime;
            
            if (!isPausing)
            {
                MoveTowardsTarget(currentTarget, moveSpeed);

                if (ReachedTarget())
                {
                    isPausing = true;
                    yield return StartCoroutine(PauseAtWaypoint());
                    isPausing = false;
                    PickNextPatrolPoint();
                }
            }

            yield return null;
        }
    }

    private IEnumerator PauseAtWaypoint()
    {
        float pauseDuration = Random.Range(minPauseTime, maxPauseTime);
        yield return new WaitForSeconds(pauseDuration);
    }

    private void PickNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        currentTarget = patrolPoints[currentPatrolIndex];
    }

    private bool ReachedTarget()
    {
        return (basePosition - currentTarget).sqrMagnitude < sqrWaypointThreshold;
    }

    private IEnumerator AttackState()
    {
        if (animator != null)
            animator.SetBool(PARAM_IS_ATTACKING, true);

        isInAttackMode = true;

        yield return StartCoroutine(RushToAttackDistance());
        
        if (!isDead && !isStunned)
        {
            if (animator != null)
                animator.SetTrigger(PARAM_TAUNT_TRIGGER);
            yield return new WaitForSeconds(tauntDuration);
            
            for (int i = 0; i < shotsToFire && !isDead && !isStunned; i++)
            {
                yield return StartCoroutine(FireProjectileWithAnimation());
                if (i < shotsToFire - 1)
                {
                    yield return new WaitForSeconds(shootInterval);
                }
            }
        }
        
        if (animator != null)
            animator.SetBool(PARAM_IS_ATTACKING, false);
    }

    private IEnumerator RushToAttackDistance()
    {
        while (SqrDistanceToPlayer() > sqrAttackDistance && !isDead && !isStunned)
        {
            if (player != null)
                MoveTowardsTarget(player.position, attackMoveSpeed);
            yield return null;
        }
    }

    private IEnumerator ReturnToWaypoint()
    {
        yield return StartCoroutine(ReturnToUprightPosition());
        
        while ((basePosition - waypointToReturnTo).sqrMagnitude > sqrWaypointThreshold && !isDead && !isStunned)
        {
            MoveTowardsTarget(waypointToReturnTo, attackMoveSpeed);
            yield return null;
        }
        
        currentTarget = waypointToReturnTo;
    }

    private IEnumerator ReturnToUprightPosition()
    {
        Quaternion uprightRotation = Quaternion.identity;
        while (Quaternion.Angle(transform.rotation, uprightRotation) > 1f && !isDead && !isStunned)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, uprightRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }

    private IEnumerator FireProjectileWithAnimation()
    {
        if (animator != null)
            animator.SetTrigger(PARAM_SHOOT_TRIGGER);

        yield return new WaitForSeconds(0.1f);
        FireProjectile();
    }

    private void FireProjectile()
    {
        if (!projectilePrefab || !player) return;
        
        var spawn = projectileSpawnPoint ? projectileSpawnPoint : transform;
        Vector3 targetPosition = CalculatePredictedPlayerPosition();
        Vector3 directionToTarget = (targetPosition - spawn.position).normalized;
        
        var proj = Instantiate(projectilePrefab, spawn.position, spawn.rotation);
        var projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(directionToTarget, projectileSpeed);
            projectile.SetDamageMultiplier(damageMultiplier);
        }
    }

    private float SqrDistanceToPlayer()
    {
        if (!player) return float.MaxValue;
        return (basePosition - player.position).sqrMagnitude;
    }

    private void MoveTowardsTarget(Vector3 targetPos, float speed)
    {
        if (isStunned) return;

        Vector3 dir = targetPos - basePosition;
        if (dir.sqrMagnitude < 0.0001f) return;

        if (!isInAttackMode)
        {
            dir.y = 0;
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * rotationSpeed
                );
            }
        }
        
        basePosition = Vector3.MoveTowards(basePosition, targetPos, speed * Time.deltaTime);
    }

    private void OnDamageTaken(int damage)
    {
        if (isDead) return;
        TriggerHitReaction();
    }

    private void OnEnemyDied()
    {
        TriggerDeath();
    }

    public void TriggerHitReaction()
    {
        if (isDead || isInAttackMode) return;
        
        StartCoroutine(HitReactionCoroutine());
    }

    private IEnumerator HitReactionCoroutine()
    {
        if (animator != null)
            animator.SetTrigger(PARAM_HIT_TRIGGER);

        Vector3 knockbackDirection = -transform.forward;
        Vector3 originalPosition = basePosition;
        Vector3 knockbackTarget = originalPosition + knockbackDirection * hitKnockbackForce;

        isStunned = true;

        float knockbackTime = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < knockbackTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / knockbackTime;
            
            float easedProgress = 1f - (1f - progress) * (1f - progress);
            basePosition = Vector3.Lerp(originalPosition, knockbackTarget, easedProgress);
            
            yield return null;
        }

        yield return new WaitForSeconds(hitStunDuration - knockbackTime);

        isStunned = false;
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        
        Die();
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isStunned = false;
        
        if (animator != null)
        {
            animator.SetTrigger(PARAM_DEATH_TRIGGER);
            animator.SetBool(PARAM_IS_FLYING, false);
            animator.SetBool(PARAM_IS_ATTACKING, false);
        }

        StopAllCoroutines();
        StartCoroutine(DestroyAfterDeathAnimation());
    }

    private IEnumerator DestroyAfterDeathAnimation()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void TakeDamage()
    {
        isInAttackMode = true;
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(10);
        }
        else
        {
            TriggerHitReaction();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Vector3 size = new Vector3(patrolLength, patrolHeight, patrolWidth);
        Vector3 boxCenter = center + Vector3.up * (patrolHeight * 0.5f);
        Gizmos.DrawWireCube(boxCenter, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, attackDistance);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector3 point in patrolPoints)
            {
                Gizmos.DrawWireSphere(point, 1f);
            }

            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentTarget, 1.5f);
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(waypointToReturnTo, 1.2f);
            }
        }
        
        if (Application.isPlaying && player != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.green : Color.red;
            Gizmos.DrawLine(basePosition, player.position);
            
            if (showPredictionGizmos && usePredictiveTargeting)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(player.position, 0.5f);
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(lastPredictedPosition, 0.7f);
                
                Vector3 shootPos = projectileSpawnPoint ? projectileSpawnPoint.position : transform.position;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(shootPos, lastPredictedPosition);
                
                if (playerVelocity.magnitude > 0.1f)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(player.position, playerVelocity.normalized * 3f);
                }
            }
        }
    }
}